const registerForm = document.getElementById('registerForm');
const alertPlaceholder = document.getElementById('alert-placeholder');

registerForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const fullName = document.getElementById('fullName').value;
    const email = document.getElementById('email').value;
    const phone = document.getElementById('phone').value;
    const password = document.getElementById('password').value;
    const department = document.getElementById('department').value;

    try {
        const response = await fetch('/api/users/register', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ fullName, email, phone, password, department })
        });

        const data = await response.json();
        alertPlaceholder.innerHTML = '';

        if (response.ok) {
            showAlert('success', data.message);
            registerForm.reset();
        } else {
            showAlert('danger', data.message || 'Registration failed');
        }

    } catch (error) {
        showAlert('danger', 'An unexpected error occurred.');
        console.error(error);
    }
});

function showAlert(type, message) {
    const alertHtml = `
                <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                    ${message}
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                </div>`;
    alertPlaceholder.innerHTML = alertHtml;
}