import ApiService from './api.js';
import AuthService from './auth.js';
import Utils from './utils.js';

// Check authentication
AuthService.checkAuth();

// State
let currentPage = 1;
let totalPages = 1;
let currentSearchTerm = null;

// Elements
const userInfo = document.getElementById('userInfo');
const welcomeMessage = document.getElementById('welcomeMessage');
const logoutBtn = document.getElementById('logoutBtn');
const uploadBtn = document.getElementById('uploadBtn');
const searchInput = document.getElementById('searchInput');
const searchBtn = document.getElementById('searchBtn');
const clearSearchBtn = document.getElementById('clearSearchBtn');
const documentsTableBody = document.getElementById('documentsTableBody');
const documentsTable = document.getElementById('documentsTable');
const loadingState = document.getElementById('loadingState');
const emptyState = document.getElementById('emptyState');
const pagination = document.getElementById('pagination');
const prevPageBtn = document.getElementById('prevPageBtn');
const nextPageBtn = document.getElementById('nextPageBtn');
const pageInfo = document.getElementById('pageInfo');

// Initialize
function init() {
    const user = AuthService.getUser();
    if (user) {
        userInfo.textContent = `${user.name} (${user.role})`;
        welcomeMessage.textContent = `Welcome, ${user.name}!`;
    }

    loadDocuments();
}

// Load documents
async function loadDocuments(page = 1, searchTerm = null) {
    try {
        showLoading();

        const response = await ApiService.getDocuments(page, 20, searchTerm);

        currentPage = response.pageNumber;
        totalPages = response.totalPages;
        currentSearchTerm = searchTerm;

        renderDocuments(response.items);
        updatePagination();

    } catch (error) {
        Utils.showToast(error.message || 'Failed to load documents', 'error');
        showEmpty();
    }
}

// Render documents
function renderDocuments(documents) {
    if (!documents || documents.length === 0) {
        showEmpty();
        return;
    }

    documentsTableBody.innerHTML = '';

    documents.forEach(doc => {
        const row = createDocumentRow(doc);
        documentsTableBody.appendChild(row);
    });

    hideLoading();
    documentsTable.style.display = 'table';
    emptyState.style.display = 'none';
}

// Create document row
function createDocumentRow(doc) {
    const tr = document.createElement('tr');

    tr.innerHTML = `
        <td>
            <strong>${escapeHtml(doc.title)}</strong>
            ${doc.description ? `<br><small style="color: #64748b;">${escapeHtml(doc.description.substring(0, 100))}${doc.description.length > 100 ? '...' : ''}</small>` : ''}
        </td>
        <td>${escapeHtml(doc.uploadedBy)}</td>
        <td>${Utils.formatDate(doc.createdAt)}</td>
        <td><span class="badge badge-${doc.accessType.toLowerCase()}">${doc.accessType}</span></td>
        <td>
            ${doc.tags.map(tag => `<span class="tag">${escapeHtml(tag)}</span>`).join(' ')}
        </td>
        <td class="actions">
            <button class="btn btn-sm btn-secondary" onclick="viewDocument(${doc.id})">View</button>
            <button class="btn btn-sm btn-secondary" onclick="downloadDocument(${doc.id}, '${escapeHtml(doc.fileName)}')">Download</button>
            <button class="btn btn-sm btn-danger" onclick="deleteDocument(${doc.id})">Delete</button>
        </td>
    `;

    return tr;
}

// View document
window.viewDocument = async (id) => {
    try {
        const doc = await ApiService.getDocumentById(id);
        alert(`Document: ${doc.title}\n\nDescription: ${doc.description || 'N/A'}\n\nOwner: ${doc.uploadedBy}\n\nAccess: ${doc.accessType}\n\nTags: ${doc.tags.join(', ')}`);
    } catch (error) {
        Utils.showToast(error.message || 'Failed to load document details', 'error');
    }
};

// Download document
window.downloadDocument = async (id, fileName) => {
    try {
        Utils.showToast('Downloading...', 'success');
        
        const blob = await ApiService.downloadDocument(id);
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);

        Utils.showToast('Download complete!', 'success');
    } catch (error) {
        Utils.showToast(error.message || 'Download failed', 'error');
    }
};

// Delete document
window.deleteDocument = async (id) => {
    if (!confirm('Are you sure you want to delete this document?')) {
        return;
    }

    try {
        await ApiService.deleteDocument(id);
        Utils.showToast('Document deleted successfully', 'success');
        loadDocuments(currentPage, currentSearchTerm);
    } catch (error) {
        Utils.showToast(error.message || 'Failed to delete document', 'error');
    }
};

// Update pagination
function updatePagination() {
    if (totalPages <= 1) {
        pagination.style.display = 'none';
        return;
    }

    pagination.style.display = 'flex';
    pageInfo.textContent = `Page ${currentPage} of ${totalPages}`;
    
    prevPageBtn.disabled = currentPage === 1;
    nextPageBtn.disabled = currentPage === totalPages;
}

// Show loading
function showLoading() {
    loadingState.style.display = 'block';
    documentsTable.style.display = 'none';
    emptyState.style.display = 'none';
}

// Hide loading
function hideLoading() {
    loadingState.style.display = 'none';
}

// Show empty
function showEmpty() {
    loadingState.style.display = 'none';
    documentsTable.style.display = 'none';
    emptyState.style.display = 'block';
    pagination.style.display = 'none';
}

// Escape HTML
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

// Event listeners
logoutBtn.addEventListener('click', () => {
    AuthService.logout();
});

uploadBtn.addEventListener('click', () => {
    window.location.href = '/upload.html';
});

searchBtn.addEventListener('click', () => {
    const searchTerm = searchInput.value.trim();
    if (searchTerm) {
        loadDocuments(1, searchTerm);
    }
});

clearSearchBtn.addEventListener('click', () => {
    searchInput.value = '';
    currentSearchTerm = null;
    loadDocuments(1);
});

searchInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
        searchBtn.click();
    }
});

prevPageBtn.addEventListener('click', () => {
    if (currentPage > 1) {
        loadDocuments(currentPage - 1, currentSearchTerm);
    }
});

nextPageBtn.addEventListener('click', () => {
    if (currentPage < totalPages) {
        loadDocuments(currentPage + 1, currentSearchTerm);
    }
});

// Initialize app
init();