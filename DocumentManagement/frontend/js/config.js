const API_CONFIG = {
    BASE_URL: 'https://localhost:7011/api/v1',
    ENDPOINTS: {
        LOGIN: '/auth/login',
        ME: '/auth/me',
        LOGOUT: '/auth/logout',
        DOCUMENTS: '/documents',
        DOCUMENT_BY_ID: (id) => `/documents/${id}`,
        DOCUMENT_DOWNLOAD: (id) => `/documents/${id}/download`,
        DOCUMENT_SEARCH: '/documents/search',
    }
};

const STORAGE_KEYS = {
    TOKEN: 'auth_token',
    USER: 'user_data'
};

export { API_CONFIG, STORAGE_KEYS };