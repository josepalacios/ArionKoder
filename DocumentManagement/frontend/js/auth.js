import { STORAGE_KEYS } from './config.js';

class AuthService {
    static setToken(token) {
        localStorage.setItem(STORAGE_KEYS.TOKEN, token);
    }

    static getToken() {
        return localStorage.getItem(STORAGE_KEYS.TOKEN);
    }

    static removeToken() {
        localStorage.removeItem(STORAGE_KEYS.TOKEN);
        localStorage.removeItem(STORAGE_KEYS.USER);
    }

    static setUser(user) {
        localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
    }

    static getUser() {
        const user = localStorage.getItem(STORAGE_KEYS.USER);
        return user ? JSON.parse(user) : null;
    }

    static isAuthenticated() {
        return !!this.getToken();
    }

    static logout() {
        this.removeToken();
        window.location.href = '/index.html';
    }

    static checkAuth() {
        if (!this.isAuthenticated()) {
            window.location.href = '/index.html';
        }
    }
}

export default AuthService;