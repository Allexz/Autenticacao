using JWT.Servico;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StackExchange.Redis;
using System.Text;

WebApplicationBuilder? builder = WebApplication.CreateBuilder(args);

// Add services to the container.

string? secretKey = builder.Configuration["Jwt:SecretKey"] ?? "SuaChaveSecretaSuperSegura";
byte[]? key = Encoding.UTF8.GetBytes(secretKey);

// Adicionar serviços de autenticação
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "SeuIssuer",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "SeuAudience",
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine($"Token validated for: {context.Principal?.Identity?.Name}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddScoped<IJWTServices, JWTServices>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Informe seu TOKEN neste campo",
        Type = SecuritySchemeType.Http,
        In = ParameterLocation.Header,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        BearerFormat = "JWT"
    };
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                Array.Empty<string>()
            }
        }
    );
});

builder.Services.AddSingleton<IConnectionMultiplexer>((option) =>
{
    var options = new ConfigurationOptions
    {
        EndPoints = { "localhost:6379" },
        ConnectRetry = 2,
        Password = "abcd1234"
    };

    try
    {
        return ConnectionMultiplexer.Connect(options);
    }
    catch (Exception ex) when (ex is RedisConnectionException || ex is Exception)
    {
        // Verifica se o erro está relacionado à autenticação
        if (ex.Message.Contains("AUTH") || ex.Message.Contains("authentication") ||
               ex.Message.Contains("ERR wrong password") || ex.Message.Contains("ERR invalid password"))
        {
            throw new InvalidOperationException("Senha inválida para o servidor Redis.", ex);
        }
        else
        {
            throw; // Relança a exceção original para outros tipos de erros
        }
    }
});

builder.Services.AddHttpClient();

WebApplication? app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
