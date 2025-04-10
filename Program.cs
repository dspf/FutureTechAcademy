using FutureTechAcademy.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);
    

// Add services to the container.
builder.Services.AddControllersWithViews();


// Configure Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // Default to Google
})
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Redirect to login if unauthorized
        options.AccessDeniedPath = "/Account/AccessDenied"; // Handle access denied
    })
    .AddGoogle(options =>
    {
        options.ClientId = builder.Configuration["Google:ClientId"];
        options.ClientSecret = builder.Configuration["Google:ClientSecret"];
    });
//blobs
builder.Services.AddSingleton<BlobStorageService>();
// Configure Cosmos DB
builder.Services.AddSingleton<CosmosDbService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new CosmosDbService(config);
});
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["AzureBlobStorage:ConnectionString1:blobServiceUri"]!).WithName("AzureBlobStorage:ConnectionString1");
    clientBuilder.AddQueueServiceClient(builder.Configuration["AzureBlobStorage:ConnectionString1:queueServiceUri"]!).WithName("AzureBlobStorage:ConnectionString1");
    clientBuilder.AddTableServiceClient(builder.Configuration["AzureBlobStorage:ConnectionString1:tableServiceUri"]!).WithName("AzureBlobStorage:ConnectionString1");
});




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
