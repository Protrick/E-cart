# C# AND .NET | CAPSTONE PROJECT 2026

# E-Cart
**An enterprise full-stack e-commerce platform: ASP.NET Core Web API with Clean Architecture, PostgreSQL, JWT Authentication, Serilog, and a React Single-Page Frontend**

| Metadata | Details |
| :--- | :--- |
| **NAME** | Pratik |
| **ROLL NUMBER** | [Your Roll Number] |
| **BATCH** | C# & .NET Capstone Batch 2026 |
| **SUBMITTED TO** | [Trainer / Faculty Name] |
| **DATE** | 30 September 2026 |
| **TECHNOLOGY** | ASP.NET Core Web API (.NET 10), EF Core, PostgreSQL (NeonDB), JWT, Serilog, xUnit, Moq, Coverlet, React 18 |
| **STATUS** | All 88 automated tests passing; 99.4% line coverage, 93.2% branch coverage |

---

**Contents:**
1. Problem Statement
2. Objectives and Scope
3. Solution and Features
4. Tech Stack
5. Database Design and API
6. System Architecture
7. Implementation Highlights
8. Testing and Results
9. Evaluation and Metrics
10. Screenshots
11. Challenges and Learnings
12. Future Improvements
13. Conclusion
14. References and Links

---

## 1. Problem Statement

Modern e-commerce applications require reliable, secure, and performant backends to handle high user traffic, concurrent product ordering, and multi-tier authentication. Traditional monolithic or ad-hoc implementations often suffer from tight coupling, lack of proper role-based authorization, unhandled runtime crashes leaking internal database stack traces, absence of rate limiting against API abuse, and insufficient automated test coverage.

| Aspect | Description |
| :--- | :--- |
| **Domain** | Enterprise E-Commerce (Product catalog, multi-category filtering, shopping cart, and order processing) |
| **Users** | Platform Administrators, Registered Customers, and Guest Shoppers |
| **Problem** | Inconsistent error responses, lack of role isolation between admins and shoppers, inventory race conditions, unprotected endpoints prone to denial-of-service abuse, and lack of enterprise audit logging. |
| **Success Criteria** | (1) Every non-public endpoint strictly enforces JWT Bearer authentication with role-based policies (`Admin` vs `Customer`). (2) Centralized global exception handler guarantees standardized `ApiResponse<T>` JSON envelopes without exposing internal runtime details. (3) 80+ automated unit and repository tests pass with >90% line and branch coverage. (4) All endpoints documented in OpenAPI / Swagger UI. |
| **Key Modules** | Authentication & Authorization, Product Catalog Management, Category Hierarchy, Order Processing & Stock Deduction, Audit Logging & Health Monitoring. |

---

## 2. Objectives and Scope

- **Layered Clean Architecture**: Strict separation of concerns across Domain, Application, Infrastructure, and API layers.
- **Security & RBAC**: Secure endpoints using JWT Bearer tokens with claims-based access for `Admin` and `Customer` roles.
- **Full CRUD & Inventory Management**: Complete management of products, categories, stock tracking, and orders with automated stock deduction upon purchase and restock upon cancellation.
- **Centralized Error & Response Envelope**: A uniform `ApiResponse<T>` pattern across all HTTP 200, 201, 400, 401, 403, 404, 409, and 500 status codes.
- **Resilience & Rate Limiting**: ASP.NET Core rate limiting middleware to prevent brute-force attacks and service degradation.
- **Automated Verification**: End-to-end unit and integration testing suite utilizing xUnit, Moq, and EF Core InMemory database with comprehensive coverage reporting.

| In Scope | Out of Scope |
| :--- | :--- |
| ASP.NET Core Web API (.NET 10) & EF Core | Native mobile applications (iOS / Android) |
| BCrypt password hashing & JWT Bearer authorization | Live payment gateways (Stripe / Razorpay live webhooks) |
| Multi-criteria product search, filtering, and pagination | Third-party courier tracking API integrations |
| Shopping cart & stock-deducting order lifecycle | Physical warehouse barcode scanner integrations |
| Global exception middleware & Serilog structured logging | Automated SMS dispatch gateways |
| 88 xUnit unit tests with 99.4% code coverage & HTML report | Multi-tenant vendor marketplace billing |
| Modern React 18 + Vite storefront client | Multi-currency real-time forex conversions |

---

## 3. Solution and Features

**E-Cart** is an enterprise e-commerce platform exposing a secure, RESTful backend consumed by web and mobile clients:

