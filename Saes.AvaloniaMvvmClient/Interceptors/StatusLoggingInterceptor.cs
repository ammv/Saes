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
            LogSendingGrpcRequest(context);
            return continuation(request, context);
        }

        public override TResponse BlockingUnaryCall<TRequest, TResponse>(TRequest request, ClientInterceptorContext<TRequest, TResponse> context, BlockingUnaryCallContinuation<TRequest, TResponse> continuation)
        {
            LogSendingGrpcRequest(context);
            return base.BlockingUnaryCall(request, context, continuation);
        }

        private static void LogSendingGrpcRequest<TRequest, TResponse>(ClientInterceptorContext<TRequest, TResponse> context)
            where TRequest : class
            where TResponse : class
        {
            MessageBus.Current.SendMessage(StatusData.SendingGrpcRequest($"Начат вызов. Тип/Метод: {context.Method.Type} / {context.Method.Name}. Запрос/Ответ: {typeof(TRequest).Name} / {typeof(TResponse).Name}"));
        }
    }
}
