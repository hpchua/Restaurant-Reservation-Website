using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Plain.RabbitMQ;
using RabbitMQ.Client;
using RestaurantReservation.Api.SwaggerConfigurations;
using RestaurantReservation.Core.Entities;
using RestaurantReservation.Core.Interfaces;
using RestaurantReservation.Core.Services;
using RestaurantReservation.Infrastructure.Data;
using RestaurantReservation.Infrastructure.Repositories;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text;

namespace RestaurantReservation.Api
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
            #region Entity Framework
            services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });
            #endregion

            #region Identity 
            services.AddIdentity<ApplicationUser, IdentityRole>(configs =>
            {
                //Disable Password Requirements
                configs.Password.RequiredLength = 6;
                configs.Password.RequireDigit = false;
                configs.Password.RequireNonAlphanumeric = false;
                configs.Password.RequireUppercase = false;
                configs.Password.RequireLowercase = false;
                //Enable Unique Email option
                configs.User.RequireUniqueEmail = true;
            }).AddRoles<IdentityRole>()
              .AddEntityFrameworkStores<DatabaseContext>()
              .AddDefaultTokenProviders();
            #endregion

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            });

            #region Seed Data
            services.AddScoped<IDbInitializer, DbInitializer>();
            #endregion

            #region Json Web Token (JWT)  
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    //Jwt Bearer -> Check the request is authorized
                    .AddJwtBearer(jwt =>
                    {
                        jwt.SaveToken = true;
                        jwt.RequireHttpsMetadata = true;
                        jwt.Audience = Configuration["JWTConfig:ValidAudience"];
                        jwt.TokenValidationParameters = new TokenValidationParameters()
                        {
                            ValidateIssuer = true,
                            ValidateAudience = true,
                            ValidateIssuerSigningKey = true,
                            ValidateLifetime = true,
                            ValidAudience = Configuration["JWTConfig:ValidAudience"],
                            ValidIssuer = Configuration["JWTConfig:ValidIssuer"],
                            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWTConfig:Secret"]))
                        };
                    });
            #endregion

            #region API Versioning
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.ApiVersionReader = new MediaTypeApiVersionReader();
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ApiVersionSelector = new CurrentImplementationApiVersionSelector(options);
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });
            #endregion

            #region Swagger Generator
            services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, SwaggerGeneralConfig>();
            services.AddSwaggerGen();
            #endregion

            #region RabbitMQ - Publisher
            services.AddSingleton<IConnectionProvider>(new ConnectionProvider(Configuration["RabbitMQLocalHost"]));
            services.AddScoped<IPublisher>(x =>
                new Publisher(
                    connectionProvider: x.GetService<IConnectionProvider>(),
                    exchange: "email_exchange",
                    exchangeType: ExchangeType.Topic
                )
            );
            #endregion

            #region Register interface service
            // Promotion Created Email Sent Status
            services.AddScoped<IPromotionEmail, PromotionEmail>();

            // RestaurantReservation.Core
            services.AddScoped<IBookingService, BookingService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IPromotionService, PromotionService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IRestaurantCategoryService, RestaurantCategoryService>();
            services.AddScoped<IRestaurantService, RestaurantService>();
            services.AddScoped<IRestaurantScheduleService, RestaurantScheduleService>();
            services.AddScoped<IUserService, UserService>();

            // RestaurantReservation.Core.Interfaces | RestaurantReservation.Infrastructure.Repositories
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IRestaurantRepository, RestaurantRepository>();
            services.AddScoped<IRestaurantCategoryRepository, RestaurantCategoryRepository>();
            services.AddScoped<IRestaurantScheduleRepository, RestaurantScheduleRepository>();
            services.AddScoped<IPromotionRepository, PromotionRepository>();
            services.AddScoped<IBookingRepository, BookingRepository>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IDbInitializer dbInitializer, IApiVersionDescriptionProvider apiVersionProvider, IPromotionEmail promotionEmail)
        {
            #region Swagger Middleware
            //Serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(setupAction =>
            {
                foreach (var desc in apiVersionProvider.ApiVersionDescriptions)
                    setupAction.SwaggerEndpoint($"/swagger/{desc.GroupName}/swagger.json", $"Restaurant Reservation API {desc.GroupName}");

                setupAction.RoutePrefix = string.Empty;
            });
            #endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            #region Seed Data
            dbInitializer.Initialize();
            #endregion

            #region Promotion Email
            promotionEmail.CheckSentStatus();
            #endregion

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
