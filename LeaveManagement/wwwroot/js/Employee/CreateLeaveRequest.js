const leaveForm = document.getElementById("leaveRequestForm");
const alertPlaceholder = document.getElementById("alert-placeholder");
const leaveTypeSelect = document.getElementById("leaveType");
const token = localStorage.getItem("token");

if (!token) window.location.href = "/Account/Login";

// Clear validation
function clearValidation() {
    document.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
}

// Show invalid feedback
function showInvalid(id, message) {
    const element = document.getElementById(id);
    element.classList.add('is-invalid');
    if (element.nextElementSibling) element.nextElementSibling.textContent = message;
}

// Show alert
function showAlert(type, message) {
    alertPlaceholder.innerHTML = `
        <div class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`;
}

// Fetch leave types from API
async function loadLeaveTypes() {
    try {
        const res = await fetch("/api/LeaveType/GetAll", {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const data = await res.json();

        if (res.ok && data.data) {
            leaveTypeSelect.innerHTML = `<option value="">Select leave type</option>` +
                data.data.map(lt => `<option value="${lt.leaveTypeId}">${lt.leaveTypeName}</option>`).join('');
        } else {
            leaveTypeSelect.innerHTML = `<option value="">Failed to load leave types</option>`;
            showAlert("danger", data.message || "Failed to load leave types");
        }
    } catch (err) {
        console.error(err);
        leaveTypeSelect.innerHTML = `<option value="">Error loading leave types</option>`;
        showAlert("danger", "Unexpected error while loading leave types");
    }
}

// Initialize leave types on page load
document.addEventListener("DOMContentLoaded", loadLeaveTypes);

// Form submission
leaveForm.addEventListener("submit", async (e) => {
    e.preventDefault();
    clearValidation();

    const startDate = document.getElementById("startDate").value;
    const endDate = document.getElementById("endDate").value;
    const leaveTypeId = parseInt(leaveTypeSelect.value);
    const remarks = document.getElementById("remarks").value;

    // Validations
    let hasError = false;

    if (!startDate) { showInvalid("startDate", "Start date is required."); hasError = true; }
    if (!endDate) { showInvalid("endDate", "End date is required."); hasError = true; }
    if (startDate && endDate && new Date(startDate) > new Date(endDate)) {
        showInvalid("startDate", "Start date cannot be after end date.");
        showInvalid("endDate", "End date cannot be before start date.");
        hasError = true;
    }
    if (!leaveTypeId) { showInvalid("leaveType", "Please select a leave type."); hasError = true; }
    if (hasError) return;

    const payload = { startDate, endDate, leaveTypeId, remarks };

    try {
        const response = await fetch("/api/LeaveRequest/request", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`
            },
            body: JSON.stringify(payload)
        });

        const data = await response.json();

        if (response.ok) {
            showAlert("success", data.message);
            leaveForm.reset();
        } else {
            showAlert("danger", data.details || data.message || "Failed to submit leave request.");
        }
    } catch (error) {
        console.error(error);
        showAlert("danger", "An unexpected error occurred.");
    }
});