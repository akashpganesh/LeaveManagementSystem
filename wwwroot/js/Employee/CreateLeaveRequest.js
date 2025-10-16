const leaveForm = document.getElementById("leaveRequestForm");
const alertPlaceholder = document.getElementById("alert-placeholder");
const token = localStorage.getItem("token");

if (!token) window.location.href = "/Account/Login";

    leaveForm.addEventListener("submit", async (e) => {
    e.preventDefault();

const payload = {
    startDate: document.getElementById("startDate").value,
endDate: document.getElementById("endDate").value,
leaveType: document.getElementById("leaveType").value,
remarks: document.getElementById("remarks").value
        };

if (!payload.startDate || !payload.endDate || !payload.leaveType) {
    showAlert("danger", "Please fill all required fields.");
return;
        }

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
    showAlert("danger", data.message || "Failed to submit leave request.");
            }
        } catch (error) {
    console.error(error);
showAlert("danger", "An unexpected error occurred.");
        }
    });

function showAlert(type, message) {
    alertPlaceholder.innerHTML = `
            <div class="alert alert-${type} alert-dismissible fade show" role="alert">
                ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
            </div>
        `;
    }