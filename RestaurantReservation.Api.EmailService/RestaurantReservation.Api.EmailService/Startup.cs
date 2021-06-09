using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Plain.RabbitMQ;
using RabbitMQ.Client;
using RestaurantReservation.Api.EmailService.BackgroundService;
using RestaurantReservation.Api.EmailService.DB;
using RestaurantReservation.Api.EmailService.MemoryStorage;

namespace RestaurantReservation.Api.EmailService
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
            #region Database Connection
            services.AddTransient<IDataProvider>(d => new DataProvider(Configuration.GetConnectionString("DefaultConnection")));
            #endregion

            services.AddControllers();

            #region Swagger
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Email API",
                    Description = "Email Microservice API for Restaurant Reservation API",
                    Version = "v1"
                });
            });
            #endregion

            #region RabbitMQ - Consumer
            services.AddSingleton<IConnectionProvider>(new ConnectionProvider(Configuration["RabbitMQLocalHost"]));
            services.AddSingleton<ISubscriber>(x =>
                new Subscriber(
                    connectionProvider: x.GetService<IConnectionProvider>(),
                    exchange: "email_exchange",
                    queue: "email_queue",
                    routingKey: "email.*",
                    exchangeType: ExchangeType.Topic
                )
            );
            #endregion

            #region Background Service
            services.AddHostedService<DataCollector>();
            #endregion

            #region Register Interface Service
            services.AddSingleton<IMemoryResultStorage, MemoryResultStorage>();
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            #region Swagger Middleware
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Email API");
            });
            #endregion
        }
    }
}
