const loginForm = document.getElementById('loginForm');
const alertPlaceholder = document.getElementById('alert-placeholder');

loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    clearValidation();

    const email = document.getElementById('email').value.trim();
    const password = document.getElementById('password').value;

    // === Validation ===
    if (!validateEmail(email)) return showInvalid('email');
    if (password.length < 6) return showInvalid('password');

    try {
        const response = await fetch('/api/users/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        const data = await response.json();
        alertPlaceholder.innerHTML = '';

        if (response.ok) {
            localStorage.setItem('token', data.data.token);

            const role = data.data.role;
            switch (role) {
                case "Admin":
                    window.location.href = '/Admin/Index';
                    break;
                case "Manager":
                    window.location.href = '/Manager/Index';
                    break;
                case "Employee":
                    window.location.href = '/Employee/Index';
                    break;
                default:
                    window.location.href = '/';
            }
        } else {
            showAlert('danger', data.message || 'Login failed. Please check your credentials.');
        }

    } catch (error) {
        console.error(error);
        showAlert('danger', 'An unexpected error occurred. Please try again.');
    }
});

// === Helper functions ===

// Show bootstrap alert
function showAlert(type, message) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`;
    alertPlaceholder.innerHTML = alertHtml;
}

// Add invalid feedback
function showInvalid(id) {
    document.getElementById(id).classList.add('is-invalid');
}

// Clear previous invalid states
function clearValidation() {
    document.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
}

// Validate email format
function validateEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}
