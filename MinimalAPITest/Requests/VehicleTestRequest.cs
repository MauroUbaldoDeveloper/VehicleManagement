using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.Entities;
using MinimalAPI.Domain.ModelViews;
using MinimalAPITest.Helpers;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace MinimalAPITest.Requests
{
    [TestClass]
    public class VehicleTestRequest
    {
        [ClassInitialize]
        public static async Task ClassInit(TestContext context)
        {
            Setup.ClassInit(context);
            await AuthenticateAsync();
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            Setup.ClassCleanup();
        }

        private static async Task AuthenticateAsync()
        {
            var loginDTO = new LoginDTO
            {
                Email = "adm@test.com",
                Password = "123456"
            };

            var loginContent = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");
            var loginResponse = await Setup.client.PostAsync("/users/login", loginContent);

            loginResponse.EnsureSuccessStatusCode();

            var loginResult = await loginResponse.Content.ReadAsStringAsync();
            var loginData = JsonSerializer.Deserialize<UserLogged>(loginResult, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (loginData == null || string.IsNullOrEmpty(loginData.Token))
                throw new InvalidOperationException("Failed to obtain authentication token.");

            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginData.Token);
        }

        [TestMethod]
        public async Task TestCreateVehicle()
        {
            var vehicleDTO = new VehicleDTO
            {
                Name = "Ninja ZX4-R",
                Mark = "Kawasaki",
                year = 2025
            };

            var content = new StringContent(JsonSerializer.Serialize(vehicleDTO), Encoding.UTF8, "application/json");

            var response = await Setup.client.PostAsync("/vehicles", content);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode, "A requisição não retornou sucesso.");

            var result = await response.Content.ReadAsStringAsync();
            var vehicleRegistered = JsonSerializer.Deserialize<Vehicle>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(vehicleRegistered);
            Assert.AreEqual("Ninja ZX4-R", vehicleRegistered.Name);
            Assert.AreEqual("Kawasaki", vehicleRegistered.Mark);
            Assert.AreEqual(2025, vehicleRegistered.year);
        }

        [TestMethod]
        public async Task TestGetVehiclesByPage()
        {
            int page = 1;
            var response = await Setup.client.GetAsync($"/vehicles?page={page}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();

            var vehicles = JsonSerializer.Deserialize<List<Vehicle>>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(vehicles);
            Assert.IsTrue(vehicles.Count > 0);
        }

        [TestMethod]
        public async Task TestGetVehiclesByID()
        {
            var vehicleID = 1;

            var response = await Setup.client.GetAsync($"/vehicles/{vehicleID}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var vehicle = JsonSerializer.Deserialize<Vehicle>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(vehicle);
            Assert.AreEqual(vehicleID, vehicle.Id);
        }

        [TestMethod]
        public async Task ModifyVehicleByID()
        {
            var vehicle = new Vehicle
            {
                Id = 2,
                Name = "Ninja 650",
                Mark = "Kawasaki",
                year = 2026
            };

            var content = new StringContent(JsonSerializer.Serialize(vehicle), Encoding.UTF8, "application/json");

            var response = await Setup.client.PutAsync($"/vehicles/{vehicle.Id}", content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, "Falha ao modificar o veículo.");

            var result = await response.Content.ReadAsStringAsync();
            var updatedVehicle = JsonSerializer.Deserialize<Vehicle>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(updatedVehicle);
            Assert.AreEqual("Ninja 650", updatedVehicle.Name);
            Assert.AreEqual("Kawasaki", updatedVehicle.Mark);
            Assert.AreEqual(2026, updatedVehicle.year);
        }

        [TestMethod]
        public async Task DeleteVehicleByID()
        {
            var vehicleID = 1;

            var response = await Setup.client.DeleteAsync($"/vehicles/{vehicleID}");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
        }
    }
}
