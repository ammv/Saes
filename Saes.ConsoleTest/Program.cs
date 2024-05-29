// See https://aka.ms/new-console-template for more information
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Models.Schemas;
using Saes.Protos.ModelServices;


var channel = GrpcChannel.ForAddress("https://localhost:7231");

var userService = new UserService.UserServiceClient(channel);

var lookup = new UserLookup
{
    Login = "admin"
};

try
{
    var response = await userService.SearchAsync(lookup);

    Console.WriteLine("Response data:");
    for (int i = 0; i < response.Data.Count; i++)
    {
        Console.WriteLine( $"\t User Id {response.Data[i].UserId}" );
    }
}
catch (Exception ex)
{

	throw;
}

Console.ReadLine();
