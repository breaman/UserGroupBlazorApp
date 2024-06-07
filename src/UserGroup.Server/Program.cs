using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using UserGroup.Data.Models;
using UserGroup.Server.Components;
using UserGroup.Server.Components.Auth;
using UserGroup.Server.Components.Emails;
using UserGroup.Shared.DTOs;

Serilog.Debugging.SelfLog.Enable(msg => Debug.WriteLine(msg));

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

bool isMigrations = Environment.GetCommandLineArgs()[0].Contains("ef.dll");

try
{
    var builder = WebApplication.CreateBuilder(args);
    
    // put things in here that only need to run when we aren't running migrations
    // good example of that is that we don't need to configure serilog when we aren't actually running our app
    if (!isMigrations)
    {
        builder.Host.UseSerilog((ctx, lc) => lc
            .ReadFrom.Configuration(ctx.Configuration));
    }

    // Add "core" services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveWebAssemblyComponents();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddHttpClient();
    // End "core" services to the container
    
    // Add blazor auth stuff
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<IdentityUserAccessor>();
    builder.Services.AddScoped<IdentityRedirectManager>();
    builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
    // To ensure custom claims are added to new identity when principal is refreshed.
    builder.Services.ConfigureOptions<ConfigureSecurityStampOptions>();
    // End blazor auth stuff
    
    // Add identity stuff
    builder.Services.AddAuthorization();
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddIdentityCookies();

    builder.Services.AddIdentityCore<User>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequiredLength = 6;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;

            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddSignInManager()
        .AddDefaultTokenProviders();
    // End identity stuff
    
    // Add application stuff
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")).EnableSensitiveDataLogging());
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddSingleton<IEnhancedEmailSender<User>, IdentityNoOpEmailSender>();
    // End application stuff
    
    // Add FluentValidation stuff
    builder.Services.AddTransient<IValidator<RegisterDto>, RegisterDtoValidator>();
    builder.Services.AddTransient<IValidator<LoginDto>, LoginDtoValidator>();
    // End FluentValidation stuff

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        app.UseMigrationsEndPoint();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();

    app.UseStaticFiles();
    app.UseAntiforgery();

    app.MapRazorComponents<App>()
        .AddInteractiveWebAssemblyRenderMode()
        .AddAdditionalAssemblies(typeof(UserGroup.Client._Imports).Assembly);

    app.MapAdditionalIdentityEndpoints();

    if (!isMigrations)
    {
        using var scope = app.Services.CreateScope();
        var serviceProvider = scope.ServiceProvider;

        InitializeData.Initialize(serviceProvider);
    }

    app.Run();
}
// don't log a fatal exception if it was triggered by the migrations ending
catch (Exception ex) when (ex.GetType().Name is not "StopTheHostException" && ex.GetType().Name is not "HostAbortedException")
{
    Log.Fatal(ex, "Unhandled Exception.");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}
