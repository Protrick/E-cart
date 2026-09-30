# E-Cart Enterprise • Full-Stack E-Commerce Solution

A high-performance enterprise e-commerce platform built with a **C# .NET 10 Web API Backend** following **Clean Architecture**, **Neon PostgreSQL Database**, **JWT Authentication & Role-Based Access Control**, and a modern **React Frontend**.

---

## 🏛️ System Architecture

```
e-cart/
├── backend/
│   ├── ECart.slnx
│   └── src/
│       ├── ECart.Domain/          # Core Domain Entities, Enums & Custom Exceptions
│       │   ├── Entities/          # User, Product, Category, Order, OrderItem
│       │   ├── Enums/             # UserRole (Admin/Customer), OrderStatus
│       │   ├── Exceptions/        # AppException, NotFound, Validation, Conflict, Unauthorized
│       │   └── Common/            # BaseEntity, ApiResponse<T>, PagedResult<T>
│       ├── ECart.Application/     # Business Logic, DTOs & Interfaces
│       │   ├── DTOs/              # Auth, Products, Categories, Orders
│       │   ├── Interfaces/        # Repositories & Services contracts
│       │   └── Services/          # AuthService, ProductService, CategoryService, OrderService
│       ├── ECart.Infrastructure/  # EF Core, PostgreSQL / NeonDB & Security
│       │   ├── Data/              # AppDbContext, DbInitializer (Seed Data)
│       │   ├── Repositories/      # UnitOfWork, GenericRepository, Product/Category/Order repos
│       │   └── Services/          # BcryptPasswordHasher, JwtTokenGenerator
│       └── ECart.Api/             # RESTful API Controllers & Middlewares
│           ├── Controllers/       # Auth, Products, Categories, Orders, Health
│           ├── Middleware/        # GlobalExceptionMiddleware, RequestLoggingMiddleware
│           ├── Program.cs         # Serilog, RateLimiting, JWT, CORS, Swagger OpenAPI
│           └── appsettings.json   # ConnectionStrings, JWT, Rate Limiting configs
│
└── frontend/                      # Modern React + Vite Frontend
    ├── src/
    │   ├── context/               # AuthContext, CartContext
    │   ├── components/            # Navbar, ProductCard, ProductDetailModal, CartDrawer, AuthModal, AdminPanel, OrdersModal, Toast
    │   ├── services/              # Centralized API fetch client with JWT Bearer
    │   ├── App.jsx                # Main Storefront & Modals orchestration
    │   └── index.css              # Custom CSS Glassmorphism Design System
    └── package.json
```

---

## 📋 Fulfilled Requirements

| # | Requirement | Implementation |
|---|---|---|
| 1 | **Secure User Registration** | `POST /api/auth/register` with BCrypt hashing (WorkFactor 11) |
| 2 | **Authentication (Username/Password)** | `POST /api/auth/login` validating username/email and password |
| 3 | **JWT Token Generation** | Tokens signed with HMAC-SHA256 containing User ID, Role, Email, and Expiry claims |
| 4 | **JWT Token Validation** | `JwtBearer` authentication middleware with strict issuer/audience/signature checks |
| 5 | **Role-Based Authorization** | `[Authorize(Roles = "Admin")]` and `[Authorize(Roles = "Customer")]` |
| 6 | **Product Retrieval** | `GET /api/products` (Search, Category filter, Price range, In-stock, Sorting, Pagination) |
| 7 | **Admin CRUD for Products** | `POST`, `PUT`, `DELETE` endpoints on `/api/products` restricted to `Admin` role |
| 8 | **Complete Product CRUD** | Full CRUD for products & categories with SKU & Slug conflict detection |
| 9 | **Centralized Exception Handling** | `GlobalExceptionMiddleware` mapping custom domain exceptions to standard RFC 7807 problem responses |
| 10 | **Application Logging** | Structured logging via Serilog (Console + Rolling log files in `logs/ecart-.log`) + `RequestLoggingMiddleware` |
| 11 | **Rate Limiting** | ASP.NET Core `AddRateLimiter` fixed window policy returning 429 Too Many Requests |
| 12 | **Clear HTTP Status Codes** | Standardized `ApiResponse<T>` envelope for 200, 201, 400, 401, 403, 404, 409, 429, 500 |
| 13 | **Separation of Concerns** | Domain, Application, Infrastructure, and API layers |
| 14 | **Multi-Frontend Support** | RESTful JSON API with CORS enabled for `http://localhost:5173` |
| 15 | **Hosting Foundation** | Configurable for IIS, Docker, Kestrel, and Cloud Platforms |

---

## 🗄️ Neon PostgreSQL Setup

To connect to your **Neon PostgreSQL** database:

1. Open `backend/src/ECart.Api/appsettings.json`.
2. Update `ConnectionStrings:DefaultConnection` with your NeonDB connection string:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=ep-xxxx-xxxx.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=YOUR_ACTUAL_NEON_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
   }
   ```
3. Or supply via environment variable:
   ```bash
   export DATABASE_URL="Host=ep-xxxx-xxxx.us-east-2.aws.neon.tech;Database=neondb;Username=neondb_owner;Password=YOUR_ACTUAL_NEON_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
   ```
4. When the API starts up, `DbInitializer` will automatically create the tables and seed default demo users and products into your NeonDB database.

---

## 🚀 Running the Application

### 1. Start the Backend API
```bash
cd backend/src/ECart.Api
dotnet run --launch-profile http
```
- API Base URL: `http://localhost:5000`
- Interactive Swagger UI: `http://localhost:5000` (or `/swagger`)
- Health Check: `http://localhost:5000/api/health`

### 2. Start the React Frontend
```bash
cd frontend
npm install
npm run dev
```
- Storefront URL: `http://localhost:5173`

---

## 🔑 Demo Accounts

| Role | Email / Username | Password | Access Privileges |
|---|---|---|---|
| **Admin** | `admin@ecart.com` / `admin` | `Admin@12345` | Product CRUD, Category CRUD, View all platform orders, Change order statuses, View users directory |
| **Customer** | `customer@ecart.com` / `johndoe` | `Customer@12345` | Browse catalog, Filter & search products, Manage shopping cart, Place orders, View order history |
