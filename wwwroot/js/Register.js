const registerForm = document.getElementById('registerForm');
const alertPlaceholder = document.getElementById('alert-placeholder');

registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    clearValidation();

    const fullName = document.getElementById('fullName').value.trim();
    const email = document.getElementById('email').value.trim();
    const phone = document.getElementById('phone').value.trim();
    const password = document.getElementById('password').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    const department = document.getElementById('department').value;

    // Basic validation
    if (!fullName) return showInvalid('fullName');
    if (!validateEmail(email)) return showInvalid('email');
    if (!/^[0-9]{10}$/.test(phone)) return showInvalid('phone');
    if (!department) return showInvalid('department');
    if (password.length < 6) return showInvalid('password');
    if (password !== confirmPassword) return showInvalid('confirmPassword');

    try {
        const response = await fetch('/api/users/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ fullName, email, phone, password, department })
        });

        const data = await response.json();
        alertPlaceholder.innerHTML = '';

        if (response.ok) {
            showAlert('success', data.message || 'Registration successful!');
            registerForm.reset();
        } else {
            showAlert('danger', data.message || 'Registration failed.');
        }
    } catch (error) {
        showAlert('danger', 'An unexpected error occurred.');
        console.error(error);
    }
});

// Helper: show bootstrap alert
function showAlert(type, message) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`;
    alertPlaceholder.innerHTML = alertHtml;
}

// Helper: mark invalid input
function showInvalid(id) {
    document.getElementById(id).classList.add('is-invalid');
}

// Helper: clear previous validation states
function clearValidation() {
    document.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
}

// Helper: validate email format
function validateEmail(email) {
    return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}
