using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RestaurantReservation.Core.Utils;
using RestaurantReservation.Frontend.Logger;
using RestaurantReservation.Frontend.Services;
using RestaurantReservation.Frontend.Services.Interfaces;
using System;

namespace RestaurantReservation.Frontend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            #region Enable Session
            services.AddSession(configs =>
            {
                configs.IdleTimeout = TimeSpan.FromMinutes(30);
                configs.Cookie.HttpOnly = true;  // Mitigate the risk of client side script accessing the protected cookie 
                configs.Cookie.IsEssential = true;
                configs.Cookie.SameSite = SameSiteMode.Lax;
            });
            #endregion

            #region Cookie Schema [Authorize]
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.AccessDeniedPath = "/AccessDenied"; //Redirect to AccessDenied when Unauthorize User access

                options.Cookie.Name = "Restaurant-Reservation-System";
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                options.LoginPath = "/Account/Login";   //Force Unauthorize User to Login Page
                options.LogoutPath = "/Account/Logout";
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
            });
            #endregion

            #region Policy and Claim
            services.AddAuthorization(options =>
            {
                options.AddPolicy(SD.Policy.ADMIN_ONLY, policy => policy.RequireAuthenticatedUser().RequireRole(SD.ROLE_ADMIN));    //Admin
                options.AddPolicy(SD.Policy.MEMBER_ONLY, policy => policy.RequireAuthenticatedUser().RequireRole(SD.ROLE_MEMBER));  //Member
                options.AddPolicy(SD.Policy.AUTHENTICATED_ONLY, policy => policy.RequireAuthenticatedUser());
            });
            #endregion

            #region HTTP Client
            services.AddHttpClient("api", (serviceProvider, client) =>
            {
                client.BaseAddress = new Uri(Configuration["APIServer:BaseAddress"]);
                client.DefaultRequestHeaders.Add("Accept", SD.CONTENT_JSON);
            });
            #endregion

            #region Razor Pages Settings
            services.AddRazorPages()
                    .AddRazorRuntimeCompilation()
                    .AddRazorPagesOptions(options =>
                    {
                        //Set the Folder to specific policy
                        options.Conventions.AuthorizeFolder("/Admins", SD.Policy.ADMIN_ONLY);
                        options.Conventions.AuthorizeFolder("/Members", SD.Policy.MEMBER_ONLY);
                        options.Conventions.AuthorizeFolder("/Restaurants", SD.Policy.AUTHENTICATED_ONLY);
                        options.Conventions.AuthorizeFolder("/Account/Profile", SD.Policy.AUTHENTICATED_ONLY);
                    })
                    .AddSessionStateTempDataProvider();  //Enable Session state based TempData storage
            #endregion

            #region Register interface service
            //NLog
            services.AddSingleton<ILog, LogNLog>();

            services.AddScoped<IAuthenticateService, AuthenticateService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IRestaurantService, RestaurantService>();
            services.AddScoped<IScheduleService, ScheduleService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IBookingService, BookingService>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // Exception For Centralised Error -> launchSettings.json
                app.UseExceptionHandler("/Error");

                #region Centralised 404 error handling
                app.UseStatusCodePagesWithReExecute("/StatusCode?code={0}");
                #endregion

                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            #region Identity Purposes
            //who are you?
            app.UseAuthentication();
            //are you allowed?
            app.UseAuthorization();
            #endregion

            #region Session
            app.UseSession();
            #endregion

            /* EndPointRoutingMiddleware -> Set Routing for Razor Pages */
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
