using Laba1_2.Data;
using Laba1_2.Models;
using Laba1_2.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // SignIn settings
    options.SignIn.RequireConfirmedEmail = false;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();

// Configure cookie policy
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});

// Add MVC services
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Add custom services
builder.Services.AddScoped<ICodeExecutionService, CodeExecutionService>();
builder.Services.AddScoped<IChallengeService, ChallengeService>();

// Додаємо сервіс для роботи з сесіями
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Додаємо сервіс для HttpContext
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Configure area routing
app.MapAreaControllerRoute(
    name: "StudentArea",
    areaName: "Student",
    pattern: "Student/{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "MentorArea",
    areaName: "Mentor",
    pattern: "Mentor/{controller=Home}/{action=Index}/{id?}");

app.MapAreaControllerRoute(
    name: "AdminArea",
    areaName: "Admin",
    pattern: "Admin/{controller=Home}/{action=Index}/{id?}");

// Configure default routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Initialize database and roles
// Initialize database and roles
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await InitializeDatabase(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

app.Run();

async Task InitializeDatabase(IServiceProvider services)
{
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Check if database exists and apply migrations
        if ((await context.Database.GetPendingMigrationsAsync()).Any())
        {
            Console.WriteLine("Applying pending migrations...");
            await context.Database.MigrateAsync();
            Console.WriteLine("Migrations applied successfully.");
        }
        else
        {
            Console.WriteLine("Database is up to date.");
        }

        // Create roles
        string[] roles = { "Admin", "Mentor", "Student" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
                Console.WriteLine($"Created role: {role}");
            }
        }

        // Create admin user
        var adminEmail = "admin@codelearning.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "System",
                LastName = "Administrator",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("Admin user created successfully.");
            }
            else
            {
                Console.WriteLine("Failed to create admin user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Create sample mentor user
        var mentorEmail = "mentor@codelearning.com";
        var mentorUser = await userManager.FindByEmailAsync(mentorEmail);

        if (mentorUser == null)
        {
            mentorUser = new User
            {
                UserName = mentorEmail,
                Email = mentorEmail,
                FirstName = "John",
                LastName = "Mentor",
                Bio = "Experienced programming instructor",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(mentorUser, "Mentor123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(mentorUser, "Mentor");
                Console.WriteLine("Mentor user created successfully.");
            }
            else
            {
                Console.WriteLine("Failed to create mentor user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Create sample student user
        var studentEmail = "student@codelearning.com";
        var studentUser = await userManager.FindByEmailAsync(studentEmail);

        if (studentUser == null)
        {
            studentUser = new User
            {
                UserName = studentEmail,
                Email = studentEmail,
                FirstName = "Jane",
                LastName = "Student",
                Bio = "Learning programming",
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(studentUser, "Student123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
                Console.WriteLine("Student user created successfully.");
            }
            else
            {
                Console.WriteLine("Failed to create student user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }

        // Додаємо деякі тестові дані для мов програмування
        if (context.Languages != null && !context.Languages.Any())
        {
            var languages = new[]
            {
                new Language { Name = "Python", Extension = ".py", Version = "3.9", SyntaxHighlighting = "python", IsActive = true },
                new Language { Name = "JavaScript", Extension = ".js", Version = "ES6", SyntaxHighlighting = "javascript", IsActive = true },
                new Language { Name = "C#", Extension = ".cs", Version = "9.0", SyntaxHighlighting = "csharp", IsActive = true },
                new Language { Name = "Java", Extension = ".java", Version = "17", SyntaxHighlighting = "java", IsActive = true },
                new Language { Name = "C++", Extension = ".cpp", Version = "C++17", SyntaxHighlighting = "cpp", IsActive = true },
                new Language { Name = "TypeScript", Extension = ".ts", Version = "4.5", SyntaxHighlighting = "typescript", IsActive = true }
            };

            await context.Languages.AddRangeAsync(languages);
            await context.SaveChangesAsync();
            Console.WriteLine("Sample languages added successfully.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
        throw;
    }
}