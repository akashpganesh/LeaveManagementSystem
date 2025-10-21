document.addEventListener("DOMContentLoaded", () => {
    const monthSelect = document.getElementById("monthFilter");
    loadLeaveRequests(monthSelect.value);

    monthSelect.addEventListener("change", () => {
        loadLeaveRequests(monthSelect.value);
    });
});

const apiBase = "/api/LeaveRequest";
const alertPlaceholder = document.getElementById("alert-placeholder");
const leaveCountsDiv = document.getElementById("leaveCounts");

async function loadLeaveRequests(months) {
    const token = localStorage.getItem("token");
    if (!token) {
        window.location.href = "/Home/Index";
        return;
    }

    try {
        let url = apiBase + "/GetAll";
        if (months) url += `?months=${months}`;

        const response = await fetch(url, {
            headers: { "Authorization": `Bearer ${token}` }
        });
        const result = await response.json();

        if (response.ok) {
            displayLeaveCounts(result.counts);
            displayLeaveRequests(result.data);
        } else {
            showAlert("danger", result.message || "Failed to load leave requests.");
        }
    } catch (error) {
        console.error(error);
        showAlert("danger", "Unexpected error occurred while loading leave requests.");
    }
}

function displayLeaveCounts(counts) {
    leaveCountsDiv.innerHTML = `
        <div class="d-flex gap-3">
            <span class="badge bg-success">Approved: ${counts.approvedCount}</span>
            <span class="badge bg-warning text-dark">Pending: ${counts.pendingCount}</span>
            <span class="badge bg-danger">Rejected: ${counts.rejectedCount}</span>
        </div>
    `;
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
                <th>Employee</th>
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
                    <td>${leave.employeeName}</td>
                    <td>${new Date(leave.startDate).toLocaleDateString()}</td>
                    <td>${new Date(leave.endDate).toLocaleDateString()}</td>
                    <td>${leave.leaveType}</td>
                    <td>${getStatusBadge(leave.status)}</td>
                    <td>${leave.remarks || "-"}</td>
                    <td>
                        <button class="btn btn-sm btn-info me-1" onclick="viewLeave(${leave.leaveId})">View</button>
                        ${leave.status === "Pending" ? `
                            <button class="btn btn-sm btn-success me-1" onclick="updateLeaveStatus(${leave.leaveId}, 'Approved')">Approve</button>
                            <button class="btn btn-sm btn-danger" onclick="updateLeaveStatus(${leave.leaveId}, 'Rejected')">Reject</button>
                        ` : ""}
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
                <p><strong>Employee:</strong> ${leave.employeeName}</p>
                <p><strong>Department:</strong> ${leave.department}</p>
                <p><strong>Leave Type:</strong> ${leave.leaveType}</p>
                <p><strong>Status:</strong> ${getStatusBadge(leave.status)}</p>
                <p><strong>Start Date:</strong> ${new Date(leave.startDate).toLocaleDateString()}</p>
                <p><strong>End Date:</strong> ${new Date(leave.endDate).toLocaleDateString()}</p>
                <p><strong>Remarks:</strong> ${leave.remarks || "-"}</p>
                <p><strong>Date Requested:</strong> ${new Date(leave.dateRequested).toLocaleDateString()}</p>
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

async function updateLeaveStatus(leaveId, status) {
    if (!confirm(`Are you sure you want to ${status.toLowerCase()} this leave?`)) return;

    const token = localStorage.getItem("token");
    try {
        const response = await fetch(`${apiBase}/${leaveId}/UpdateStatus`, {
            method: "POST",
            headers: {
                "Authorization": `Bearer ${token}`,
                "Content-Type": "application/json"
            },
            body: JSON.stringify({ status })
        });
        const data = await response.json();

        if (response.ok) {
            showAlert("success", data.message);
            loadLeaveRequests(document.getElementById("monthFilter").value);
        } else {
            showAlert("danger", data.message || `Failed to ${status.toLowerCase()} leave.`);
        }
    } catch {
        showAlert("danger", `Error updating leave status.`);
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