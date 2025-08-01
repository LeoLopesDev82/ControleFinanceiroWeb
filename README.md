💰 ControleFinanceiroWeb
ControleFinanceiroWeb is a modern web application for personal finance management, built using ASP.NET Core MVC. It is a complete web migration of a previous desktop application (Windows Forms), now featuring a responsive design, clean architecture, and full integration between front-end and back-end in a single project.

🧾 Overview
The system allows users to manage financial statements (bank or credit card transactions), with automatic categorization based on configured keywords. It also provides summary reports, category management, and type-of-statement tracking — all with a focus on usability and maintainability.

✅ Features
💼 Statement Management
Add transactions manually or import from spreadsheets

Filter by custom date ranges

Automatically categorize entries using keyword matching

🏷️ Category Management
Full CRUD for categories (e.g., Utilities, Entertainment, Food)

Add identifiers (keywords) to help auto-categorize entries

Prevent duplicate entries by name validation

📂 Statement Type Management
Manage types of accounts/statements (e.g., Checking Account, Credit Card)

Prevent deletion when linked to existing data

📊 Reporting
View financial summaries grouped by category and date

Quickly identify uncategorized entries for manual correction

🛠️ Tech Stack
Layer	Technologies
Backend	ASP.NET Core MVC, C#, LINQ, Entity Framework Core
Frontend	HTML5, CSS3, Vanilla JavaScript (no external libs)
Database	Firebird 3.0 (.FDB local file)
Architecture	Clean MVC pattern with layered business services

🌐 Modern UI
The user interface is minimalist and responsive, featuring:

Green as the primary theme color 💚

Rounded elements for a modern look

Clean JavaScript fetch-based form submissions

Smooth UX with no external dependencies

🚀 Getting Started
Install Firebird 3.0 on your machine.

Clone this repository and open it with Visual Studio 2022+.

Update the connection string in Program.cs to point to your local .fdb database file:

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseFirebird(@"User=SYSDBA;Password=masterkey;Database=C:\YourPath\DATABASE.FDB;..."));
    
Run the project (F5) and access it via your browser.

✅ Make sure the Firebird service is running before launching the app.

🎯 Why This Project?
This project showcases:

Full-stack ASP.NET Core MVC development

Clean separation of concerns using a service-based architecture

Efficient front-end and back-end communication using modern JavaScript

Real-world implementation of Entity Framework Core + Firebird

A working finance control system with practical features and validations

📄 License
This project is open source under the MIT License.