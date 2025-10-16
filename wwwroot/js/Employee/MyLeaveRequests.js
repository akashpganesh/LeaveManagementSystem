document.addEventListener("DOMContentLoaded", () => {
    loadLeaveRequests();
});

const apiBase = "/api/LeaveRequest";
const alertPlaceholder = document.getElementById("alert-placeholder");

async function loadLeaveRequests() {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "/Home/Index";
        return;
    }

    try {
        const response = await fetch(`${apiBase}/MyLeaveRequests`, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        const result = await response.json();

        if (response.ok) {
            displayLeaveRequests(result.data);
        } else {
            showAlert("danger", result.message || "Failed to load leave requests.");
        }
    } catch (error) {
        console.error(error);
        showAlert("danger", "Unexpected error occurred while loading leave requests.");
    }
}

function displayLeaveRequests(leaves) {
    const container = document.getElementById("leaveList");
    container.innerHTML = "";

    if (!leaves || leaves.length === 0) {
        container.innerHTML = `<p class="text-center text-muted">No leave requests found.</p>`;
        return;
    }

    const table = document.createElement("table");
    table.className = "table table-bordered table-striped";

    table.innerHTML = `
        <thead class="table-light">
            <tr>
                <th>ID</th>
                <th>Start</th>
                <th>End</th>
                <th>Type</th>
                <th>Status</th>
                <th>Remarks</th>
                <th>Actions</th>
            </tr>
        </thead>
        <tbody>
            ${leaves.map(leave => `
                <tr>
                    <td>${leave.leaveId}</td>
                    <td>${new Date(leave.startDate).toLocaleDateString()}</td>
                    <td>${new Date(leave.endDate).toLocaleDateString()}</td>
                    <td>${leave.leaveType}</td>
                    <td>${getStatusBadge(leave.status)}</td>
                    <td>${leave.remarks || "-"}</td>
                    <td>
                        <button class="btn btn-sm btn-info" onclick="viewLeave(${leave.leaveId})">View</button>
                    </td>
                </tr>
            `).join("")}
        </tbody>
    `;

    container.appendChild(table);
}

async function viewLeave(leaveId) {
    const token = localStorage.getItem("token");

    try {
        const response = await fetch(`${apiBase}/${leaveId}`, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        const result = await response.json();

        if (response.ok) {
            const leave = result.data;
            const modalBody = document.getElementById("leaveDetailsBody");

            modalBody.innerHTML = `
                <p><strong>Leave Type:</strong> ${leave.leaveType}</p>
                <p><strong>Status:</strong> ${getStatusBadge(leave.status)}</p>
                <p><strong>Start Date:</strong> ${new Date(leave.startDate).toLocaleDateString()}</p>
                <p><strong>End Date:</strong> ${new Date(leave.endDate).toLocaleDateString()}</p>
                <p><strong>Remarks:</strong> ${leave.remarks || "-"}</p>
                <p><strong>Date Requested:</strong> ${new Date(leave.dateRequested).toLocaleDateString()}</p>
                <p><strong>Approved By:</strong> ${leave.managerName || "Pending"}</p>
            `;

            const leaveModal = new bootstrap.Modal(document.getElementById('leaveDetailsModal'));
            leaveModal.show();
        } else {
            showAlert("danger", result.message || "Failed to fetch leave details.");
        }
    } catch {
        showAlert("danger", "Error fetching leave details.");
    }
}

function getStatusBadge(status) {
    let colorClass = "secondary";
    if (status === "Approved") colorClass = "success";
    else if (status === "Pending") colorClass = "warning";
    else if (status === "Rejected") colorClass = "danger";
    return `<span class="badge bg-${colorClass}">${status}</span>`;
}

function showAlert(type, message) {
    const alertHtml = `
        <div class="alert alert-${type} alert-dismissible fade show mt-3" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>`;
    alertPlaceholder.innerHTML = alertHtml;
}
