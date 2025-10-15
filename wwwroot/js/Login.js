const loginForm = document.getElementById('loginForm');
const alertPlaceholder = document.getElementById('alert-placeholder');

loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();

    const email = document.getElementById('email').value;
    const password = document.getElementById('password').value;

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
            localStorage.setItem('userId', data.data.userId);
            localStorage.setItem('Role', data.data.role);

            const role = data.data.role;
            if (role === "Admin") {
                window.location.href = '/Admin/Index';
            } else if (role === "Manager") {
                window.location.href = '/Manager/Index';
            } else if (role === "Employee") {
                window.location.href = '/Employee/Index';
            } else {
                window.location.href = '/Dashboard';
            }
        } else {
            showAlert('danger', data.message || 'Login failed');
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