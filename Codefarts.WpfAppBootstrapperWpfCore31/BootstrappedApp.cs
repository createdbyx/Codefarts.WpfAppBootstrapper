// <copyright file="BootstrappedApp.cs" company="Codefarts">
// Copyright (c) Codefarts
// contact@codefarts.com
// http://www.codefarts.com
// </copyright>

using System.Runtime.Loader;

namespace Codefarts.WpfAppBootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using Codefarts.AppCore;
    using Codefarts.AppCore.Interfaces;
    using Codefarts.DependencyInjection;
    using Codefarts.ViewMessaging;

    /// <summary>
    /// Interaction logic for BootstrappedApp.xaml
    /// </summary>
    public partial class BootstrappedApp : Application
    {
        private IDependencyInjectionProvider diProvider;
        private string startView;

        public List<string> AssemblySearchFolders
        {
            get;
        }

        public BootstrappedApp(IDependencyInjectionProvider diProvider, string startView)
        {
            this.diProvider = diProvider;
            this.startView = startView ?? throw new ArgumentNullException(nameof(startView));
            this.AssemblySearchFolders = new List<string>();
        }

        public event RegistrationHandler IoCRegistration;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            //  var currentDomain = AppDomain.CurrentDomain;
            // currentDomain.AssemblyResolve += this.ResolveAssemblies;
            AssemblyLoadContext.Default.Resolving += this.ResolveAssembliesB;

            var viewService = new WpfViewService() { MvvmEnabled = true };
            viewService.ViewModelTypeResolve += (s, re) => this.diProvider.Resolve(re.Type);

            this.diProvider.Register<IViewService>(() => viewService);
            this.diProvider.Register<IPlatformProvider>(() => new WpfPlatformProvider());

            this.OnIoCRegistration(this.diProvider);

            IView mainView;
            try
            {
                // show main window
                mainView = viewService.CreateView(this.startView);
                if (mainView == null)
                {
                    MessageBox.Show($"Could not locate '{this.startView}' view!", "Error");
                    this.Shutdown(1);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                this.Shutdown(1);
                return;
            }

            var window = mainView.ViewReference as Window;
            this.MainWindow = window;
            var args = GenericMessageArguments.Show();
            viewService.SendMessage(GenericMessageConstants.Show, mainView, args);
        }

        private Assembly? ResolveAssembliesB(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            var folderPaths = new List<string>(this.AssemblySearchFolders);
            folderPaths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            var filter = arg2;// new AssemblyName(args.Name);

            switch (Path.GetExtension(filter.Name.ToLowerInvariant()))
            {
                case ".resources":
                    return null;
            }

            foreach (var folderPath in folderPaths)
            {
                if (!Directory.Exists(folderPath))
                {
                    continue;
                }

                var fileMatches = Directory.GetFiles(folderPath, filter.Name + ".dll", SearchOption.AllDirectories);
                var assemblyPath = fileMatches.FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(assemblyPath) && File.Exists(assemblyPath))
                {
                    return arg1.LoadFromAssemblyPath(assemblyPath);// Assembly.LoadFrom(assemblyPath);
                }
            }

            return null;
        }

        //private Assembly ResolveAssemblies(object sender, System.ResolveEventArgs args)
        //{
        //    var folderPaths = new List<string>(this.AssemblySearchFolders);
        //    folderPaths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
        //    var filter = new AssemblyName(args.Name);

        //    switch (Path.GetExtension(filter.Name.ToLowerInvariant()))
        //    {
        //        case ".resources":
        //            return null;
        //    }

        //    foreach (var folderPath in folderPaths)
        //    {
        //        if (!Directory.Exists(folderPath))
        //        {
        //            continue;
        //        }

        //        var fileMatches = Directory.GetFiles(folderPath, filter.Name + ".dll", SearchOption.AllDirectories);
        //        var assemblyPath = fileMatches.FirstOrDefault();
        //        if (!string.IsNullOrWhiteSpace(assemblyPath) && File.Exists(assemblyPath))
        //        {
        //            return Assembly.LoadFrom(assemblyPath);
        //        }
        //    }

        //    return null;
        //}

        protected virtual void OnIoCRegistration(IDependencyInjectionProvider provider)
        {
            var args = new RegistrationHandlerArgs(provider);
            var handler = this.IoCRegistration;
            if (handler != null)
            {
                handler.Invoke(this, args);
            }
        }
    }
}
