// <copyright file="IDependencyInjectionProvider.cs" company="Codefarts">
// Copyright (c) Codefarts
// contact@codefarts.com
// http://www.codefarts.com
// </copyright>

namespace Codefarts.WpfAppBootstrapper
{
    using System;

    public interface IDependencyInjectionProvider
    {
        void Register(Type key, Type concrete);

        void Register(Type key, Func<object> callback);

        object Resolve(Type type);

        void ResolveMembers(object value);
    }
}
