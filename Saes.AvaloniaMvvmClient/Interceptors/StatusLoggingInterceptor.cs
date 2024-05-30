using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Saes.AvaloniaMvvmClient.Core;

#nullable disable

namespace Saes.AvaloniaMvvmClient.Interceptors
{
    public class StatusLoggingInterceptor : Interceptor
    {
        public override AsyncUnaryCall<TResponse> AsyncUnaryCall<TRequest, TResponse>(
            TRequest request,
            ClientInterceptorContext<TRequest, TResponse> context,
            AsyncUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest($"Начат вызов. Тип/Метод: {context.Method.Type} / {context.Method.Name}. Запрос/Ответ: {typeof(TRequest).Name} / {typeof(TResponse).Name}"));
            return continuation(request, context);
        }
    }
}
