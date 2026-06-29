💰 ControleFinanceiroWeb is a modern web application for personal finance management, built using ASP.NET Core MVC. It is a complete web migration of a previous desktop application (Windows Forms), now featuring a responsive design, clean architecture, full integration between front-end and back-end in a single project, and robust automated tests.

🧾 Overview
The system allows users to manage financial statements (bank or credit card transactions) with automatic categorization based on configured keywords. It also provides summary reports, category management, account statement tracking, and asynchronous database flows designed for high performance.

✅ Features
💼 Statement Management
- Add transactions manually or import from spreadsheets via tab-delimited clipboard copy-paste.
- Filter by dynamic monthly date ranges (automatically defaults to the current month).
- Automatically categorize entries using keyword matching.

🏷️ Category Management
- Full CRUD for categories (e.g., Utilities, Entertainment, Food).
- Configure identifiers (keywords) to help auto-categorize entries.
- Prevent duplicate entries by name validation.
- Strongly-typed category entry types (Fixed vs. Variable).

📂 Statement Type Management
- Manage types of accounts/statements (e.g., Checking Account, Credit Card).
- Prevent deletion when linked to existing data.

📊 Reporting
- View financial summaries grouped by category and date.
- Quickly identify uncategorized entries for manual correction.

⚡ Performance & Best Practices
- **Asynchronous Execution**: Fully migrated I/O-bound database operations to `async/await` patterns to maximize thread pool efficiency and prevent resource starvation.
- **Strongly-typed Enums**: Replaced raw magic characters ('F'/'V') for category types with a compiled `CategoryType` enum, mapped via EF Core Value Converters.
- **Dynamic Filtering**: Date ranges now compute dynamically based on the current system time, keeping the dashboard populated by default.
- **Automated Unit Testing**: Includes a dedicated xUnit test suite covering business algorithms, input parsing, and model validation.

🛠️ Tech Stack
- **Backend**: ASP.NET Core MVC (net9.0), C#, LINQ, Entity Framework Core 9.
- **Testing**: xUnit, FluentAssertions, .NET Test SDK.
- **Frontend**: HTML5, CSS3 (Bootstrap 5, Bootstrap Icons), Vanilla JavaScript (fetch-based asynchronous API calls).
- **Database**: Firebird 3.0 (.FDB local file).

🌐 Modern UI
The user interface is minimalist and responsive, featuring:
- Green as the primary theme color 💚
- Rounded elements for a modern look.
- Clean JavaScript fetch-based form submissions.
- Interactive charts rendered dynamically with Chart.js.

🧪 Testing
The project includes a comprehensive suite of unit tests verifying critical components. To run the tests, execute:
```bash
dotnet test
```
The test suite covers:
- **Business Logic**: Automatically matching category keyword configurations against transaction descriptions.
- **Sanitization Helpers**: Clean parsing of currency inputs (Brazilian Real `R$ 1.500,50` -> `1500.50m`), integer parsing, and date conversions.
- **Data Validation**: ViewModel annotations (`[Required]`, `[MaxLength]`) tested to prevent corrupted entries.

🚀 Getting Started
1. Install Firebird 3.0 on your machine.
2. Clone this repository and open it with Visual Studio 2022+ or Rider.
3. Make sure the Firebird service is running before launching the app.
4. Run the project (F5) and access it via your browser. 
*(The application connection string dynamically maps the relative path of the local `DATABASE.FDB` file, so no hardcoded absolute path edits are required in development)*

🎯 Why This Project?
This project showcases:
- Full-stack ASP.NET Core MVC development.
- Clean separation of concerns using a service-based architecture.
- Thread-safe asynchronous database access.
- High developer experience (DX) with self-resolving database paths and dynamic date defaults.
- Reliable C# unit testing.
- Efficient front-end and back-end communication using modern JavaScript fetch APIs.

📄 License
This project is open source under the MIT License.
