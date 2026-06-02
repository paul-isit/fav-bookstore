const API_BASE = location.port === "5500" || location.port === "5501"
  ? "http://localhost:5142/api"
  : "/api";

const sessionKey = "favouriteBooksSession";
const signupForm = document.querySelector("#signup-form");
const loginForm = document.querySelector("#login-form");

async function apiRequest(path, options = {}) {
  const response = await fetch(`${API_BASE}${path}`, {
    headers: { "Content-Type": "application/json", ...(options.headers || {}) },
    ...options
  });

  const data = await response.json().catch(() => null);
  if (!response.ok) {
    throw new Error(data?.message || "The server could not complete the request.");
  }

  return data;
}

function saveSession(user) {
  localStorage.setItem(sessionKey, JSON.stringify({
    name: user.name,
    email: user.email,
    role: user.role,
    loggedInAt: new Date().toISOString()
  }));
}

if (signupForm) {
  const message = document.querySelector("#signup-message");

  signupForm.addEventListener("submit", async event => {
    event.preventDefault();
    message.textContent = "Creating account...";

    try {
      const user = await apiRequest("/signup", {
        method: "POST",
        body: JSON.stringify({
          name: document.querySelector("#signup-name").value.trim(),
          email: document.querySelector("#signup-email").value.trim().toLowerCase(),
          password: document.querySelector("#signup-password").value
        })
      });

      saveSession(user);
      message.textContent = `Account created. Welcome, ${user.name}. Redirecting...`;
      setTimeout(() => {
        window.location.href = "index.html#catalogue";
      }, 700);
    } catch (error) {
      message.textContent = error.message;
    }
  });
}

if (loginForm) {
  const message = document.querySelector("#login-message");

  loginForm.addEventListener("submit", async event => {
    event.preventDefault();
    message.textContent = "Logging in...";

    try {
      const user = await apiRequest("/login", {
        method: "POST",
        body: JSON.stringify({
          email: document.querySelector("#login-email").value.trim().toLowerCase(),
          password: document.querySelector("#login-password").value
        })
      });

      saveSession(user);
      message.textContent = `Login successful. Welcome back, ${user.name}. Redirecting...`;
      setTimeout(() => {
        window.location.href = "index.html#catalogue";
      }, 700);
    } catch (error) {
      message.textContent = error.message;
    }
  });
}
