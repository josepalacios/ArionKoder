import ApiService from './api.js';
import AuthService from './auth.js';
import Utils from './utils.js';

// Redirect if already logged in
if (AuthService.isAuthenticated()) {
    window.location.href = '/dashboard.html';
}

const loginForm = document.getElementById('loginForm');
const emailInput = document.getElementById('email');
const passwordInput = document.getElementById('password');
const loginBtn = document.getElementById('loginBtn');

loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    Utils.clearError('email');
    Utils.clearError('password');

    const email = emailInput.value.trim();
    const password = passwordInput.value;

    // Validation
    if (!email) {
        Utils.showError('email', 'Email is required');
        return;
    }

    if (!Utils.validateEmail(email)) {
        Utils.showError('email', 'Invalid email format');
        return;
    }

    if (!password) {
        Utils.showError('password', 'Password is required');
        return;
    }

    if (password.length < 6) {
        Utils.showError('password', 'Password must be at least 6 characters');
        return;
    }

    try {
        Utils.showLoading(loginBtn);

        await ApiService.login(email, password);

        Utils.showMessage('loginMessage', 'Login successful! Redirecting...', 'success');

        setTimeout(() => {
            window.location.href = '/dashboard.html';
        }, 1000);

    } catch (error) {
        Utils.hideLoading(loginBtn);
        Utils.showMessage('loginMessage', error.message || 'Login failed. Please try again.', 'error');
    }
});