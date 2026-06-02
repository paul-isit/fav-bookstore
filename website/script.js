const fallbackBooks = [
  {
    Id: "B01",
    Price: 34.95,
    Stock: 12,
    Title: "The Art of Layering",
    Author: "Jeremy Allan",
    Genre: "Software Engineering",
    Publisher: "Swinburne Academic Press"
  },
  {
    Id: "B02",
    Price: 59.99,
    Stock: 4,
    Title: "Decoupled Web Architectures",
    Author: "Justin Chen",
    Genre: "Computer Science",
    Publisher: "Glenferrie Technical Publishing"
  },
  {
    Id: "B03",
    Price: 45.00,
    Stock: 25,
    Title: "Single Responsibility Guidebook",
    Author: "Pulkit Pannu",
    Genre: "Design Patterns",
    Publisher: "Swinsoft Press"
  }
];

const API_BASE = location.port === "5500" || location.port === "5501"
  ? "http://localhost:5142/api"
  : "/api";

const state = {
  books: [],
  cart: JSON.parse(localStorage.getItem("favouriteBooksCart") || "[]"),
  session: JSON.parse(localStorage.getItem("favouriteBooksSession") || "null")
};

const money = new Intl.NumberFormat("en-AU", {
  style: "currency",
  currency: "AUD"
});

const bookGrid = document.querySelector("#book-grid");
const catalogueMessage = document.querySelector("#catalogue-message");
const searchInput = document.querySelector("#search-input");
const genreFilter = document.querySelector("#genre-filter");
const cartCount = document.querySelector("#cart-count");
const cartItems = document.querySelector("#cart-items");
const cartTotal = document.querySelector("#cart-total");
const checkoutButton = document.querySelector("#checkout-button");
const checkoutForm = document.querySelector("#checkout-form");
const clearCartButton = document.querySelector("#clear-cart-button");
const checkoutMessage = document.querySelector("#checkout-message");
const cartToggle = document.querySelector("#cart-toggle");
const cartPreview = document.querySelector("#cart-preview");
const cartPreviewClose = document.querySelector("#cart-preview-close");
const cartPreviewItems = document.querySelector("#cart-preview-items");
const cartPreviewTotal = document.querySelector("#cart-preview-total");
const sessionStatus = document.querySelector("#session-status");
const signOutButton = document.querySelector("#sign-out-button");
const loginLink = document.querySelector("#login-link");
const signupLink = document.querySelector("#signup-link");
const invoicePanel = document.querySelector("#invoice-panel");
const invoiceNumber = document.querySelector("#invoice-number");
const invoiceCustomer = document.querySelector("#invoice-customer");
const invoicePayment = document.querySelector("#invoice-payment");
const invoiceAddress = document.querySelector("#invoice-address");
const invoiceItems = document.querySelector("#invoice-items");
const invoiceTotal = document.querySelector("#invoice-total");

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

async function loadBooks() {
    try {
        // Try to load books from the backend API.
        state.books = normalizeBooks(await apiRequest("/books"));

        if (catalogueMessage) {
            catalogueMessage.textContent = "";
        }
    } catch (apiError) {
        // Do NOT fall back to books.json while debugging.
        // Otherwise it hides the real problem by showing all books anyway.
        console.error("Failed to load /api/books:", apiError);

        state.books = [];

        if (catalogueMessage) {
            catalogueMessage.textContent = "Could not load /api/books. Check the backend controller.";
        }
    }

    populateGenres();
    renderBooks();
    renderCart();
    renderCartPreview();
    renderSession();
}

function normalizeBooks(books) {
  return books.map(book => ({
    Id: book.Id || book.id || "",
    Price: Number(book.Price ?? book.price ?? 0),
    Stock: Number(book.Stock ?? book.stock ?? 0),
    Title: book.Title || book.title || "",
    Author: book.Author || book.author || "",
    Genre: book.Genre || book.genre || "",
    Publisher: book.Publisher || book.publisher || ""
  }));
}