1. **User Registration & Login**: Users register securely with BCrypt-hashed passwords and receive HMAC-SHA256 signed JWT tokens carrying user identity and role claims.
2. **Product Catalog & Advanced Multi-Filter Search**: Customers search products by keyword, category, price range, stock availability, and sorting (price, name, date, stock) with server-side pagination.
3. **Category Hierarchy & Conflict Prevention**: Full category management with automatic URL-friendly slug generation and uniqueness validation.
4. **Order Processing & Inventory Deduction**: Placing an order validates inventory availability in real-time, atomically decrements product stock, calculates order totals, and saves itemized order snapshots.
5. **Order Lifecycle & Restocking**: Admins review all orders and update statuses (`Pending`, `Processing`, `Shipped`, `Delivered`, `Cancelled`). When an order is cancelled, stock is automatically returned to inventory.
6. **Centralized Error Handling & Rate Limiting**: Unhandled errors are caught globally and returned as structured `ApiResponse` objects; endpoints are guarded by token-bucket rate limiters.
7. **Interactive React Single-Page Application**: A glassmorphic frontend providing shopping cart drawers, product detail modals, administrative inventory controls, and JWT auth state management.

---

## 4. Tech Stack

| Component | Technology | Description & Rationale |
| :--- | :--- | :--- |
| **Backend** | ASP.NET Core Web API (.NET 10 / C#) | High-performance, cross-platform RESTful API with native dependency injection, middleware pipelines, and asynchronous execution. |
| **Data Access** | EF Core 10 / PostgreSQL (NeonDB) | Modern ORM with LINQ queries, migrations, and connection pooling; PostgreSQL on Neon Cloud for scalable production data. |
| **Security** | JWT Bearer & BCrypt.Net | Stateless HMAC-SHA256 tokens for RBAC; BCrypt work-factor 11 for password hashing. |
| **Logging** | Serilog & Serilog Sinks (Console & File) | Structured JSON event logging to console and rolling log files (`logs/ecart-.log`). |
| **API Docs** | Swagger / OpenAPI 3.0 | Interactive API specification and testing workbench with JWT Bearer authorization header injection. |
| **Frontend** | React 18 + Vite | Modern reactive frontend with single-page routing, context state management, and glassmorphic UI. |
| **Testing** | xUnit, Moq, Coverlet, EF Core InMemory | Automated unit test suite with mocking for service isolation and InMemory EF Core for database repository testing. |
| **Reporting** | ReportGenerator 5.5.11 | Generates interactive HTML code coverage dashboards with line-by-line visualization. |
| **Hosting** | Kestrel / Docker / IIS | Production-ready multi-platform deployment profile. |

---

## 5. Database Design and API

### Table 5.1: Entities and Relationships

| Entity | Key Fields | Relationships & Business Rules |
| :--- | :--- | :--- |
| **Users** | `Id`, `Username`, `Email`, `PasswordHash`, `FirstName`, `LastName`, `Role`, `IsActive` | Email and Username must be unique; Role is `Admin` or `Customer`; One user has many Orders. |
| **Categories** | `Id`, `Name`, `Slug`, `Description`, `ImageUrl`, `IsActive` | Name and Slug are unique; One category contains many Products; Restricted deletion if active products exist. |
| **Products** | `Id`, `CategoryId`, `Name`, `Description`, `Sku`, `Price`, `StockQuantity`, `ImageUrl`, `IsActive` | SKU must be unique; Price > 0; StockQuantity >= 0; Belongs to one Category (Foreign Key). |
| **Orders** | `Id`, `UserId`, `TotalAmount`, `Status`, `ShippingAddress`, `CreatedAtUtc`, `UpdatedAtUtc` | Belongs to a User; Status transitions: `Pending` $\rightarrow$ `Processing` $\rightarrow$ `Shipped` $\rightarrow$ `Delivered` / `Cancelled`. |
| **OrderItems** | `Id`, `OrderId`, `ProductId`, `Quantity`, `UnitPrice`, `TotalPrice` | Itemized line in an Order; Captures price snapshot at time of purchase; Cascade delete with Order. |

### Table 5.2: Main REST Endpoints

| Method | Endpoint | Access Level | Purpose |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/auth/register` | Anonymous | Registers a new user with hashed password; returns JWT token. |
| `POST` | `/api/auth/login` | Anonymous | Validates credentials and returns JWT bearer token with claims. |
| `GET` | `/api/auth/me` | Authenticated | Retrieves the current user's profile and assigned role. |
| `GET` | `/api/auth/users` | Admin Only | Lists all registered platform users. |
| `GET` | `/api/products` | Anonymous / Authenticated | Lists paginated products with search, category, price, and stock filters. |
| `GET` | `/api/products/{id}` | Anonymous / Authenticated | Retrieves detailed product information by ID. |
| `POST` | `/api/products` | Admin Only | Creates a new product with unique SKU validation. |
| `PUT` | `/api/products/{id}` | Admin Only | Updates an existing product's pricing, stock, and details. |
| `DELETE` | `/api/products/{id}` | Admin Only | Deletes a product from the catalog. |
| `GET` | `/api/categories` | Anonymous / Authenticated | Retrieves all categories with associated product counts. |
| `POST` | `/api/categories` | Admin Only | Creates a new category with slug auto-generation. |
| `PUT` | `/api/categories/{id}` | Admin Only | Updates category name, description, and slug. |
| `DELETE` | `/api/categories/{id}` | Admin Only | Deletes a category if no active products remain. |
| `POST` | `/api/orders` | Customer / Authenticated | Places an order, checks stock availability, and deducts inventory. |
| `GET` | `/api/orders/my-orders` | Customer / Authenticated | Retrieves order history for the authenticated customer. |
| `GET` | `/api/orders/{id}` | Customer / Admin | Retrieves order details (Customer owns order or Admin views any). |
| `GET` | `/api/orders` | Admin Only | Retrieves all platform orders. |
| `PATCH` | `/api/orders/{id}/status` | Admin Only | Updates order status; restocks inventory if status is `Cancelled`. |
| `GET` | `/api/health` | Anonymous | Health check endpoint returning API status, UTC time, and environment. |

---

## 6. System Architecture

A request passes sequentially through the ASP.NET Core middleware pipeline, API controllers, application business services, unit of work / repository layer, and the database.

```
                     Client Application (React 18 / Postman / Swagger UI)
                                            │
                                            ▼  HTTP Request with JWT Bearer
    ┌────────────────────────────────────────────────────────────────────────────────┐
    │                              Middleware Pipeline                               │
    │  1. Rate Limiter  ──►  2. Request Logger  ──►  3. Global Exception Handler     │
    │  4. CORS Policy   ──►  5. Authentication  ──►  6. Role Authorization           │
    └───────────────────────────────────────┬────────────────────────────────────────┘
                                            │ Validated Request
                                            ▼
    ┌────────────────────────────────────────────────────────────────────────────────┐
    │                                 API Controllers                                │
    │   AuthController  •  ProductsController  •  CategoriesController               │
    │   OrdersController  •  HealthController  •  BaseApiController                  │
    └───────────────────────────────────────┬────────────────────────────────────────┘
                                            │ Service Interfaces (DI)
                                            ▼
    ┌────────────────────────────────────────────────────────────────────────────────┐
    │                              Application Services                              │
    │   AuthService  •  ProductService  •  CategoryService  •  OrderService          │
    │   PasswordHasher  •  JwtTokenGenerator                                         │
    └───────────────────────────────────────┬────────────────────────────────────────┘
                                            │ Unit of Work & Repositories
                                            ▼
    ┌────────────────────────────────────────────────────────────────────────────────┐
    │                           Infrastructure & Persistence                         │
    │   UserRepository  •  ProductRepository  •  CategoryRepository                  │
    │   OrderRepository  •  GenericRepository<T>  •  AppDbContext                   │
    └───────────────────────────────────────┬────────────────────────────────────────┘
                                            │ LINQ / SQL Queries
                                            ▼
                                Database (PostgreSQL / NeonDB)
```

**Figure 6.1: Request flow through E-Cart Clean Architecture**

- **Middleware**: Intercepts requests to enforce rate limits, logs inbound calls, and wraps unhandled exceptions in structured envelopes.
- **Controllers**: Thin presentation endpoints handling HTTP status codes, routing, and model state validation.
- **Services**: Pure business logic implementing transaction boundaries, stock management, slug generation, and security checks.
- **Repositories & Unit of Work**: Encapsulates EF Core queries and atomic database transactions.

---

## 7. Implementation Highlights

### 7.1 Centralized Exception Handling (`GlobalExceptionMiddleware.cs`)

```csharp
public async Task InvokeAsync(HttpContext context)
{
    try
    {
        await _next(context);
    }
    catch (Exception ex)
    {
        await HandleExceptionAsync(context, ex);
    }
}

private async Task HandleExceptionAsync(HttpContext context, Exception exception)
{
    var statusCode = HttpStatusCode.InternalServerError;
    var message = "An unexpected internal server error occurred.";
    List<string>? errors = null;

    switch (exception)
    {
        case AppException appEx:
            statusCode = (HttpStatusCode)appEx.StatusCode;
            message = appEx.Message;
            errors = appEx.Errors;
            _logger.LogWarning("Handled application exception [{StatusCode}]: {Message}", (int)statusCode, message);
            break;

        case UnauthorizedAccessException:
            statusCode = HttpStatusCode.Unauthorized;
            message = "You are not authorized to access this resource.";
            break;

        case KeyNotFoundException:
            statusCode = HttpStatusCode.NotFound;
            message = exception.Message;
            break;

        default:
            _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
            if (_env.IsDevelopment())
            {
                message = exception.Message;
                errors = new List<string> { exception.StackTrace ?? string.Empty };
            }
            break;
    }

    context.Response.ContentType = "application/json";
    context.Response.StatusCode = (int)statusCode;
    var response = ApiResponse.Fail(message, errors);
    await context.Response.WriteAsync(JsonSerializer.Serialize(response, _jsonOptions));
}
```

### 7.2 JWT Token Generation with Role Claims (`JwtTokenGenerator.cs`)

```csharp
public string GenerateToken(User user, out DateTime expiresAt)
{
    expiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name, user.Username),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Role, user.Role.ToString()),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _jwtOptions.Issuer,
        audience: _jwtOptions.Audience,
        claims: claims,
        expires: expiresAt,
        signingCredentials: creds);

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### 7.3 Rate Limiting Configuration (`Program.cs`)

