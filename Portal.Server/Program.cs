using Microsoft.AspNetCore.HttpOverrides;
using Portal.Server;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls();
Console.WriteLine("Adding databases and identity services");
builder.AddDatabases();
builder.AddIdentityServices();

builder.AddOtherServices();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});
WebApplication? app = builder.Build();

app.Use((context, next) =>
{
    if (context.Request.Scheme != "https")
    {
        context.Request.Scheme = "https";
    }
    return next(context);
});
app.UseForwardedHeaders();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    _ = app.UseExceptionHandler("/Error");
    _ = app.UseHsts();
}
else
{
    _ = app.UseDeveloperExceptionPage();
    app.UseWebAssemblyDebugging();
}

// Only use HTTPS redirection in development or when not behind a proxy
if (!app.Environment.IsDevelopment())
{
    _ = app.UseHttpsRedirection();
}

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
