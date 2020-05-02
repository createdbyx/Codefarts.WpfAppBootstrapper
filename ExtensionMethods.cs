// <copyright file="ExtensionMethods.cs" company="Codefarts">
// Copyright (c) Codefarts
// contact@codefarts.com
// http://www.codefarts.com
// </copyright>

namespace Codefarts.WpfAppBootstrapper
{
    using System;

    public static class ExtensionMethods
    {
        public static void Register<T>(this IDependencyInjectionProvider provider, Func<T> callback)
        {
            provider.Register(typeof(T), () => callback());
        }

        public static T Resolve<T>(this IDependencyInjectionProvider provider)
        {
            return (T)provider.Resolve(typeof(T));
        }
    }
} 