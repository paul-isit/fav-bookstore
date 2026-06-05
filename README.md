# Favourite Books Online Bookstore System

This repository contains the implementation of the **Favourite Books Online Bookstore System**, developed for **SWE30003 - Software Architectures and Design**.

The system is implemented as an **ASP.NET Core MVC web application**. It leverages a robust Model-View-Controller architecture to separate the user interface (Razor Views & static assets), application control logic (Controllers), and the underlying domain model.

---

## 🚀 Getting Started & Requirements

The project targets **.NET 10.0**. Before running the server, please ensure you have the correct .NET SDK installed.

### 1. Verify Your .NET SDK Version
To verify your installed .NET SDK versions, open your terminal and run:
```bash
dotnet --list-sdks
```
Alternatively, check your active version:
```bash
dotnet --version
```
Ensure that **`10.0.x`** (or higher) is listed. If you do not have .NET 10.0 installed, download it from the [Official .NET Download Portal](https://dotnet.microsoft.com/download).

### 2. How to Build and Run the Server
From the repository root directory, execute the following commands in order:

* **Restore dependencies**:
  ```bash
  dotnet restore
  ```
* **Build the solution**:
  ```bash
  dotnet build
  ```
* **Run the integrated server**:
  ```bash
  dotnet run --urls http://localhost:5142
  ```

Once Kestrel starts, open your browser and navigate to:
```text
http://localhost:5142
```

> [!NOTE]
> Upon startup, the backend automatically seeds initial demo data (`books.json` and `users.json`) from seed files and runs the full suite of integration tests via `TestRunner.RunAllTests()`.

---

## 🛠️ Project Architecture & Structure

```text
FavouriteBookstore/
├── Controllers/        # MVC controllers & REST API endpoints
│   ├── HomeController.cs     # Serves the HTML views
│   └── StoreApiController.cs # Handles RESTful AJAX data requests
├── Models/             # Domain classes & data models (OO Domain Pattern)
│   ├── Address.cs            # Address validation & formatting
│   ├── Book.cs               # Book properties & inventory
│   ├── Catalogue.cs          # Active listing filter
│   ├── ShoppingCart.cs       # Cart mutations & state
│   ├── Order.cs              # Order workflow & payment integration
│   └── User.cs               # Polymorphic User profiles (Factory Pattern)
├── Services/           # Core subsystems & business logic coordinates
│   ├── AccountManager.cs     # Coordinates login & session state
│   └── BookstoreSystem.cs    # Facade coordinator (Singleton)
├── Views/              # ASP.NET Core Razor Pages (Views)
│   └── Home/
│       ├── Index.cshtml      # Book catalogue explorer
│       ├── Cart.cshtml       # Cart checkout & delivery info form
│       ├── Login.cshtml      # Customer authentication portal
│       └── Signup.cshtml     # Customer registration portal
├── wwwroot/            # Static assets
│   ├── css/styles.css        # Premium custom CSS styling
│   └── js/script.js          # Core frontend AJAX operations & DOM interactions
├── Infrastructure/     # Persistence layers
│   └── data/                 # JSON databases (books.json, users.json)
├── Program.cs          # Application startup, DI configuration, and routes
├── TestRunner.cs       # In-memory E2E integration verification tests
└── appsettings.json    # Local configuration file
```

---

## 📝 Frontend Routes

The application uses dynamic page routing mapped via the `HomeController`. Navigate to the following paths in your browser:

* **`/`** or **`/Home/Index`** — Book catalogue and cart preview sidebar.
* **`/Home/Cart`** — Dedicated cart page and checkout invoice form.
* **`/Home/Login`** — Member sign-in page.
* **`/Home/Signup`** — New customer account registration.

---

## 🔌 Backend REST API Endpoints

The javascript frontend interacts asynchronously with the following controller endpoints:

```text
GET  /api/books      # Loads current book data and active stock counts
POST /api/signup     # Registers a customer account into users.json
POST /api/login      # Validates credentials against users.json
POST /api/guest      # Instantiates a guest shopper session
POST /api/checkout   # Validates billing/shipping, decrements stock, & registers invoice
```

---

## ✅ Dual-Layer Address Input Validation

To ensure extreme data integrity, the system implements a robust dual-layer validation for the **State** and **Postcode** inputs:

### Client-Side Validation (Frontend)
1. **Interactive Restrictions (`wwwroot/js/script.js`)**: 
   - **State Input**: Instantly filters non-alphabetical characters, caps length to `3`, and automatically capitalizes typed letters.
   - **Postcode Input**: Automatically strips any non-digit input and caps length to exactly `4` digits.
2. **HTML5 Constraints (`Views/Home/Cart.cshtml`)**:
   - Enforces specific matching patterns (`pattern` attribute) and displays helpful informative prompt popups (`title` attribute) upon field mismatches.
3. **Visual Feedback (`wwwroot/css/styles.css`)**:
   - Uses `:focus:invalid` and `:focus` selectors to transition input boundaries with a clear red warning outline if invalid input is entered.

### Domain-Model Validation (Backend)
The `Address.IsValid()` method performs strict, safe server-side validation:
- **State Check**: Ensures the state is a recognized abbreviation of the 8 official Australian states or territories: `VIC`, `NSW`, `QLD`, `WA`, `SA`, `TAS`, `ACT`, or `NT` (case-insensitively).
- **Postcode Check**: Validates that the postal code is exactly a `4`-digit numeric string.