function renderSession() {
  state.session = JSON.parse(localStorage.getItem("favouriteBooksSession") || "null");

  if (!sessionStatus || !signOutButton || !loginLink || !signupLink) return;

  if (!state.session) {
    sessionStatus.hidden = true;
    signOutButton.hidden = true;
    loginLink.hidden = false;
    signupLink.hidden = false;
    return;
  }

  sessionStatus.textContent = `Shopping as ${state.session.name}`;
  sessionStatus.hidden = false;
  signOutButton.hidden = false;
  loginLink.hidden = true;
  signupLink.hidden = true;
}


function populateGenres() {
  if (!genreFilter) return;

  const genres = [...new Set(state.books.map(book => book.Genre).filter(Boolean))].sort();
  genreFilter.innerHTML = '<option value="">All genres</option>';

  genres.forEach(genre => {
    const option = document.createElement("option");
    option.value = genre;
    option.textContent = genre;
    genreFilter.append(option);
  });
}

function getFilteredBooks() {
  const search = searchInput ? searchInput.value.trim().toLowerCase() : "";
  const genre = genreFilter ? genreFilter.value : "";

  return state.books.filter(book => {
    const matchesSearch = !search || [book.Title, book.Author, book.Genre]
      .some(value => String(value || "").toLowerCase().includes(search));
    const matchesGenre = !genre || book.Genre === genre;
    return matchesSearch && matchesGenre;
  });
}

function getCartLines() {
  const grouped = state.cart.reduce((items, id) => {
    items[id] = (items[id] || 0) + 1;
    return items;
  }, {});

  return Object.entries(grouped)
    .map(([id, quantity]) => ({ book: state.books.find(book => book.Id === id), quantity }))
    .filter(line => line.book);
}

function getCartTotal(lines) {
  return lines.reduce((total, line) => total + Number(line.book.Price || 0) * line.quantity, 0);
}

function renderBooks() {
  if (!bookGrid) return;

  const books = getFilteredBooks();
  bookGrid.innerHTML = "";
  if (catalogueMessage && !books.length) catalogueMessage.textContent = "No matching books found.";

  books.forEach(book => {
    const soldOut = Number(book.Stock || 0) <= 0;
    const card = document.createElement("article");
    card.className = "book-card";
    card.innerHTML = `
      <div class="cover">${escapeHtml(book.Title?.charAt(0) || "B")}</div>
      <div class="book-copy">
        <p class="genre">${escapeHtml(book.Genre || "Book")}</p>
        <h3>${escapeHtml(book.Title || "Untitled")}</h3>
        <p class="book-meta">by ${escapeHtml(book.Author || "Unknown author")}</p>
        <p class="book-meta">${escapeHtml(book.Publisher || "Unknown publisher")}</p>
        <p><strong>${money.format(Number(book.Price || 0))}</strong> <span class="stock">${Number(book.Stock || 0)} in stock</span></p>
        <div class="card-actions">
          <button class="button" type="button" data-add="${escapeHtml(book.Id)}" ${soldOut ? "disabled" : ""}>${soldOut ? "Sold out" : "Add to cart"}</button>
        </div>
      </div>
    `;
    bookGrid.append(card);
  });
}

function addToCart(bookId) {
  const book = state.books.find(item => item.Id === bookId);
  const currentQuantity = state.cart.filter(id => id === bookId).length;

  if (!book || currentQuantity >= Number(book.Stock || 0)) {
    if (catalogueMessage) catalogueMessage.textContent = "No more stock is available for that book.";
    return;
  }

  state.cart.push(bookId);
  saveCart();
  renderCart();
  renderCartPreview();
  openCartPreview();
}

function removeFromCart(bookId) {
  const index = state.cart.indexOf(bookId);
  if (index >= 0) state.cart.splice(index, 1);
  saveCart();
  renderCart();
  renderCartPreview();
  renderSession();
}

function setCartQuantity(bookId, quantity) {
  const book = state.books.find(item => item.Id === bookId);
  const safeQuantity = Math.max(0, Math.min(Number(quantity || 0), Number(book?.Stock || 0)));

  state.cart = state.cart.filter(id => id !== bookId);
  for (let index = 0; index < safeQuantity; index += 1) {
    state.cart.push(bookId);
  }

  if (checkoutMessage && quantity > safeQuantity) {
    checkoutMessage.textContent = `Only ${safeQuantity} copies of ${book?.Title || "that book"} are available.`;
  }

  saveCart();
  renderCart();
  renderCartPreview();
}

