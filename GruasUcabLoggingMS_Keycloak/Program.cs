using LogginMS.Application.Commands;
using LogginMS.Service.Interfaces;
using LogginMS.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5230); // Puerto HTTP
    options.ListenAnyIP(7133); // Puerto HTTPS
});


// Configuración explícita de URLs
builder.WebHost.UseUrls("http://+:5230", "https://+:7133");





// Configuración de autenticación con JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"];
        var keycloakRealm = builder.Configuration["Keycloak:Realm"];
        var keycloakClientId = builder.Configuration["Keycloak:ClientId"];


        options.Authority = $"{keycloakBaseUrl}/realms/{keycloakRealm}";
        options.Audience = "client-public";
        options.RequireHttpsMetadata = false; // Cambia esto a 'true' en producción

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero // Ajusta esto si necesitas mayor flexibilidad en la expiración del token
        };
    });

builder.Services.AddHttpClient();

// Configura los servicios de controladores
builder.Services.AddControllers();

// Registro de MediatR
builder.Services.AddMediatR(typeof(LoginCommand).Assembly);

// Registro de servicios
builder.Services.AddTransient<IAuthService, AuthService>();
builder.Services.AddSingleton<IKeycloakClientSecret,KeycloakClientSecret>();


// Agrega servicios de Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configuración de Swagger para incluir el esquema de autorización
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "bearer",
        BearerFormat = "jwt",
        In = ParameterLocation.Header,
        Description = "Ingrese 'Bearer' seguido de un espacio y el token JWT."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin() // Permite solicitudes desde cualquier origen
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Habilita Swagger en el pipeline de la aplicación
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.OAuthClientId("client-public"); // Asegúrate de usar el ID de cliente de Keycloak
        c.OAuthAppName("Login Service API");
        c.OAuthUsePkce();
    });
}

// Configuración de la autenticación y autorización
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
