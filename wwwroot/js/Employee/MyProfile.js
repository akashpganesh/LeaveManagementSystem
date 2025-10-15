const alertPlaceholder = document.getElementById("alert-placeholder");

// Extract UserId from JWT
function getUserIdFromToken() {
    const token = localStorage.getItem("token");
    if (!token) return null;

    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.UserId;
}

const userId = getUserIdFromToken();
if (!userId) window.location.href = "/Account/Login";

const token = localStorage.getItem("token");

// Load user profile
async function loadUserProfile() {
    try {
        const response = await fetch(`/api/users/${userId}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const result = await response.json();
        if (response.ok) {
            const user = result.data;
            document.getElementById("name").value = user.fullName || "";
            document.getElementById("email").value = user.email || "";
            document.getElementById("phone").value = user.phone || "";
            document.getElementById("role").value = user.role || "";
            document.getElementById("department").value = user.department || "-";
            document.getElementById("managerName").value = user.managerName || "-";
        } else {
            showAlert("danger", result.message || "Failed to load profile.");
        }
    } catch (error) {
        console.error(error);
        showAlert("danger", "Error loading profile.");
    }
}

document.addEventListener("DOMContentLoaded", loadUserProfile);

// Update Profile
document.getElementById("updateProfileForm").addEventListener("submit", async (e) => {
    e.preventDefault();
    const payload = {
        name: document.getElementById("name").value,
        email: document.getElementById("email").value,
        phone: document.getElementById("phone").value
    };

    try {
        const response = await fetch(`/api/users/${userId}`, {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });

        const data = await response.json();
        if (response.ok) {
            showAlert("success", data.message);
        } else {
            showAlert("danger", data.message || "Failed to update profile.");
        }
    } catch (error) {
        console.error(error);
        showAlert("danger", "Error updating profile.");
    }
});

// Change Password with current password confirmation
document.getElementById("changePasswordForm").addEventListener("submit", async (e) => {
    e.preventDefault();

    const old1 = document.getElementById("oldPassword1").value;
    const old2 = document.getElementById("oldPassword2").value;
    const newPassword = document.getElementById("newPassword").value;

    if (old1 !== old2) {
        showAlert("danger", "Current password does not match.");
        return;
    }

    const payload = {
        oldPassword: old1,
        newPassword: newPassword
    };

    try {
        const response = await fetch("/api/users/change-password", {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });

        const data = await response.json();
        if (response.ok) {
            showAlert("success", data.message);
            document.getElementById("changePasswordForm").reset();
            // Close modal
            const modal = bootstrap.Modal.getInstance(document.getElementById("changePasswordModal"));
            modal.hide();
        } else {
            showAlert("danger", data.message || "Failed to change password.");
        }
    } catch (error) {
        console.error(error);
        showAlert("danger", "Error changing password.");
    }
});

// Alert helper
function showAlert(type, message) {
    alertPlaceholder.innerHTML = `
                <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                    ${message}
                    <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
                </div>
            `;
}