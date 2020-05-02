// <copyright file="IocRegistrationHandlerArgs.cs" company="Codefarts">
// Copyright (c) Codefarts
// </copyright>

namespace Codefarts.WpfAppBootstrapper
{
    using System;

    public delegate void IocRegistrationHandler(object sender, IocRegistrationHandlerArgs e);

    public class IocRegistrationHandlerArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IocRegistrationHandlerArgs"/> class.
        /// </summary>
        /// <param name="ioc">A reference to the Ioc container.</param>
        internal IocRegistrationHandlerArgs(IDependencyInjectionProvider ioc)
        {
            this.DependencyInjectionProvider = ioc;
        }

        public IDependencyInjectionProvider DependencyInjectionProvider { get; }
    }
}