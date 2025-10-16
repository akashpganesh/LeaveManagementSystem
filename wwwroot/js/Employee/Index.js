document.addEventListener("DOMContentLoaded", async () => {
    await loadDashboard();
});

async function loadDashboard() {
    try {
        const token = localStorage.getItem("token");

        const response = await fetch("/api/LeaveRequest/GetDashboard", {
            method: "GET",
            headers: {
                "Authorization": `Bearer ${token}`
            }
        });

        const result = await response.json();

        if (response.ok && result.data) {
            const data = result.data;

            document.getElementById("employee-name").textContent = data.employeeName;

            document.getElementById("employee-info").innerHTML = `
                        <p><strong>Department:</strong> ${data.department}</p>
                        <p><strong>Manager:</strong> ${data.managerName}</p>
                    `;

            document.getElementById("approved-count").textContent = data.totalApproved;
            document.getElementById("pending-count").textContent = data.totalPending;
            document.getElementById("rejected-count").textContent = data.totalRejected;
        } else {
            alert(result.message || "Failed to load dashboard data.");
        }
    } catch (error) {
        alert("Error loading dashboard data: " + error.message);
    }
}