using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI;
using MinimalAPI.Domain.Interfaces;
using MinimalAPITest.Mocks;

namespace MinimalAPITest.Helpers
{
    public class Setup
    {
        public const string PORT = "5001";
        public static TestContext testContext = default!;
        public static WebApplicationFactory<Startup> http = default!;
        public static HttpClient client = default!;

        public static void ClassInit(TestContext testContext)
        {
            Setup.testContext = testContext;
            Setup.http = new WebApplicationFactory<Startup>();


            Setup.http = Setup.http.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("https_port", Setup.PORT)
                       .UseEnvironment("Testing")
                       .ConfigureServices(services =>
                       {
                           services.AddScoped<IUserService, UserServiceMock>();
                           services.AddScoped<IVehicleService, VehicleServiceMock>();                    
                       });
            });

            Setup.client = Setup.http.CreateClient();
        }

        public static void ClassCleanup()
        {
            if (Setup.http != null)     
                Setup.http.Dispose();           
        }
    }
}
