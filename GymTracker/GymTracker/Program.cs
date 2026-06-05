using GymTracker.Client.Pages;
using GymTracker.Components;
using GymTracker.Components.Account;
using GymTracker.Data;
using GymTracker.Models;
using GymTracker.Services;
using GymTrackerBusinessService.Generic;
using GymTrackerDataModel;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using GymTrackerBusinessService.Repository;


var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();

    return new HttpClient
    {
        BaseAddress = new Uri(nav.BaseUri)
    };
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, PersistingServerAuthenticationStateProvider>();
builder.Services.AddHttpClient();
builder.Services.AddAuthorization();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<EntityDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddScoped(typeof(IGenericRepoService<>), typeof(GenericRepoService<>));
builder.Services.AddScoped(typeof(IWorkoutExerciseSetService), typeof(WorkoutExerciseSetService));
builder.Services.AddScoped(typeof(IExerciseService), typeof(ExerciseService));
builder.Services.AddScoped(typeof(IWorkoutScheduleService), typeof(WorkoutScheduleService));
builder.Services.AddScoped(typeof(IWorkoutHistory), typeof(WorkoutHistory));


builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailSender<ApplicationUser>, SmtpEmailSender>();
builder.Services.AddMudServices();
builder.Services.AddCascadingAuthenticationState();

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
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(GymTracker.Client._Imports).Assembly);

app.MapAdditionalIdentityEndpoints();
app.MapPost("/api/login", async (
    HttpContext context,
    SignInManager<ApplicationUser> signInManager,
    [FromForm] string Email,
    [FromForm] string Password,
    [FromForm] string? returnUrl,
    [FromForm] bool RememberMe=  false) =>
{
    var result = await signInManager.PasswordSignInAsync(
        Email,
        Password,
        RememberMe,
        lockoutOnFailure: true);

    if (!result.Succeeded)
        return Results.BadRequest("Invalid login");
    var target = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
    return Results.LocalRedirect(target);

}).DisableAntiforgery();

app.MapPost("/api/logout", async (
    HttpContext context,
    SignInManager<ApplicationUser> signInManager) =>
{
    await signInManager.SignOutAsync();

    return Results.Redirect("/");
}).DisableAntiforgery(); ;
app.Run();
