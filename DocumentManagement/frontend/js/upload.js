import ApiService from './api.js';
import AuthService from './auth.js';
import Utils from './utils.js';

// Check authentication
AuthService.checkAuth();

// Elements
const userInfo = document.getElementById('userInfo');
const logoutBtn = document.getElementById('logoutBtn');
const backBtn = document.getElementById('backBtn');
const uploadForm = document.getElementById('uploadForm');
const titleInput = document.getElementById('title');
const descriptionInput = document.getElementById('description');
const fileInput = document.getElementById('file');
const tagsInput = document.getElementById('tags');
const accessTypeSelect = document.getElementById('accessType');
const cancelBtn = document.getElementById('cancelBtn');
const submitBtn = document.getElementById('submitBtn');
const fileName = document.getElementById('fileName');
const fileSize = document.getElementById('fileSize');
const accessTypeHelp = document.getElementById('accessTypeHelp');

// Initialize
const user = AuthService.getUser();
if (user) {
    userInfo.textContent = `${user.name} (${user.role})`;
}

// File input handler
fileInput.addEventListener('change', (e) => {
    const file = e.target.files[0];
    
    if (file) {
        fileName.textContent = file.name;
        fileSize.textContent = Utils.formatFileSize(file.size);

        // Validate file
        Utils.clearError('file');

        if (!Utils.validateFileType(file.name)) {
            Utils.showError('file', 'Invalid file type. Only PDF, DOCX, and TXT are allowed.');
            fileInput.value = '';
            return;
        }

        if (!Utils.validateFileSize(file.size)) {
            Utils.showError('file', 'File size exceeds 10MB limit.');
            fileInput.value = '';
            return;
        }
    } else {
        fileName.textContent = 'No file selected';
        fileSize.textContent = '';
    }
});

// Access type handler
accessTypeSelect.addEventListener('change', (e) => {
    const helpTexts = {
        'Private': 'Private: Only you can access',
        'Public': 'Public: All authenticated users can access',
        'Restricted': 'Restricted: Only specific users you share with can access'
    };
    accessTypeHelp.textContent = helpTexts[e.target.value];
});

// Form submit
uploadForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    // Clear previous errors
    Utils.clearError('title');
    Utils.clearError('description');
    Utils.clearError('file');
    Utils.clearError('tags');

    // Get values
    const title = titleInput.value.trim();
    const description = descriptionInput.value.trim();
    const file = fileInput.files[0];
    const tagsStr = tagsInput.value.trim();
    const accessType = accessTypeSelect.value;

    // Validate
    let hasError = false;

    if (!title) {
        Utils.showError('title', 'Title is required');
        hasError = true;
    } else if (title.length > 255) {
        Utils.showError('title', 'Title cannot exceed 255 characters');
        hasError = true;
    }

    if (description && description.length > 2000) {
        Utils.showError('description', 'Description cannot exceed 2000 characters');
        hasError = true;
    }

    if (!file) {
        Utils.showError('file', 'File is required');
        hasError = true;
    }

    if (hasError) {
        return;
    }

    // Parse tags
    const tags = tagsStr ? tagsStr.split(',').map(t => t.trim()).filter(t => t) : [];

    // Create FormData
    const formData = new FormData();
    formData.append('file', file);
    formData.append('title', title);
    formData.append('accessType', accessType);
    
    if (description) {
        formData.append('description', description);
    }

    if (tags.length > 0) {
        tags.forEach(tag => formData.append('tags', tag));
    }

    try {
        Utils.showLoading(submitBtn);

        await ApiService.uploadDocument(formData);

        Utils.showToast('Document uploaded successfully!', 'success');
        Utils.showMessage('uploadMessage', 'Document uploaded successfully! Redirecting...', 'success');

        setTimeout(() => {
            window.location.href = '/dashboard.html';
        }, 1500);

    } catch (error) {
        Utils.hideLoading(submitBtn);
        Utils.showToast(error.message || 'Upload failed', 'error');
        Utils.showMessage('uploadMessage', error.message || 'Upload failed. Please try again.', 'error');
    }
});

// Event listeners
logoutBtn.addEventListener('click', () => {
    AuthService.logout();
});

backBtn.addEventListener('click', () => {
    window.location.href = '/dashboard.html';
});

cancelBtn.addEventListener('click', () => {
    window.location.href = '/dashboard.html';
});