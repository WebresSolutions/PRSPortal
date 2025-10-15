//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using Portal.Server.Data;
//using Portal.Server.Models;
//using Quartz;
//using static OpenIddict.Abstractions.OpenIddictConstants;

//namespace Portal.Server;

//public class Startup(IConfiguration configuration)
//{
//    public IConfiguration Configuration { get; } = configuration;

//    public void ConfigureServices(IServiceCollection services)
//    {
//        _ = services.AddDbContext<AuthDBContext>(options =>
//        {
//            // Configure the context to use sqlite.
//            _ = options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-Portal-server.sqlite3")}");

//            // Register the entity sets needed by OpenIddict.
//            // Note: use the generic overload if you need
//            // to replace the default OpenIddict entities.
//            _ = options.UseOpenIddict();
//        });

//        // Register the Identity services.
//        _ = services.AddIdentity<ApplicationUser, IdentityRole>()
//            .AddEntityFrameworkStores<AuthDBContext>()
//            .AddDefaultTokenProviders()
//            .AddDefaultUI();

//        // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
//        // (like pruning orphaned authorizations/tokens from the database) at regular intervals.
//        _ = services.AddQuartz(options =>
//        {
//            options.UseSimpleTypeLoader();
//            options.UseInMemoryStore();
//        });

//        // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
//        _ = services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);

//        _ = services.AddOpenIddict()

//            // Register the OpenIddict core components.
//            .AddCore(options =>
//             {
//                 // Configure OpenIddict to use the Entity Framework Core stores and models.
//                 // Note: call ReplaceDefaultEntities() to replace the default OpenIddict entities.
//                 _ = options.UseEntityFrameworkCore()
//                        .UseDbContext<AuthDBContext>();

//                 // Enable Quartz.NET integration.
//                 _ = options.UseQuartz();
//             })

//            // Register the OpenIddict client components.
//            .AddClient(options =>
//            {
//                // Note: this sample uses the code flow, but you can enable the other flows if necessary.
//                _ = options.AllowAuthorizationCodeFlow();

//                // Register the signing and encryption credentials used to protect
//                // sensitive data like the state tokens produced by OpenIddict.
//                _ = options.AddDevelopmentEncryptionCertificate()
//                       .AddDevelopmentSigningCertificate();

//                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
//                _ = options.UseAspNetCore()
//                       .EnableStatusCodePagesIntegration()
//                       .EnableRedirectionEndpointPassthrough();

//                // Register the System.Net.Http integration and use the identity of the current
//                // assembly as a more specific user agent, which can be useful when dealing with
//                // providers that use the user agent as a way to throttle requests (e.g Reddit).
//                _ = options.UseSystemNetHttp()
//                       .SetProductInformation(typeof(Startup).Assembly);

//                // Register the Web providers integrations.
//                //
//                // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
//                // URI per provider, unless all the registered providers support returning a special "iss"
//                // parameter containing their URL as part of authorization responses. For more information,
//                // see https://datatracker.ietf.org/doc/html/draft-ietf-oauth-security-topics#section-4.4.
//                _ = options.UseWebProviders()
//                       .AddGitHub(options =>
//                       {
//                           _ = options.SetClientId("c4ade52327b01ddacff3")
//                                  .SetClientSecret("da6bed851b75e317bf6b2cb67013679d9467c122")
//                                  .SetRedirectUri("callback/login/github");
//                       });
//            })

//            // Register the OpenIddict server components.
//            .AddServer(options =>
//            {
//                // Enable the authorization, logout, token and userinfo endpoints.
//                _ = options.SetAuthorizationEndpointUris("connect/authorize")
//                       .SetEndSessionEndpointUris("connect/logout")
//                       .SetTokenEndpointUris("connect/token")
//                       .SetUserInfoEndpointUris("connect/userinfo");

//                // Mark the "email", "profile" and "roles" scopes as supported scopes.
//                _ = options.RegisterScopes(Scopes.Email, Scopes.Profile, Scopes.Roles);

//                // Note: the sample uses the code and refresh token flows but you can enable
//                // the other flows if you need to support implicit, password or client credentials.
//                _ = options.AllowAuthorizationCodeFlow()
//                       .AllowRefreshTokenFlow();

//                // Register the signing and encryption credentials.
//                _ = options.AddDevelopmentEncryptionCertificate()
//                       .AddDevelopmentSigningCertificate();

//                // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
//                _ = options.UseAspNetCore()
//                       .EnableAuthorizationEndpointPassthrough()
//                       .EnableEndSessionEndpointPassthrough()
//                       .EnableStatusCodePagesIntegration()
//                       .EnableTokenEndpointPassthrough();
//            })

//            // Register the OpenIddict validation components.
//            .AddValidation(options =>
//            {
//                // Import the configuration from the local OpenIddict server instance.
//                _ = options.UseLocalServer();

//                // Register the ASP.NET Core host.
//                _ = options.UseAspNetCore();
//            });

//        _ = services.AddControllersWithViews();
//        _ = services.AddRazorPages();

//        // Register the worker responsible for seeding the database.
//        // Note: in a real world application, this step should be part of a setup script.
//        _ = services.AddHostedService<Worker>();
//    }

//    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
//    {
//        if (env.IsDevelopment())
//        {
//            _ = app.UseDeveloperExceptionPage();
//            app.UseWebAssemblyDebugging();
//        }
//        else
//        {
//            _ = app.UseExceptionHandler("/Error");
//            _ = app.UseHsts();
//        }

//        _ = app.UseHttpsRedirection();
//        _ = app.UseBlazorFrameworkFiles();
//        _ = app.UseStaticFiles();

//        _ = app.UseRouting();

//        _ = app.UseAuthentication();
//        _ = app.UseAuthorization();

//        _ = app.UseEndpoints(endpoints =>
//        {
//            _ = endpoints.MapRazorPages();
//            _ = endpoints.MapControllers();
//            _ = endpoints.MapFallbackToFile("index.html");
//        });
//    }
//}