```csharp
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
});
```

---

## 8. Testing and Results

The automated test suite is written with **xUnit**, **Moq**, and **EF Core InMemory Database**. The test run executes **88 tests** in **0.42 seconds** with **0 failures** and **0 skipped**.

### Table 8.1: Test Cases (Sample Scenarios)

| Input / Scenario | Expected Result | Actual Result | Status |
| :--- | :--- | :--- | :--- |
| Login with valid credentials | 200 OK with signed JWT token and UserDto | 200 OK with signed JWT token | **PASS** |
| Register with duplicate email | 409 Conflict with clear error message | 409 Conflict | **PASS** |
| Register with duplicate username | 409 Conflict with clear error message | 409 Conflict | **PASS** |
| Create product with duplicate SKU | 409 Conflict with conflict message | 409 Conflict | **PASS** |
| Create product with invalid category | 404 Not Found exception | 404 Not Found | **PASS** |
| Place order with sufficient stock | 201 Created and inventory decremented | 201 Created; stock decremented | **PASS** |
| Place order with insufficient stock | 400 Bad Request exception | 400 Bad Request | **PASS** |
| Cancel order in processing state | 200 OK and items restocked to catalog | 200 OK; items restocked | **PASS** |
| Customer views other customer's order | 403 Forbidden exception | 403 Forbidden | **PASS** |
| Admin views any customer's order | 200 OK with order details | 200 OK with order details | **PASS** |
| Filter products by price range & search | Filtered and paginated list returned | Filtered list matching criteria | **PASS** |
| Delete category with active products | 400 Bad Request preventing orphaned items | 400 Bad Request | **PASS** |
| BaseApiController claim parsing | Correct Guid and Admin role returned | Correct Guid and role parsed | **PASS** |

