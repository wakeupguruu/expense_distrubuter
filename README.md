# ExpenseSplitter.Cli

## Description
ExpenseSplitter helps groups track shared expenses and compute fair settlements. It includes a CLI for quick usage and a Web app for a richer UI.

## Installation
- Prerequisite: .NET SDK 8.0+
- Clone the repository:
```bash
git clone https://github.com/<your-username>/<your-repo>.git
cd <your-repo>
```
- Restore and build:
```bash
dotnet restore
dotnet build
```

## How to run
- CLI application:
```bash
dotnet run --project ExpenseSplitter.Cli -- [args]
```
- Web application:
```bash
dotnet run --project ExpenseSplitter.Web
```
Then open the URL shown in the console output.

## How to use (CLI)
- Launch the CLI (see above). You will be asked for a group name.
- Use the menu to manage members and expenses:
  - 1) Add member — add participants (e.g., Alice, Bob).
  - 2) List members — view current members.
  - 3) Add expense — enter description, amount, payer, and choose split mode:
    - Split equally among all members
    - Split equally among selected members
    - Custom amounts for selected members
  - 4) Show balances — see who owes and who is owed
  - 5) Show expenses — list recorded expenses and splits
  - 6) Settlement suggestion — suggested transfers to settle up


Notes:
- Balances: negative means the person is owed money; positive means they owe.
- For custom splits, the sum must equal the total amount. The app will warn if you assign 0 to selected participants.
