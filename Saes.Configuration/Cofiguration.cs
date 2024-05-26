using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System;

namespace Saes.Configuration
{
    public static class Cofiguration
    {
        public static string ConnectionString => @"Data Source=KOMPUTER\SQLEXPRESS;Initial Catalog=SAES;Integrated Security=True;Connect Timeout=300;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False";

        public static int SessionExpiredHours => 3;
    }
}