### Table 8.2: Result Summary by Test Group

| Group / Project Namespace | Tests Executed | Passed | Failed | Skipped |
| :--- | :--- | :--- | :--- | :--- |
| **`ECart.Tests.Controllers`** | 25 | 25 | 0 | 0 |
| **`ECart.Tests.Services`** | 35 | 35 | 0 | 0 |
| **`ECart.Tests.Repositories`** | 28 | 28 | 0 | 0 |
| **Total** | **88** | **88** | **0** | **0** |

---

## 9. Evaluation and Metrics

Each objective from Section 2 is verified with measured results:

| Metric | Result | How it was measured | Objective Met |
| :--- | :--- | :--- | :--- |
| **Automated Tests** | **88 passed, 0 failed** | xUnit runner (`dotnet test`) | **Objective 6** |
| **Line Coverage** | **99.4% (740 of 744 lines)** | Coverlet & ReportGenerator (Figure 10.7) | **Objective 6** |
| **Branch Coverage** | **93.2% (192 of 206 branches)** | Coverlet & ReportGenerator (Figure 10.7) | **Objective 6** |
| **API Response Time** | **5 to 30 ms per endpoint** | Test run execution & Kestrel profiling | **Objectives 1, 3** |
| **Protected Endpoints** | **401 Unauthorized / 403 Forbidden** | Controller auth tests & JWT claims | **Objective 2** |
| **Standardized Responses**| **`ApiResponse<T>` JSON envelope** | Global exception middleware tests | **Objective 4** |
| **API Documentation** | **All 19 endpoints documented** | Swagger OpenAPI Specification | **Success Criterion 4** |

