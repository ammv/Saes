// See https://aka.ms/new-console-template for more information
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Models.Schemas;
using Saes.Protos.ModelServices;


DateTime dateTime = DateTime.Now;
Console.WriteLine(dateTime);
Console.WriteLine(dateTime.ToUniversalTime());
Console.WriteLine(dateTime.ToUniversalTime().ToUniversalTime());
Console.ReadLine();

