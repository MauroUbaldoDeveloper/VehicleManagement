using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MinimalAPI.Domain.DTOs;
using MinimalAPI.Domain.DTOs.Enums;
using MinimalAPI.Domain.Entities;
using MinimalAPI.Domain.Interfaces;
using MinimalAPI.Domain.ModelViews;
using MinimalAPI.Domain.Services;
using MinimalAPI.Infrastructure;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MinimalAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            key = Configuration?.GetSection("Jwt")?.ToString() ?? "";
        }

        public IConfiguration? Configuration { get; set; } = default!;
        private string key;

        public void ConfigureServices(IServiceCollection services)
        {

            if (string.IsNullOrEmpty(key))
                key = "123456";

            services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

                    ValidateIssuer = false,
                    ValidateAudience = false,
                };
            });

            services.AddAuthorization();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IVehicleService, VehicleService>();

            services.AddDbContext<DatabaseContext>(
                options => options.UseSqlServer(
                    Configuration?.GetConnectionString("sql")
                    )
                );

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(
                    options =>
                    {
                        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Name = "Authorization",
                            Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
                            Scheme = "bearer",
                            BearerFormat = "JWT",
                            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                            Description = "Insert the JWT Token here."
                        });

                        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                        {

                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = "Bearer"
                                    }
                                },
                                new string[] { }
                            }
                        
                        });

                    }

                );

        }
    
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints => {


                #region Home
                endpoints.MapGet("/", () => Results.Json(new Home())).AllowAnonymous().WithTags("Home");
                #endregion
                #region Users

                string GenerateJWTToken(User user)
                {
                    if (string.IsNullOrEmpty(key))
                        return string.Empty;

                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim>
                                                {
                                                    new Claim("Email", user.Email),
                                                    new Claim("Profile", user.Profile),
                                                    new Claim(ClaimTypes.Role, user.Profile)
                                                };

                    var token = new JwtSecurityToken(
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: credentials
                    );

                    return new JwtSecurityTokenHandler().WriteToken(token);

                }

                endpoints.MapPost("/users/login", ([FromBody] LoginDTO loginDTO, IUserService userService) =>
                {
                    var user = userService.Login(loginDTO);

                    if (user != null)
                    {
                        string token = GenerateJWTToken(user);
                        return Results.Ok(new UserLogged
                        {
                            Email = user.Email,
                            Profile = user.Profile,
                            Token = token
                        });
                    }
                    else
                        return Results.Unauthorized();
                }).AllowAnonymous().WithTags("Users").
                Produces<UserLogged>(StatusCodes.Status200OK).
                Produces(StatusCodes.Status401Unauthorized);

                endpoints.MapPost("/users", ([FromBody] UserDTO userDTO, IUserService userService) =>
                {

                    var validation = new ValidationErrors
                    {
                        Messages = new List<string>()
                    };

                    if (string.IsNullOrEmpty(userDTO.Email))
                        validation.Messages.Add("Email cannot be empty.");

                    if (string.IsNullOrEmpty(userDTO.Password))
                        validation.Messages.Add("Password cannot be empty.");

                    if (userDTO.Profile == null)
                        validation.Messages.Add("Profile cannot be null.");

                    if (validation.Messages.Count > 0)
                        return Results.BadRequest(validation);

                    var user = new MinimalAPI.Domain.Entities.User
                    {
                        Email = userDTO.Email,
                        Password = userDTO.Password,
                        Profile = userDTO.Profile.ToString() ?? Profile.EDITOR.ToString(),
                    };

                    userService.Include(user);

                    return Results.Created($"/users/{user.Id}", (new UserViewModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Profile = user.Profile
                    }));

                }).RequireAuthorization().
                RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).
                Produces(StatusCodes.Status400BadRequest).
                Produces<UserViewModel>(StatusCodes.Status201Created).
                WithTags("Users");

                endpoints.MapGet("/users", ([FromQuery] int? page, IUserService userService) =>
                {
                    var usersViewModels = new List<UserViewModel>();
                    var users = userService.All(page);

                    foreach (var user in users)
                    {
                        usersViewModels.Add(new UserViewModel
                        {
                            Id = user.Id,
                            Email = user.Email,
                            Profile = user.Profile
                        });
                    }

                    return Results.Ok(usersViewModels);
                }).RequireAuthorization().
                RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).
                Produces<List<UserViewModel>>(StatusCodes.Status200OK).
                WithTags("Users");

                endpoints.MapGet("/users/{id}", ([FromRoute] int id, IUserService userService) =>
                {
                    var user = userService.FindById(id);

                    if (user == null)
                        return Results.NotFound();

                    return Results.Ok(new UserViewModel
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Profile = user.Profile
                    });
                }).RequireAuthorization().
                RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).
                Produces(StatusCodes.Status404NotFound).
                Produces<UserViewModel>(StatusCodes.Status200OK).
                WithTags("Users");

                #endregion

                #region Vehicles

                ValidationErrors ValidateDTO(VehicleDTO vehicleDTO)
                {
                    var validation = new ValidationErrors { Messages = new List<string>() };

                    if (string.IsNullOrEmpty(vehicleDTO.Name))
                        validation.Messages.Add("The name cannot be empty.");

                    if (string.IsNullOrEmpty(vehicleDTO.Mark))
                        validation.Messages.Add("The mark cannot be empty.");

                    if (vehicleDTO.year < 1950)
                        validation.Messages.Add("The vehicle is too old. Please enter a more recent year.");

                    return validation;
                }

                endpoints.MapPost("/vehicles", ([FromBody] VehicleDTO VehicleDTO, IVehicleService vehicleService) =>
                {
                    var validation = ValidateDTO(VehicleDTO);

                    if (validation.Messages.Count > 0)
                        return Results.BadRequest(validation);

                    var vehicle = new MinimalAPI.Domain.Entities.Vehicle
                    {
                        Name = VehicleDTO.Name,
                        Mark = VehicleDTO.Mark,
                        year = VehicleDTO.year,
                    };

                    vehicleService.Include(vehicle);

                    return Results.Created($"/vehicle/{vehicle.Id}", vehicle);
                }).RequireAuthorization().
                RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" }).
                Produces(StatusCodes.Status400BadRequest).
                Produces<Vehicle>(StatusCodes.Status201Created).
                WithTags("Vehicles");

                endpoints.MapGet("/vehicles", ([FromQuery] int? page, IVehicleService vehicleService) =>
                {
                    var vehicles = vehicleService.All(page);

                    return Results.Ok(vehicles);
                }).RequireAuthorization().
                Produces<List<Vehicle>>(StatusCodes.Status200OK).
                WithTags("Vehicles");

                endpoints.MapGet("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
                {
                    var vehicle = vehicleService.FindById(id);

                    if (vehicle == null)
                        return Results.NotFound();

                    return Results.Ok(vehicle);
                }).RequireAuthorization().
                RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" }).
                Produces(StatusCodes.Status404NotFound).
                Produces<Vehicle>(StatusCodes.Status200OK).
                WithTags("Vehicles");

                endpoints.MapPut("/vehicles/{id}", ([FromRoute] int id, VehicleDTO vehicleDTO, IVehicleService vehicleService) =>
                {

                    var vehicle = vehicleService.FindById(id);

                    if (vehicle == null)
                        return Results.NotFound();

                    var validation = ValidateDTO(vehicleDTO);

                    if (validation.Messages.Count > 0)
                        return Results.BadRequest(validation);

                    vehicle.Name = vehicleDTO.Name;
                    vehicle.Mark = vehicleDTO.Mark;
                    vehicle.year = vehicleDTO.year;

                    vehicleService.Refresh(vehicle);

                    return Results.Ok(vehicle);
                }).RequireAuthorization().
                RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).
                Produces(StatusCodes.Status404NotFound).
                Produces(StatusCodes.Status400BadRequest).
                Produces<Vehicle>(StatusCodes.Status200OK).
                WithTags("Vehicles");

                endpoints.MapDelete("/vehicles/{id}", ([FromRoute] int id, IVehicleService vehicleService) =>
                {
                    var vehicle = vehicleService.FindById(id);

                    if (vehicle == null)
                        return Results.NotFound();

                    vehicleService.Delete(vehicle);

                    return Results.NoContent();
                }).RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" }).
                RequireAuthorization().
                Produces(StatusCodes.Status404NotFound).
                Produces(StatusCodes.Status204NoContent).
                WithTags("Vehicles");

                #endregion

            });
        }
    }
}
