using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Saes.GrpcServer.Interceptors;
using Saes.GrpcServer.Mapping;
using Saes.GrpcServer.ProtoServices;
using Saes.GrpcServer.ProtoServices.AuthService;
using Saes.GrpcServer.ProtoServices.ModelServices;
using Saes.GrpcServer.Services;
using Saes.Models;
using Grpc.AspNetCore;

var context = new SaesContext();
context.Logs.Any(x => x.LogId != 0);

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<SaesContext>(option =>
{
    option.UseSqlServer(Saes.Configuration.Cofiguration.ConnectionString);
});

new ServicesRegister().Register(builder.Services);
// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
//#if !DEBUG
builder.Services.AddGrpc(options =>
{
    AuthorizationInterceptor.Context = context;
    options.Interceptors.Add<AuthorizationInterceptor>();
});

builder.Services.AddGrpcReflection();

var config = new TypeAdapterConfig();
new MapsterConfig(config);
new RegisterMapper().Register(config);
builder.Services.AddSingleton(config);
builder.Services.AddScoped<IMapper, ServiceMapper>(); //��������� ��� ������

var app = builder.Build();
app.MapGrpcReflectionService();

var loggerFactory = app.Services.GetService<ILoggerFactory>();
loggerFactory.AddFile(builder.Configuration["Logging:LogFilePath"].ToString());

// Configure the HTTP request pipeline.
// GRPC

new ModelServicesGrpcRegister().Register(app);
app.MapGrpcService<AuthenticationService>();
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();

context.Dispose();
