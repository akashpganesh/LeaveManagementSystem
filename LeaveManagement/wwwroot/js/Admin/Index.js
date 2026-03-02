const token = localStorage.getItem("token");
let statusChart, departmentChart, leaveTypeChart;

document.addEventListener("DOMContentLoaded", async () => {
    if (!token) {
        window.location.href = "/Account/Login";
        return;
    }

    const monthFilter = document.getElementById("monthFilter");
    monthFilter.addEventListener("change", loadDashboard);

    await loadDashboard();
});

async function loadDashboard() {
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
        const departments = data.data;

        // === Update summary cards ===
        document.getElementById("approvedCount").innerText = counts.approvedCount;
        document.getElementById("pendingCount").innerText = counts.pendingCount;
        document.getElementById("rejectedCount").innerText = counts.rejectedCount;
        document.getElementById("cancelledCount").innerText = counts.cancelledCount;

        // === Chart 1: Status Overview ===
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

        // === Chart 2: Department-wise ===
        const deptLabels = Object.keys(departments);
        const deptCounts = deptLabels.map(d => departments[d].length);

        if (departmentChart) departmentChart.destroy();
        const deptCtx = document.getElementById("departmentChart");
        departmentChart = new Chart(deptCtx, {
            type: "pie",
            data: {
                labels: deptLabels,
                datasets: [{
                    data: deptCounts,
                    backgroundColor: ["#0d6efd", "#198754", "#ffc107", "#dc3545", "#6c757d", "#6610f2"]
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { position: "bottom" } }
            }
        });

        // === Chart 3: Leave Type Distribution ===
        const leaveTypeCount = {};
        for (const dept of Object.keys(departments)) {
            for (const leave of departments[dept]) {
                leaveTypeCount[leave.leaveType] = (leaveTypeCount[leave.leaveType] || 0) + 1;
            }
        }

        const leaveTypeLabels = Object.keys(leaveTypeCount);
        const leaveTypeValues = Object.values(leaveTypeCount);

        if (leaveTypeChart) leaveTypeChart.destroy();
        const leaveTypeCtx = document.getElementById("leaveTypeChart");
        leaveTypeChart = new Chart(leaveTypeCtx, {
            type: "doughnut",
            data: {
                labels: leaveTypeLabels,
                datasets: [{
                    data: leaveTypeValues,
                    backgroundColor: ["#0dcaf0", "#20c997", "#ffc107", "#dc3545", "#6f42c1"]
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