function saveCart() {
  localStorage.setItem("favouriteBooksCart", JSON.stringify(state.cart));
}

function renderCart() {
  const lines = getCartLines();
  if (cartCount) cartCount.textContent = String(state.cart.length);
  if (!cartItems) return;

  cartItems.innerHTML = "";

  if (!lines.length) {
    cartItems.innerHTML = '<p class="muted">Your cart is empty.</p>';
    if (cartTotal) cartTotal.textContent = money.format(0);
    return;
  }

  lines.forEach(({ book, quantity }) => {
    const lineTotal = Number(book.Price || 0) * quantity;

    const line = document.createElement("article");
    line.className = "cart-line";
    line.innerHTML = `
      <div>
        <h3>${escapeHtml(book.Title)}</h3>
        <p class="book-meta">${escapeHtml(book.Author)} · ${Number(book.Stock || 0)} in stock</p>
      </div>
      <div class="quantity-control" aria-label="Quantity for ${escapeHtml(book.Title)}">
        <button class="icon-button" type="button" data-decrease="${escapeHtml(book.Id)}" aria-label="Decrease quantity">-</button>
        <input type="number" min="0" max="${Number(book.Stock || 0)}" value="${quantity}" data-quantity="${escapeHtml(book.Id)}" aria-label="Quantity" />
        <button class="icon-button" type="button" data-increase="${escapeHtml(book.Id)}" aria-label="Increase quantity">+</button>
      </div>
      <strong>${money.format(lineTotal)}</strong>
      <button class="ghost-button" type="button" data-remove-line="${escapeHtml(book.Id)}">Remove</button>
    `;
    cartItems.append(line);
  });

  if (cartTotal) cartTotal.textContent = money.format(getCartTotal(lines));
}

function renderCartPreview() {
  if (!cartPreviewItems || !cartPreviewTotal) return;

  const lines = getCartLines();
  cartPreviewItems.innerHTML = "";

  if (!lines.length) {
    cartPreviewItems.innerHTML = '<p class="muted">Your cart is empty.</p>';
    cartPreviewTotal.textContent = money.format(0);
    return;
  }

  lines.forEach(({ book, quantity }) => {
    const item = document.createElement("div");
    item.className = "cart-preview-line";
    item.innerHTML = `
      <div>
        <strong>${escapeHtml(book.Title)}</strong>
        <span>Qty ${quantity}</span>
      </div>
      <span>${money.format(Number(book.Price || 0) * quantity)}</span>
    `;
    cartPreviewItems.append(item);
  });

  cartPreviewTotal.textContent = money.format(getCartTotal(lines));
}

function getCheckoutDetails() {
  if (!checkoutForm) return { address: null, payment: null };

  const form = new FormData(checkoutForm);
  return {
    address: {
      street: String(form.get("street") || "").trim(),
      suburb: String(form.get("suburb") || "").trim(),
      state: String(form.get("state") || "").trim(),
      postcode: String(form.get("postcode") || "").trim()
    },
    payment: {
      method: String(form.get("method") || "").trim()
    }
  };
}

async function checkout(event) {
  if (event) event.preventDefault();

  const lines = getCartLines();
  if (!lines.length) {
    checkoutMessage.textContent = "Add a book before checking out.";
    return;
  }

  if (checkoutForm && !checkoutForm.reportValidity()) return;

  try {
    if (checkoutMessage) checkoutMessage.textContent = "Processing checkout...";
    const checkoutDetails = getCheckoutDetails();
    const result = await apiRequest("/checkout", {
      method: "POST",
      body: JSON.stringify({
        email: state.session?.email || null,
        items: lines.map(line => ({ id: line.book.Id, quantity: line.quantity })),
        address: checkoutDetails.address,
        payment: checkoutDetails.payment
      })
    });

    state.books = normalizeBooks(result.books || result.Books || []);
    state.cart = [];
    saveCart();
    populateGenres();
    renderBooks();
    renderCart();
    renderCartPreview();
    renderInvoice(result.invoice || result.Invoice);
    checkoutMessage.textContent = (result.message || result.Message) + " Total paid: " + money.format(result.total || result.Total || 0) + " Stock has been updated.";
  } catch (error) {
    checkoutMessage.textContent = error.message;
    await loadBooks();
  }
}

