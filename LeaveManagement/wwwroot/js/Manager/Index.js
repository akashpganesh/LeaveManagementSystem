const token = localStorage.getItem("token");
let statusChart, leaveTypeChart, employeeChart;

document.addEventListener("DOMContentLoaded", async () => {
    if (!token) {
        window.location.href = "/Account/Login";
        return;
    }

    const monthFilter = document.getElementById("monthFilter");
    monthFilter.addEventListener("change", loadManagerDashboard);

    await loadManagerDashboard();
});

async function loadManagerDashboard() {
    const months = document.getElementById("monthFilter").value;
    let url = "/api/LeaveRequest/GetAll";
    if (months !== "all") url += `?months=${months}`;

    try {
        const response = await fetch(url, {
            headers: { "Authorization": `Bearer ${token}` }
        });

        if (!response.ok) {
            alert("Failed to load dashboard data");
            return;
        }

        const data = await response.json();
        const counts = data.counts;
        const leaves = data.data;

        // === Update Summary Cards ===
        document.getElementById("approvedCount").innerText = counts.approvedCount;
        document.getElementById("pendingCount").innerText = counts.pendingCount;
        document.getElementById("rejectedCount").innerText = counts.rejectedCount;
        document.getElementById("cancelledCount").innerText = counts.cancelledCount;

        // === Chart 1: Leave Status Overview ===
        if (statusChart) statusChart.destroy();
        const statusCtx = document.getElementById("statusChart");
        statusChart = new Chart(statusCtx, {
            type: "bar",
            data: {
                labels: ["Approved", "Pending", "Rejected", "Cancelled"],
                datasets: [{
                    label: "Requests",
                    data: [
                        counts.approvedCount,
                        counts.pendingCount,
                        counts.rejectedCount,
                        counts.cancelledCount
                    ],
                    backgroundColor: ["#198754", "#ffc107", "#dc3545", "#6c757d"]
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } }
            }
        });

        // === Chart 2: Leave Type Distribution ===
        const leaveTypeCount = {};
        for (const leave of leaves) {
            leaveTypeCount[leave.leaveType] = (leaveTypeCount[leave.leaveType] || 0) + 1;
        }

        if (leaveTypeChart) leaveTypeChart.destroy();
        const leaveTypeCtx = document.getElementById("leaveTypeChart");
        leaveTypeChart = new Chart(leaveTypeCtx, {
            type: "doughnut",
            data: {
                labels: Object.keys(leaveTypeCount),
                datasets: [{
                    data: Object.values(leaveTypeCount),
                    backgroundColor: ["#0dcaf0", "#20c997", "#ffc107", "#dc3545", "#6f42c1"]
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { position: "bottom" } }
            }
        });

        // === Chart 3: Leaves per Employee ===
        const employeeCount = {};
        for (const leave of leaves) {
            employeeCount[leave.employeeName] = (employeeCount[leave.employeeName] || 0) + 1;
        }

        if (employeeChart) employeeChart.destroy();
        const employeeCtx = document.getElementById("employeeChart");
        employeeChart = new Chart(employeeCtx, {
            type: "pie",
            data: {
                labels: Object.keys(employeeCount),
                datasets: [{
                    data: Object.values(employeeCount),
                    backgroundColor: ["#0d6efd", "#198754", "#ffc107", "#dc3545", "#6c757d", "#6610f2"]
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { position: "bottom" } }
            }
        });

    } catch (error) {
        console.error("Error loading dashboard:", error);
        alert("An unexpected error occurred while loading the dashboard.");
    }
}