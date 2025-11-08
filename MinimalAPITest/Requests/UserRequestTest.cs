using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.DTOs.Enums;
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
    public class UserRequestTest
    {

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            Setup.ClassInit(context);
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

            var loginContent = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "application/json");
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
        public async Task TestLogin()
        {
            var loginDTO = new LoginDTO { Email = "adm@test.com", Password = "123456" };
            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

            var response = await Setup.client.PostAsync("/users/login", content);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var admLogged = JsonSerializer.Deserialize<UserLogged>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(admLogged);
            Assert.IsNotNull(admLogged.Email ?? "");
            Assert.IsNotNull(admLogged.Profile ?? "");
            Assert.IsNotNull(admLogged.Token ?? "");
        }

        [TestMethod]
        public async Task TestUserRegister()
        {
            await AuthenticateAsync();

            UserDTO userDTO = new UserDTO()
            {
                Email = "test25@test.com",
                Password = "testPassword",
                Profile = Profile.ADM
            };

            var content = new StringContent(JsonSerializer.Serialize(userDTO), Encoding.UTF8, "Application/json");

            var response = await Setup.client.PostAsync("/users", content);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();

            var userCreated = JsonSerializer.Deserialize<UserViewModel>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(userCreated);
        }

        [TestMethod]
        public async Task TestGetUsersByPage()
        {
            int page = 1;
            await AuthenticateAsync();

            var response = await Setup.client.GetAsync($"/users?page={page}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadAsStringAsync();

            var users = JsonSerializer.Deserialize<List<UserViewModel>>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(users);
            Assert.IsTrue(users.Count > 0);
        } 

        [TestMethod]
        public async Task TestGetUserByID()
        {
            var userID = 1;
            await AuthenticateAsync();

            var response = await Setup.client.GetAsync($"/users/{userID}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(user);
            Assert.AreEqual(userID, user.Id);
        }
    }
}
