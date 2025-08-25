# NetScraperDemo

**Author:** Erick Esquilin  
**Last updated:** August 22, 2025  

---

## 📌 Overview

This project demonstrates how I set up a **React (Vite + TypeScript + MUI) frontend** with an **ASP.NET Core minimal API backend**.  

Key highlights:
- Local development using **Entity Framework Core InMemory** database (for quick dev without external dependencies).
- Seamless switch to **SQL Server** when available.
- Backend exposes CRUD endpoints for Groups/Terms.
- Frontend uses Axios and TypeScript for typed API calls.
- CORS configured to allow frontend/backend interaction in dev.

The goal was to create a repeatable **local dev SOP** while ensuring scalability for production SQL usage.

---

## 🛠️ Prerequisites

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)  
- [Node.js 20+ (LTS)](https://nodejs.org)  
- (Optional) [Docker Desktop](https://www.docker.com/) — for running SQL locally  

---

## 🚀 Backend (ASP.NET Core Minimal API)

**Project folder:** `backend/NetScraper.Api`

### Steps I Took
1. **Created the API Project**
   ```bash
   dotnet new web -n NetScraper.Api
   cd NetScraper.Api
   ```

2. **Added EF Core Packages**
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore.SqlServer
   dotnet add package Microsoft.EntityFrameworkCore.Design
   dotnet add package Microsoft.EntityFrameworkCore.InMemory
   ```

3. **Models Example**
   ```csharp
   // SearchGroup.cs
   namespace NetScraper.Api.Models;
   public class SearchGroup {
       public int Id { get; set; }
       public string Name { get; set; } = "";
       public ICollection<SearchTerm> Terms { get; set; } = new List<SearchTerm>();
   }
   ```

   ```csharp
   // SearchTerm.cs
   namespace NetScraper.Api.Models;
   public class SearchTerm {
       public int Id { get; set; }
       public string Term { get; set; } = "";
       public DateTime? StartDate { get; set; }
       public DateTime? EndDate { get; set; }
       public string? OutputQuery { get; set; }
       public int SearchGroupId { get; set; }
       public SearchGroup? SearchGroup { get; set; }
   }
   ```

4. **DbContext Example**
   ```csharp
   public class AppDbContext(DbContextOptions<AppDbContext> opts) : DbContext(opts) {
       public DbSet<SearchGroup> SearchGroups => Set<SearchGroup>();
       public DbSet<SearchTerm> SearchTerms => Set<SearchTerm>();

       protected override void OnModelCreating(ModelBuilder b) {
           b.Entity<SearchGroup>().HasIndex(g => g.Name).IsUnique();
           b.Entity<SearchTerm>()
               .HasOne(t => t.SearchGroup)
               .WithMany(g => g.Terms)
               .HasForeignKey(t => t.SearchGroupId)
               .OnDelete(DeleteBehavior.Cascade);
       }
   }
   ```

5. **Endpoints Provided**
   - `GET /api/groups` → List groups  
   - `POST /api/groups` → Create group  
   - `DELETE /api/groups/{id}` → Delete group  
   - `GET /api/groups/{id}/terms` → List terms by group  
   - `POST /api/terms` → Create term  
   - `DELETE /api/terms/{id}` → Delete term  

6. **Features**
   - Toggle between **InMemory** and **SQL Server**.
   - **CORS policy** to allow frontend communication.
   - **Health check endpoint** for DB availability.
   - Auto-migration enabled when using SQL Server.

---

## 🎨 Frontend (Vite + React + TypeScript + MUI)

**Project folder:** `frontend/netscrapper`

### Steps I Took
1. **Created the Vite Project**
   ```bash
   npm create vite@latest netscrapper -- --template react-ts
   cd netscrapper
   npm install
   ```

2. **Installed UI & Utilities**
   ```bash
   npm install @mui/material @emotion/react @emotion/styled                @mui/icons-material @mui/x-date-pickers dayjs axios
   ```

3. **API Helper Example (`src/api.ts`)**
   ```ts
   import axios from "axios";
   export const BASE_URL = import.meta.env.VITE_API_BASE ?? "http://localhost:5088";
   export const api = axios.create({ baseURL: BASE_URL, withCredentials: true });

   export async function getGroups() {
     const { data } = await api.get("/api/groups");
     return data;
   }
   ```

4. **Frontend Notes**
   - Used **MUI Tables & Icons** for UI consistency.
   - Integrated **Day.js date pickers**.
   - `.env` file stores API base URL (`VITE_API_BASE=http://localhost:5207`).
   - Run locally with:
     ```bash
     npm run dev
     ```

---

## ⚡ Common Gotchas (and Fixes)

- Missing `dayjs` → Install with `npm i dayjs`.  
- Vite couldn't resolve `"./api"` → File extension typo (`api.ts.txt`).  
- CORS issues → Updated backend secrets to allow `http://localhost:5173`.  
- Migration conflicts → Re-ran `dotnet ef database update`.  
- Backend crashes off DB → Enabled **UseInMemory** mode.  

---

## ✅ Status

- Fully working **local dev setup** (React + ASP.NET Core + EF Core).  
- Can switch seamlessly between **InMemory DB** and **SQL Server**.  
- Frontend and backend communicate with CORS enabled.  

---

## 📚 References

- [ASP.NET Core Minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)  
- [Entity Framework Core](https://learn.microsoft.com/ef/core/)  
- [Vite Docs](https://vitejs.dev/guide/)  
- [React Docs](https://react.dev/)  
- [MUI Docs](https://mui.com/)  

---
