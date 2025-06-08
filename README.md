# HRApp – HR Management WPF Application

HRApp is a desktop solution for automating HR processes in educational institutions. The application covers the full employee life cycle: hiring, orders, document management, time tracking, reminders and report generation with export to Word/Excel.

## Main features

### Employees
- Maintain a list of employees with filtering and search.
- Employee card based on the official T‑2 form (all personal and HR data).
- Digital labor book with saving and export.
- Support for dismissal with date tracking.
- Attach and store employee CVs.

### Orders
- Orders for hiring, transfers, dismissal, vacations, business trips and sick leaves.
- Automatic order numbering (`PREFIX-YEAR-NUMBER`).
- Export of orders to Word using templates.

### Documents and journals
- Incoming, outgoing and internal documents.
- Journals for orders and document registration.

### Time sheet
- Automatic generation of a monthly time sheet.
- Accounting for vacations, sick leaves, weekends and working days.
- Summary totals for attendance and absences.
- Export of the time sheet to Excel.

### Reminders
- Deadlines for vacations, certifications, contracts, IDs and trainings.
- Customizable period (3/7/30 days).
- "Processed" mark and filtering.
- Export reminders to Excel.
- Telegram notifications at application start.

### Reports
- Orders, vacations, sick leaves and employee list for any period.
- Citizenship report.
- Export of all reports to Excel.

### Awards
- Register employee awards (date, number, type and department).
- Award form linked to the employee.
- Export award lists to Excel.

## Technology stack
| Technology                          | Purpose                                   |
| ----------------------------------- | ----------------------------------------- |
| **.NET 8 / WPF**                    | Main application framework and GUI        |
| **Entity Framework Core (SQLite)**  | Embedded database (SQLite file)           |
| **ClosedXML**                       | Export of reports and time sheet to Excel |
| **Xceed.DocX**                      | Export of documents and orders to Word    |
| **MaterialDesignInXAML**            | Modern UI styles and controls             |
| **MVVM (partially)**                | Separation of logic and presentation      |

## Prerequisites
- .NET SDK 8.0 or later
- Visual Studio 2022/2023 with the **.NET desktop development** workload

## Setup and run
1. Clone this repository.
2. Open `HRApp.sln` in Visual Studio.
3. Restore NuGet packages if they do not restore automatically.
4. Build the solution.
5. On first run a SQLite database will be created in `%AppData%\HRApp\hrdatabase.db`. Entity Framework Core migrations will update the schema automatically.
6. Press **F5** or choose *Start without debugging* to run the application.

### Telegram configuration
Some reminder functions can send notifications to Telegram. To enable this, edit `Helpers/TelegramHelper.cs` and set your bot token and chat id:
```csharp
private const string BotToken = "YOUR_BOT_TOKEN"; // from https://t.me/BotFather
private const string ChatId = "YOUR_CHAT_ID";   // from https://t.me/userinfobot
```
Without these values the application will run but Telegram notifications will not be sent.