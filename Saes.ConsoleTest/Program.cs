// See https://aka.ms/new-console-template for more information
using Microsoft.EntityFrameworkCore;
using Saes.Models;
using Saes.Models.Schemas;

Console.WriteLine("Hello, World!");

using(var ctx = new SaesContext())
{
    var user  = await ctx.Users.FirstOrDefaultAsync(x => x.Login == "admin");
    while(true)
    {
        Console.Write("Enter password: ");
        var password = Console.ReadLine();

        if(password == "q")
        {
            break;
        }

        byte[] hash = ctx.udfHashSalt(password, user.PasswordSalt);
        if(hash.SequenceEqual(user.PasswordHash))
        {
            Console.WriteLine("Доступ есть");
            Console.ReadLine();

        }
        else
        {
            Console.WriteLine("Неверный пароль");
        }
    }
}
