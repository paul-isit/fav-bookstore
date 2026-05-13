# Favourite Books Online Bookstore System

This repository contains the implementation of the **Favourite Books Online Bookstore System**, developed for **SWE30003 - Software Architectures and Design**.

The system is implemented as an **ASP.NET Core MVC web application**. It uses the Model-View-Controller architecture to separate the user interface, application control logic, and object-oriented domain model.

## Project Overview

Favourite Books is a physical bookstore expanding into an online storefront. The system allows customers to browse books, manage a shopping cart, place orders, and view order history. It also supports staff and administrative tasks such as managing the book catalogue, handling shipments, and generating sales reports.

## Technology Stack

- **Framework:** ASP.NET Core MVC
- **Language:** C#
- **Frontend:** HTML, CSS, JavaScript, Razor Views
- **Database:** SQL-based database, such as SQL Server or SQLite
- **Architecture:** Model-View-Controller with object-oriented backend design

## Project Structure

```text
FavouriteBookstore/
├── Controllers/        # Handles incoming web requests and coordinates application flow
├── Models/             # Contains domain classes and data models
├── Views/              # Razor pages used to render HTML responses
├── wwwroot/            # Static files such as CSS, JavaScript, and images
├── Data/               # Database context, seed data, and persistence-related code
├── Services/           # Business services and external service integrations
├── Program.cs          # Application startup and configuration
└── appsettings.json    # Application settings and database configuration