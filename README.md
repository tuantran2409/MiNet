# MiNet - Social Networking Platform

MiNet is a modern social networking platform built with **ASP.NET Core MVC** and **.NET 9.0**. It features real-time notifications, post sharing, stories, and social connectivity.

## 🚀 Features

- **Social Interaction**: Create posts with hashtags, follow friends, and share stories.
- **Real-time Notifications**: Instant updates using **SignalR**.
- **Authentication**: Secure login with **ASP.NET Core Identity**, plus **Google** and **GitHub** OAuth integration.
- **Admin Dashboard**: Comprehensive management tools for users and content.
- **Responsive Design**: Modern UI designed for a seamless user experience.

## 🛠️ Tech Stack

- **Backend**: .NET 9.0 (C#)
- **Database**: SQL Server (EF Core 9.0)
- **Frontend**: HTML5, CSS3, JavaScript (SignalR Client)
- **Auth**: ASP.NET Core Identity, OAuth 2.0 (Google, GitHub)

## ⚙️ Setup Instructions

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server](https://www.microsoft.com/sql-server/) (LocalDB or Express)
- [Visual Studio 2022](https://visualstudio.microsoft.com/vs/) (or VS Code / JetBrains Rider)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/tuantran2409/MiNet.git
   cd MiNet
   ```

2. **Configure the Database**:
   Update the `DefaultConnection` in `MiNet/appsettings.json` with your connection string.
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MiNetDB;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

3. **Configure OAuth (Optional)**:
   Provide your **Google** and **GitHub** Client ID/Secret in `appsettings.json` to enable third-party logins.

4. **Run the Application**:
   The database will be automatically created and seeded on the first run.
   ```bash
   dotnet run --project MiNet
   ```
   Or open `MiNet.sln` in Visual Studio and press **F5**.

## 📂 Project Structure

- **MiNet/**: The main Web Application project (MVC).
- **MiNet.Data/**: The Data Access layer (Models, DbContext, Services).

---
*Developed with ❤️ by [tuantran2409](https://github.com/tuantran2409)*
