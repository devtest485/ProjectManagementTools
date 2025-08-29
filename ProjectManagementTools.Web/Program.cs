using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using ProjectManagementTools.Core.DTOs;
using ProjectManagementTools.Core.Entities.Auth;
using ProjectManagementTools.Core.Interfaces.Services;
using ProjectManagementTools.Infrastructure.Data;
using ProjectManagementTools.Web.Components;
using ProjectManagementTools.Web.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add services to the container
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add HttpContextAccessor first - this is crucial for Blazor Server
builder.Services.AddHttpContextAccessor();

// Add DbContext with proper configuration for Blazor Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Enable sensitive data logging in development only
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
}, ServiceLifetime.Scoped); // Explicitly set to Scoped

// Configure Identity with proper lifetime for Blazor Server
builder.Services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure Identity services for Blazor Server - This is crucial
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// IMPORTANT: Add this for Blazor Server Identity support
builder.Services.AddScoped<SignInManager<ApplicationUser>>();
builder.Services.AddScoped<UserManager<ApplicationUser>>();

// Register application services with explicit scoped lifetime using IServiceProvider pattern
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add AutoMapper with proper assembly scanning
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddMaps(Assembly.GetExecutingAssembly());
});

// Configure email options
builder.Services.Configure<EmailOptions>(builder.Configuration.GetSection("EmailSettings"));

// Add authorization with proper policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ProjectOwner", policy => policy.RequireRole("Admin", "ProjectOwner"));
    options.AddPolicy("Member", policy => policy.RequireRole("Admin", "ProjectOwner", "Member"));
});

// Add SignalR for real-time notifications with Blazor Server specific configuration
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

// Add additional services for better error handling
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    if (builder.Environment.IsDevelopment())
    {
        logging.AddDebug();
    }
});

// Add server-side rendering for better compatibility
builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = builder.Environment.IsDevelopment();
    options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
    options.DisconnectedCircuitMaxRetained = 100;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable routing
app.UseRouting();

// Authentication & Authorization - Order is important
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Initialize database with proper error handling and scope management
try
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            // Ensure database is created
            await context.Database.EnsureCreatedAsync();

            // Seed admin user if needed
            await SeedAdminUser(userManager, roleManager, logger);

            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database");
            throw;
        }
    }
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogCritical(ex, "Failed to initialize the application");
    throw;
}

app.Run();

// Helper method for seeding admin user
static async Task SeedAdminUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole<Guid>> roleManager, ILogger logger)
{
    try
    {
        string[] roles = { "Admin", "ProjectOwner", "Member", "Viewer" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                if (result.Succeeded)
                {
                    logger.LogInformation("Created role: {Role}", role);
                }
                else
                {
                    logger.LogError("Failed to create role {Role}: {Errors}", role,
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }

        var adminEmail = "admin@projectflow.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Admin user created successfully: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists: {Email}", adminEmail);
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error occurred while seeding admin user and roles");
        throw;
    }
}