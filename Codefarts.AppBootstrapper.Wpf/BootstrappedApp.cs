// <copyright file="BootstrappedApp.cs" company="Codefarts">
// Copyright (c) Codefarts
// contact@codefarts.com
// http://www.codefarts.com
// </copyright>

namespace Codefarts.WpfAppBootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.Loader;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="BootstrappedApp"/> class.
        /// </summary>
        /// <param name="diProvider">Reference to a <see cref="IDependencyInjectionProvider"/> implementation.</param>
        /// <param name="startView">The name of the start view.</param>
        public BootstrappedApp(IDependencyInjectionProvider diProvider, string startView)
        {
            this.diProvider = diProvider ?? throw new ArgumentNullException(nameof(diProvider));
            this.startView = startView ?? throw new ArgumentNullException(nameof(startView));
            this.AssemblySearchFolders = new List<string>();
        }

        public event RegistrationHandler IoCRegistration;

        public List<string> AssemblySearchFolders { get; }

        protected override void OnStartup(StartupEventArgs e)
        {
            //PresentationTraceSources.Refresh();
            //PresentationTraceSources.DataBindingSource.Listeners.Add(new ConsoleTraceListener());
            ////PresentationTraceSources.DataBindingSource.Listeners.Add(new DebugTraceListener());
            //PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Warning | SourceLevels.Error;
            base.OnStartup(e);

            //  var currentDomain = AppDomain.CurrentDomain;
            // currentDomain.AssemblyResolve += this.ResolveAssemblies;
            AssemblyLoadContext.Default.Resolving += this.ResolveAssembliesB;

            var viewService = this.diProvider.Resolve<WpfViewService>();
            viewService.MvvmEnabled = true;
            viewService.ViewModelTypeResolve += (s, re) => this.diProvider.Resolve(re.Type);
            viewService.BeforeViewDeleted += this.ViewService_BeforeViewDeleted;

            this.diProvider.Register<IViewService>(() => viewService);
            this.diProvider.Register<IPlatformProvider, WpfPlatformProvider>();

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

        private void ViewService_BeforeViewDeleted(object sender, ViewEventArgs e)
        {
            // TODO: special case code 
            var win = e.View.ViewReference as Window;

            //var viewService = this.diProvider.Resolve<WpfViewService>();
            //var args = GenericMessageArguments.Close();
            //viewService.SendMessage(GenericMessageConstants.Close, e.View, args);

            win?.Close();
        }

        private Assembly? ResolveAssembliesB(AssemblyLoadContext arg1, AssemblyName arg2)
        {
            var folderPaths = new List<string>(this.AssemblySearchFolders);
            folderPaths.Add(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            var filter = arg2; // new AssemblyName(args.Name);

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
                    return arg1.LoadFromAssemblyPath(assemblyPath); // Assembly.LoadFrom(assemblyPath);
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
            handler?.Invoke(this, args);
        }
    }
}