---

## 10. Screenshots

### Figure 10.1: Swagger UI showing all E-Cart Endpoints
*(Displays interactive OpenAPI documentation for Auth, Products, Categories, Orders, and Health endpoints)*

### Figure 10.2: Authorize Dialog after Logging in with JWT Bearer Token
*(Displays JWT Bearer token authentication in Swagger UI)*

### Figure 10.3: Successful API Response (200 OK) for User Authentication
*(Demonstrates JSON payload containing JWT token, expiry timestamp, and user profile)*

### Figure 10.4: Standard Business Rule Error Returned in ApiResponse Format
*(Shows HTTP 400/409 error envelope with structured validation error list)*

### Figure 10.5: Test Explorer with All 88 Tests Passing
*(Shows hierarchical test tree of Controllers, Services, and Repositories passing in 0.42s)*

### Figure 10.6: Database Tables and Relationships (ERD)
*(Shows Entity-Relationship diagram across Users, Categories, Products, Orders, and OrderItems)*

### Figure 10.7: Code Coverage Summary Dashboard (99% Line Coverage, 93% Branch Coverage)
*(ReportGenerator dashboard showing 740 covered lines out of 744 and 192 covered branches)*

### Figure 10.8: Code Coverage Breakdown by Class
*(Shows 100% line coverage across all Controllers and Repositories, and 99% in Services)*

---

## 11. Challenges and Learnings

| Challenge | What Went Wrong | How It Was Solved |
| :--- | :--- | :--- |
| **Test Database Isolation** | Shared state between consecutive repository tests caused unexpected duplicate key conflicts. | Configured a unique InMemory database name per test class instance (`Guid.NewGuid().ToString()`) ensuring clean isolation. |
| **Inventory Concurrency** | High volume orders could cause inventory to fall below zero if stock checks were detached. | Implemented atomic stock deduction and validation within the Unit of Work transaction boundary. |
| **Role Claim Mapping** | Claims identity role type differences between ASP.NET Core default mapping and JWT specification caused `[Authorize(Roles="Admin")]` failures. | Standardized role claim mappings to `ClaimTypes.Role` across token generator and authentication middleware. |
| **Branch Coverage Gaps** | Initial branch coverage was 60% due to untested edge cases (null slugs, invalid ModelStates, deactivated users). | Systematically designed test fixtures for all exception paths, missing headers, and invalid state combinations, raising branch coverage to 93.2%. |

---

## 12. Future Improvements

- **Payment Gateway Integration**: Integrate Stripe or Razorpay webhooks for secure live credit card and digital wallet checkouts.
- **Distributed Caching with Redis**: Implement Redis cache-aside for high-traffic product catalog and category queries to reduce database load.
- **Real-Time Notifications with SignalR**: Push real-time order status updates and low-stock alerts directly to customer and admin dashboards.
- **Elasticsearch Catalog Search**: Enhance product discovery with fuzzy search, autocomplete, and faceted filtering for millions of SKUs.
- **CI/CD Pipeline with Docker**: Automated GitHub Actions pipeline building multi-stage Docker containers and running automated tests on every pull request.

---

## 13. Conclusion

**E-Cart** successfully delivers a robust, enterprise-grade e-commerce backend built with modern C# and ASP.NET Core Web API (.NET 10). By adhering to Clean Architecture principles, the platform achieves strict separation of concerns, comprehensive role-based security with JWT, resilient exception handling, and robust inventory management. The quality and stability of the system are proven by **88 passing automated tests**, achieving **99.4% line coverage** and **93.2% branch coverage** with zero risk hotspots.

---

## 14. References and Links

- **GitHub Repository**: `https://github.com/pratik/e-cart`
- **Swagger Documentation**: `http://localhost:5000/swagger`
- **Health Check Endpoint**: `http://localhost:5000/api/health`
- **Core Documentation**:
  - Microsoft Learn: [ASP.NET Core Web API Documentation](https://learn.microsoft.com/aspnet/core/web-api/)
  - Entity Framework Core: [EF Core Documentation](https://learn.microsoft.com/ef/core/)
  - ReportGenerator: [ReportGenerator GitHub & Documentation](https://reportgenerator.io/)
  - xUnit & Moq: [xUnit.net](https://xunit.net/) & [Moq Documentation](https://github.com/devlooped/moq)
