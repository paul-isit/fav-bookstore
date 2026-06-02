# Favourite Books Online Bookstore System

This repository contains the implementation of the **Favourite Books Online Bookstore System**, developed for **SWE30003 - Software Architectures and Design**.

The system is implemented as an **ASP.NET Core MVC web application**. It uses the Model-View-Controller architecture to separate the user interface, application control logic, and object-oriented domain model.

## Project Overview

Favourite Books is a physical bookstore expanding into an online storefront. The system allows customers to browse books, manage a shopping cart, place orders, and view order history. It also supports staff and administrative tasks such as managing the book catalogue, handling shipments, and generating sales reports.

## Technology Stack

- **Framework:** ASP.NET Core MVC / ASP.NET Core Web API
- **Language:** C#
- **Frontend:** HTML, CSS, JavaScript in the `website` folder
- **Database:** JSON data files in `Infrastructure/data` for the current prototype
- **Architecture:** MVC/domain model with API endpoints used by the frontend

## Project Structure

```text
FavouriteBookstore/
├── Controllers/        # MVC controllers and API endpoints
├── Models/             # Domain classes and data models
├── Views/              # Original ASP.NET Razor views
├── website/            # Frontend HTML, CSS, and JavaScript pages
├── wwwroot/            # ASP.NET static assets
├── Infrastructure/     # JSON data files and persistence connector
├── Services/           # Business services and system facade
├── Program.cs          # Application startup and routing configuration
└── appsettings.json    # Application settings
```

## Running the Integrated Website

Run the app from the repository root, not from the `website` folder:

```bash
dotnet run --urls http://localhost:5142
```

Then open the frontend in a browser:

```text
http://localhost:5142/website/index.html
```

Do not use Python Live Server for the integrated version. The frontend now connects to backend API endpoints, so it needs the ASP.NET app running.

## Frontend Pages

```text
/website/index.html   # Catalogue and cart preview
/website/cart.html    # Full cart and checkout
/website/login.html   # Login page
/website/signup.html  # Signup page
```

## Backend API Connections

The JavaScript frontend calls these backend endpoints:

```text
GET  /api/books      # Loads current book data and stock counts
POST /api/signup     # Creates a customer account in users.json
POST /api/login      # Validates login against users.json
POST /api/checkout   # Processes checkout and updates stock counts in books.json
```

## Testing the Flow

1. Start the backend with `dotnet run --urls http://localhost:5142`.
2. Open `http://localhost:5142/website/index.html`.
3. Add books to the cart.
4. Open the cart preview or go to `cart.html`.
5. Checkout.
6. Refresh the catalogue and confirm the stock count decreased.

Signup and login can be tested through `signup.html` and `login.html`. User records are stored in:

```text
Infrastructure/data/users.json
```

Book stock is stored in:

```text
Infrastructure/data/books.json
```