function renderInvoice(invoice) {
  if (!invoicePanel || !invoice) return;

  const payment = invoice.payment || invoice.Payment || {};
  const address = invoice.address || invoice.Address || {};
  const items = invoice.items || invoice.Items || [];
  const total = invoice.total ?? invoice.Total ?? 0;

  invoiceNumber.textContent = invoice.invoiceNumber || invoice.InvoiceNumber || "";
  invoiceCustomer.textContent = state.session
    ? `${state.session.name} (${state.session.email || state.session.role})`
    : "Guest shopper";
  invoicePayment.textContent = payment.method || payment.Method || "Payment";
  invoiceAddress.textContent = [
    address.street || address.Street,
    address.suburb || address.Suburb,
    address.state || address.State,
    address.postcode || address.Postcode
  ].filter(Boolean).join(", ");

  invoiceItems.innerHTML = "";
  items.forEach(item => {
    const line = document.createElement("div");
    line.className = "invoice-line";
    line.innerHTML = `
      <span>${escapeHtml(item.title || item.Title)} × ${Number(item.quantity || item.Quantity || 0)}</span>
      <strong>${money.format(Number(item.lineTotal || item.LineTotal || 0))}</strong>
    `;
    invoiceItems.append(line);
  });

  invoiceTotal.textContent = money.format(Number(total));
  invoicePanel.hidden = false;
  invoicePanel.scrollIntoView({ behavior: "smooth", block: "start" });
}

function openCartPreview() {
  if (!cartPreview || !cartToggle) return;
  cartPreview.hidden = false;
  cartToggle.setAttribute("aria-expanded", "true");
}

function closeCartPreview() {
  if (!cartPreview || !cartToggle) return;
  cartPreview.hidden = true;
  cartToggle.setAttribute("aria-expanded", "false");
}

function escapeHtml(value) {
  return String(value || "")
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#039;");
}

if (searchInput) searchInput.addEventListener("input", renderBooks);
if (genreFilter) genreFilter.addEventListener("change", renderBooks);

if (bookGrid) {
  bookGrid.addEventListener("click", event => {
    const button = event.target.closest("[data-add]");
    if (button) addToCart(button.dataset.add);
  });
}

if (cartItems) {
  cartItems.addEventListener("click", event => {
    const removeButton = event.target.closest("[data-remove-line]");
    const increaseButton = event.target.closest("[data-increase]");
    const decreaseButton = event.target.closest("[data-decrease]");

    if (removeButton) setCartQuantity(removeButton.dataset.removeLine, 0);
    if (increaseButton) {
      const currentQuantity = state.cart.filter(id => id === increaseButton.dataset.increase).length;
      setCartQuantity(increaseButton.dataset.increase, currentQuantity + 1);
    }
    if (decreaseButton) removeFromCart(decreaseButton.dataset.decrease);
  });

  cartItems.addEventListener("change", event => {
    const input = event.target.closest("[data-quantity]");
    if (input) setCartQuantity(input.dataset.quantity, Number(input.value));
  });
}

if (clearCartButton) {
  clearCartButton.addEventListener("click", () => {
    state.cart = [];
    saveCart();
    renderCart();
    renderCartPreview();
    if (invoicePanel) invoicePanel.hidden = true;
    if (checkoutMessage) checkoutMessage.textContent = "Cart cleared.";
  });
}

if (checkoutButton && !checkoutForm) checkoutButton.addEventListener("click", checkout);
if (checkoutForm) checkoutForm.addEventListener("submit", checkout);

if (signOutButton) {
  signOutButton.addEventListener("click", () => {
    localStorage.removeItem("favouriteBooksSession");
    state.session = null;
    renderSession();
  });
}

if (cartToggle) {
  cartToggle.addEventListener("click", event => {
    event.stopPropagation();
    if (cartPreview.hidden) openCartPreview();
    else closeCartPreview();
  });
}

if (cartPreviewClose) cartPreviewClose.addEventListener("click", closeCartPreview);

document.addEventListener("click", event => {
  if (!cartPreview || cartPreview.hidden) return;
  if (!event.target.closest(".cart-menu")) closeCartPreview();
});

loadBooks();
