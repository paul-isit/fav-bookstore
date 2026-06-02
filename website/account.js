const API_BASE = location.port === "5500" || location.port === "5501"
  ? "http://localhost:5142/api"
  : "/api";

const sessionKey = "favouriteBooksSession";
const signupForm = document.querySelector("#signup-form");
const loginForm = document.querySelector("#login-form");
const guestButton = document.querySelector("#guest-login-button");
const cartCount = document.querySelector("#cart-count");
const sessionStatus = document.querySelector("#session-status");
const signOutButton = document.querySelector("#sign-out-button");
const loginLink = document.querySelector("#login-link");
const signupLink = document.querySelector("#signup-link");

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

function normaliseUser(user) {
  return {
    name: user.name || user.Name || "Guest Shopper",
    email: user.email || user.Email || "",
    role: user.role || user.Role || "Guest"
  };
}

function renderHeaderState() {
  const cart = JSON.parse(localStorage.getItem("favouriteBooksCart") || "[]");
  const session = JSON.parse(localStorage.getItem(sessionKey) || "null");

  if (cartCount) cartCount.textContent = String(cart.length);
  if (!sessionStatus || !signOutButton || !loginLink || !signupLink) return;

  if (!session) {
    sessionStatus.hidden = true;
    signOutButton.hidden = true;
    loginLink.hidden = false;
    signupLink.hidden = false;
    return;
  }

  sessionStatus.textContent = `Shopping as ${session.name}`;
  sessionStatus.hidden = false;
  signOutButton.hidden = false;
  loginLink.hidden = true;
  signupLink.hidden = true;
}

if (signOutButton) {
  signOutButton.addEventListener("click", () => {
    localStorage.removeItem(sessionKey);
    renderHeaderState();
  });
}

if (signupForm) {
  const message = document.querySelector("#signup-message");

  signupForm.addEventListener("submit", async event => {
    event.preventDefault();
    message.textContent = "Creating account...";

    try {
      const user = normaliseUser(await apiRequest("/signup", {
        method: "POST",
        body: JSON.stringify({
          name: document.querySelector("#signup-name").value.trim(),
          email: document.querySelector("#signup-email").value.trim().toLowerCase(),
          password: document.querySelector("#signup-password").value
        })
      }));

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

renderHeaderState();

if (loginForm) {
  const message = document.querySelector("#login-message");

  loginForm.addEventListener("submit", async event => {
    event.preventDefault();
    message.textContent = "Logging in...";

    try {
      const user = normaliseUser(await apiRequest("/login", {
        method: "POST",
        body: JSON.stringify({
          email: document.querySelector("#login-email").value.trim().toLowerCase(),
          password: document.querySelector("#login-password").value
        })
      }));

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

if (guestButton) {
  const message = document.querySelector("#login-message");

  guestButton.addEventListener("click", async () => {
    message.textContent = "Starting guest session...";

    try {
      const user = normaliseUser(await apiRequest("/guest", { method: "POST" }));
      saveSession(user);
      message.textContent = "Guest session started. Redirecting...";
      setTimeout(() => {
        window.location.href = "index.html#catalogue";
      }, 500);
    } catch (error) {
      message.textContent = error.message;
    }
  });
}
