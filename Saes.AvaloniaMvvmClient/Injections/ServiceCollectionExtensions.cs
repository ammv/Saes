﻿using Microsoft.Extensions.DependencyInjection;
using Saes.AvaloniaMvvmClient.Interceptors;
using Saes.AvaloniaMvvmClient.Services.Impementations;
using Saes.AvaloniaMvvmClient.Services.Interfaces;
using Saes.AvaloniaMvvmClient.ViewModels;
using Saes.AvaloniaMvvmClient.ViewModels.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Saes.AvaloniaMvvmClient.Injections
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddCommonServices(this IServiceCollection collection)
        {
            collection.AddSingleton<ISessionKeyService, FileSessionKeyService>();
            
        }

        public static void AddGrpcServices(this IServiceCollection collection)
        {
            collection.AddTransient<SessionKeyInterceptor>();
            collection.AddSingleton<IGrpcChannelFactory>(sp =>
            {
                return new GrpcChannelFactory("https://localhost:7231", sp);
            });

        }

        public static void AddMainViewModels(this IServiceCollection collection)
        {
            collection.AddTransient<MainViewModel>();
        }

        public static void AddAuthViewModels(this IServiceCollection collection)
        {
            collection.AddTransient<AuthenticationMainViewModel>();
            collection.AddTransient<FirstFactorAuthenticationViewModel>();
            collection.AddTransient<SecondFactorAuthenticationViewModel>();
        }
    }
}