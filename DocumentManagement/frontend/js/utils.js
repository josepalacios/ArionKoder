class Utils {
    static showToast(message, type = 'success') {
        const container = document.getElementById('toastContainer');
        if (!container) return;

        const toast = document.createElement('div');
        toast.className = `toast ${type}`;
        toast.textContent = message;

        container.appendChild(toast);

        setTimeout(() => {
            toast.style.animation = 'slideOut 0.3s ease';
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    }

    static showLoading(button) {
        const btnText = button.querySelector('.btn-text');
        const btnLoader = button.querySelector('.btn-loader');
        
        if (btnText) btnText.style.display = 'none';
        if (btnLoader) btnLoader.style.display = 'inline-flex';
        button.disabled = true;
    }

    static hideLoading(button) {
        const btnText = button.querySelector('.btn-text');
        const btnLoader = button.querySelector('.btn-loader');
        
        if (btnText) btnText.style.display = 'inline';
        if (btnLoader) btnLoader.style.display = 'none';
        button.disabled = false;
    }

    static showMessage(elementId, message, type = 'success') {
        const element = document.getElementById(elementId);
        if (!element) return;

        element.textContent = message;
        element.className = `message ${type}`;
        element.style.display = 'block';

        setTimeout(() => {
            element.style.display = 'none';
        }, 5000);
    }

    static formatDate(dateString) {
        const date = new Date(dateString);
        return new Intl.DateTimeFormat('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        }).format(date);
    }

    static formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round((bytes / Math.pow(k, i)) * 100) / 100 + ' ' + sizes[i];
    }

    static validateEmail(email) {
        const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        return re.test(email);
    }

    static validateFileSize(size, maxSizeMB = 10) {
        const maxSize = maxSizeMB * 1024 * 1024;
        return size <= maxSize;
    }

    static validateFileType(filename) {
        const allowedExtensions = ['pdf', 'docx', 'txt'];
        const extension = filename.split('.').pop().toLowerCase();
        return allowedExtensions.includes(extension);
    }

    static clearError(inputId) {
        const errorElement = document.getElementById(`${inputId}Error`);
        if (errorElement) {
            errorElement.textContent = '';
        }
    }

    static showError(inputId, message) {
        const errorElement = document.getElementById(`${inputId}Error`);
        if (errorElement) {
            errorElement.textContent = message;
        }
    }
}

export default Utils;