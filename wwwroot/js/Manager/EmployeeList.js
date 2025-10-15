document.addEventListener("DOMContentLoaded", loadUsers);

const apiBase = "/api/users";
const alertPlaceholder = document.getElementById("alert-placeholder");

async function loadUsers() {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "/";
        return;
    }

    try {
        const response = await fetch(apiBase, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        const result = await response.json();

        if (response.ok) {
            displayUsers(result.data);
        } else {
            showAlert("danger", result.message || "Failed to load users.");
        }
    } catch (error) {
        console.error(error);
        showAlert("danger", "An unexpected error occurred while loading users.");
    }
}

function displayUsers(groupedUsers) {
    const container = document.getElementById("userList");
    container.innerHTML = "";

    Object.keys(groupedUsers).forEach(role => {
        const users = groupedUsers[role];

        const section = document.createElement("div");
        section.classList.add("mb-5");

        section.innerHTML = `
            <h4 class="mb-3">${role}s</h4>
            <table class="table table-bordered table-striped">
                <thead class="table-light">
                    <tr>
                        <th>ID</th>
                        <th>Full Name</th>
                        <th>Email</th>
                        <th>Phone</th>
                        <th>Department</th>
                        <th>Manager</th>
                        <th>Date Joined</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    ${users.map(user => `
                        <tr>
                            <td>${user.userId}</td>
                            <td>${user.fullName}</td>
                            <td>${user.email}</td>
                            <td>${user.phone}</td>
                            <td>${user.department || "-"}</td>
                            <td>${user.managerName || "-"}</td>
                            <td>${new Date(user.dateJoined).toLocaleDateString()}</td>
                            <td>
                                <button class="btn btn-sm btn-info me-1" onclick="viewDetails(${user.userId})">View</button>
                            </td>
                        </tr>
                    `).join("")}
                </tbody>
            </table>
        `;

        container.appendChild(section);
    });
}

// View details
async function viewDetails(userId) {
    const token = localStorage.getItem("token");
    try {
        const response = await fetch(`${apiBase}/${userId}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const data = await response.json();

        if (response.ok) {
            const user = data.data;

            document.getElementById("modalFullName").textContent = user.fullName || "-";
            document.getElementById("modalEmail").textContent = user.email || "-";
            document.getElementById("modalPhone").textContent = user.phone || "-";
            document.getElementById("modalDepartment").textContent = user.department || "-";
            document.getElementById("modalRole").textContent = user.role || "-";
            document.getElementById("modalManager").textContent = user.managerName || "-";
            document.getElementById("modalDateJoined").textContent = new Date(user.dateJoined).toLocaleDateString();

            const userModal = new bootstrap.Modal(document.getElementById("userDetailsModal"));
            userModal.show();
        } else {
            showAlert("danger", data.message || "Failed to fetch user details.");
        }
    } catch (error) {
        console.error(error);
        showAlert("danger", "Error fetching user details.");
    }
}

// Alert Helper
function showAlert(type, message) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show mt-3" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`;
    alertPlaceholder.innerHTML = alertHtml;
}
