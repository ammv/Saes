using Microsoft.EntityFrameworkCore;
using Saes.GrpcServer.Interceptors;
using Saes.GrpcServer.ProtoServices;
using Saes.GrpcServer.ProtoServices.AuthService;
using Saes.GrpcServer.Services.Implementations;
using Saes.GrpcServer.Services.Interfaces;
using Saes.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SaesContext>(option =>
{
    option.UseSqlServer(Saes.Configuration.Cofiguration.ConnectionString);
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ILogAuthenticationService, LogAuthenticationService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.Interceptors.Add<AuthorizationInterceptor>();
});


var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<AuthenticationService>();
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
