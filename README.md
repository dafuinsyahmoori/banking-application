# 💰 Banking Application API

A simple and clean RESTful API for a basic banking system, built using **ASP.NET Core** and **C#**.  
This project supports core banking operations such as **sign in**, **deposit**, **withdraw**, and **transfer**, all secured using **cookie-based authentication**.

---

## 🚀 Features

- 🔐 **Sign In** (authentication via cookie)
- 💸 **Deposit** to account
- 💵 **Withdraw** from account
- 🔁 **Transfer** between accounts
- 📄 **API Documentation via Swagger**

---

## ⚙️ Tech Stack

- **Language**: C#
- **Framework**: ASP.NET Core
- **Authentication**: Cookie
- **API Testing**: Swagger UI

---

## 🧪 How to Run

Make sure you have [.NET SDK](https://dotnet.microsoft.com/en-us/download) installed.

Clone the project:
```bash
git clone https://github.com/dafuinsyahmoori/banking-application.git BankingApplication
cd BankingApplication
```

### 🛠️ Configuration

Before running the project, make sure your appsettings.json (or appsettings.{Environment}.json for specific environment) file is properly configured. See appsettings.Example.json for a basic example.

#### 📝 Notes:

- Change the ConnectionStrings.BankingApplication string to match your local database configuration.

Or you can use User Secret (highly recommended, only for Development Environment):

```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:BankingApplication" "Your Connection String"
```

Run the project:

```bash
dotnet run
```

After running, open your browser to access the Swagger UI:

```bash
http://localhost:5224/swagger