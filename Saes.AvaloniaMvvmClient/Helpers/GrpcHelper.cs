using Grpc.Core;
using Saes.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Helpers
{
    public static class GrpcHelper
    {
        public static async Task<bool> CheckConnection(CallInvoker callInvoker, int attempts = 10, int delay = 500)
        {
            var client = new Greeter.GreeterClient(callInvoker);
            while (attempts >= 0)
            {
                try
                {
                    await client.SayHelloAsync(new HelloRequest { Name = "Artemka" });
                    
                    return true;
                }
                catch (Exception)
                {
                    --attempts;
                    await Task.Delay(delay);
                }
            }
            return false;
        }
    }
}
