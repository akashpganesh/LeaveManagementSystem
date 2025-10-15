document.addEventListener("DOMContentLoaded", loadUsers);

const apiBase = "/api/users";
const alertPlaceholder = document.getElementById("alert-placeholder");

async function loadUsers() {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "/Account/Login";
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
                                <button class="btn btn-sm btn-danger me-1" onclick="deleteUser(${user.userId})">Delete</button>
                                ${user.role === "Employee" ? `
                                    <button class="btn btn-sm btn-warning me-1" onclick="promoteToManager(${user.userId})">Promote</button>
                                    <button class="btn btn-sm btn-secondary" onclick="assignManager(${user.userId})">Assign Manager</button>
                                ` : ""}
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


// Delete user
async function deleteUser(userId) {
    if (!confirm("Are you sure you want to delete this user?")) return;

    const token = localStorage.getItem("token");
    try {
        const response = await fetch(`${apiBase}/${userId}`, {
            method: "DELETE",
            headers: { "Authorization": `Bearer ${token}` }
        });
        const data = await response.json();

        if (response.ok) {
            showAlert("success", data.message);
            loadUsers();
        } else {
            showAlert("danger", data.message || "Failed to delete user.");
        }
    } catch {
        showAlert("danger", "Error deleting user.");
    }
}

// Promote to Manager
async function promoteToManager(userId) {

    const confirmAction = confirm("Are you sure you want to promote this employee?");
    if (!confirmAction) return;

    const token = localStorage.getItem("token");
    try {
        const response = await fetch(`${apiBase}/promote-to-manager`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify({ userId })
        });
        const data = await response.json();

        if (response.ok) {
            showAlert("success", data.message);
            loadUsers();
        } else {
            showAlert("danger", data.message || "Failed to promote user.");
        }
    } catch {
        showAlert("danger", "Error promoting user.");
    }
}

//  Assign Manager 
async function assignManager(userId) {
    const token = localStorage.getItem("token");

    //  Get user details first (to know department)
    const userResponse = await fetch(`${apiBase}/${userId}`, {
        headers: { "Authorization": `Bearer ${token}` }
    });
    const userData = await userResponse.json();
    if (!userResponse.ok) {
        showAlert("danger", userData.message || "Failed to fetch user details.");
        return;
    }

    const user = userData.data;
    document.getElementById("assignUserId").value = user.userId;
    document.getElementById("assignDept").textContent = user.department || "-";

    //  Fetch managers for this department
    const dropdown = document.getElementById("managerDropdown");
    dropdown.innerHTML = `<option value="">Loading...</option>`;

    try {
        const managerResponse = await fetch(`${apiBase}/managers-by-department/${user.department}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const managerData = await managerResponse.json();

        if (managerResponse.ok) {
            dropdown.innerHTML = `<option value="">-- Select Manager --</option>`;
            managerData.data.forEach(m => {
                const option = document.createElement("option");
                option.value = m.userId;
                option.textContent = `${m.fullName} (${m.email})`;
                dropdown.appendChild(option);
            });
        } else {
            dropdown.innerHTML = `<option value="">No managers found</option>`;
        }
    } catch (error) {
        console.error(error);
        dropdown.innerHTML = `<option value="">Error fetching managers</option>`;
    }

    //  Show modal
    const assignModal = new bootstrap.Modal(document.getElementById("assignManagerModal"));
    assignModal.show();

    //  Handle Assign button click
    const confirmBtn = document.getElementById("confirmAssignBtn");
    confirmBtn.onclick = async () => {
        const managerId = dropdown.value;
        if (!managerId) {
            showAlert("warning", "Please select a manager.");
            return;
        }

        try {
            const response = await fetch(`${apiBase}/assign-manager`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                    "Authorization": `Bearer ${token}`
                },
                body: JSON.stringify({
                    userId: parseInt(userId),
                    managerId: parseInt(managerId)
                })
            });

            const result = await response.json();
            if (response.ok) {
                showAlert("success", result.message || "Manager assigned successfully.");
                assignModal.hide();
                loadUsers();
            } else {
                showAlert("danger", result.message || "Failed to assign manager.");
            }
        } catch (error) {
            showAlert("danger", "Error assigning manager.");
        }
    };
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
