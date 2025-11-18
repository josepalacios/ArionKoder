import { API_CONFIG } from './config.js';
import AuthService from './auth.js';

class ApiService {
    static async request(endpoint, options = {}) {
        const token = AuthService.getToken();
        
        const config = {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...(token && { 'Authorization': `Bearer ${token}` }),
                ...options.headers
            }
        };

        try {
            const response = await fetch(`${API_CONFIG.BASE_URL}${endpoint}`, config);

            if (response.status === 401) {
                AuthService.logout();
                throw new Error('Session expired. Please login again.');
            }

            if (!response.ok) {
                const error = await response.json();
                throw new Error(error.error || `HTTP ${response.status}: ${response.statusText}`);
            }

            const contentType = response.headers.get('content-type');
            if (contentType && contentType.includes('application/json')) {
                return await response.json();
            }

            return response;
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    }

    static async login(email, password) {
        const response = await this.request(API_CONFIG.ENDPOINTS.LOGIN, {
            method: 'POST',
            body: JSON.stringify({ email, password })
        });

        AuthService.setToken(response.token);
        AuthService.setUser(response.user);

        return response;
    }

    static async getDocuments(pageNumber = 1, pageSize = 20, searchTerm = null) {
        const params = new URLSearchParams({
            pageNumber,
            pageSize,
            ...(searchTerm && { searchTerm })
        });

        return await this.request(`${API_CONFIG.ENDPOINTS.DOCUMENTS}?${params}`);
    }

    static async getDocumentById(id) {
        return await this.request(API_CONFIG.ENDPOINTS.DOCUMENT_BY_ID(id));
    }

    static async uploadDocument(formData) {
        const token = AuthService.getToken();
        
        const response = await fetch(`${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.DOCUMENTS}`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`
            },
            body: formData
        });

        if (response.status === 401) {
            AuthService.logout();
            throw new Error('Session expired. Please login again.');
        }

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.error || 'Upload failed');
        }

        return await response.json();
    }

    static async deleteDocument(id) {
        return await this.request(API_CONFIG.ENDPOINTS.DOCUMENT_BY_ID(id), {
            method: 'DELETE'
        });
    }

    static async downloadDocument(id) {
        const token = AuthService.getToken();
        
        const response = await fetch(
            `${API_CONFIG.BASE_URL}${API_CONFIG.ENDPOINTS.DOCUMENT_DOWNLOAD(id)}`,
            {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            }
        );

        if (!response.ok) {
            throw new Error('Download failed');
        }

        return response.blob();
    }
}

export default ApiService;
