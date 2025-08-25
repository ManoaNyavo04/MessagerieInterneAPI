using System.Text;
using MessagerieInterneAPI;
using MessagerieInterneAPI.Data;
using MessagerieInterneAPI.Modules.Discussion;
using MessagerieInterneAPI.Modules.Hubs;
using MessagerieInterneAPI.Modules.Role;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var jwtIssuer = builder.Configuration.GetSection("Jwt:Issuer").Get<string>();
var jwtKey = builder.Configuration.GetSection("Jwt:Key").Get<string>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
 .AddJwtBearer(options =>
 {
     options.TokenValidationParameters = new TokenValidationParameters
     {
         ValidateIssuer = true,
         ValidateAudience = true,
         ValidateLifetime = true,
         ClockSkew = TimeSpan.Zero,
         ValidateIssuerSigningKey = true,
         ValidIssuer = jwtIssuer,
         ValidAudience = jwtIssuer,
         IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
         RoleClaimType = "Logistique"
     };

     // 👇 très important pour permettre l'authentification WebSocket
     options.Events = new JwtBearerEvents
     {
         OnMessageReceived = context =>
         {
             var accessToken = context.Request.Query["access_token"];

             // Si la requête vient du Hub SignalR
             var path = context.HttpContext.Request.Path;
             if (!string.IsNullOrEmpty(accessToken) &&
                 (path.StartsWithSegments("/chathub")))
             {
                 // Lire le token depuis la query
                 context.Token = accessToken;
             }

             return Task.CompletedTask;
         }
     };
 });



builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Veuillez entrer 'Bearer' suivi de votre token JWT"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }

    });
});



// Ajoute le service CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000")
            // policy.WithOrigins("http://10.5.100.7:3004")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials()
                  .SetIsOriginAllowed(_ => true);
        });
});

builder.Services.AddScoped<UtilisateurService>();
builder.Services.AddScoped<GroupeDiscussionService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddSignalR();

builder.Services.AddSingleton<IUserIdProvider, MyCustomUserIdProvider>();

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI();
}
app.UseRouting();

app.UseCors("AllowLocalhost3000");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
// app.UseCors("AllowReactApp");


app.MapHub<ChatHub>("/chathub");





// app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
