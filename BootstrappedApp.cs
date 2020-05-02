namespace Codefarts.WpfAppBootstrapper
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using Codefarts.AppCore;
    using Codefarts.AppCore.Interfaces;
    using Codefarts.Settings.XmlDoc.Unity;
    using Codefarts.ViewMessaging;

    /// <summary>
    /// Interaction logic for BootstrappedApp.xaml
    /// </summary>
    public partial class BootstrappedApp : Application
    {
        public event IocRegistrationHandler IoCRegistration;
        private IDependencyInjectionProvider diProvider;

        public BootstrappedApp(IDependencyInjectionProvider diProvider)
        {
            this.diProvider = diProvider;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var currentDomain = AppDomain.CurrentDomain;
            currentDomain.AssemblyResolve += this.ResolveAssemblies;


            var viewService = new WpfViewService() { MvvmEnabled = true };
            viewService.ViewModelTypeResolve += (s, re) => this.diProvider.Resolve(re.Type);

            this.diProvider.Register<IViewService>(() => viewService);
            this.diProvider.Register<IPlatformProvider>(() => new WpfPlatformProvider());
            //ioc.Register<ILocalizationProvider>(()=> new WpfViewService());  


            // TODO: Settings setup should not be here and should be handled my the actual application
            // not this boot strapper code.
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            var fileName = Path.Combine(appData, appName, "settings.xml");
            var xmlDocSettings = new XmlDocSettings(fileName);  // singleton instance
            this.diProvider.Register<ISettingsProvider>(() => xmlDocSettings);

            this.OnIoCRegistration(this.diProvider);

            IView mainView;
            try
            {
                // show main window
                mainView = viewService.CreateView("Application");
                if (mainView == null)
                {
                    MessageBox.Show("Could not locate application view!", "Error");
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
            var args = GenericMessageArguments.Build(
                GenericMessageArguments.Show);
            // GenericMessageArguments.SetModel(applicationViewModel));
            mainView.SendMessage(args);
        }

        private Assembly ResolveAssemblies(object sender, System.ResolveEventArgs args)
        {
            var folderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var filter = new AssemblyName(args.Name);
            var fileMatches = Directory.GetFiles(folderPath, filter.Name + ".dll", SearchOption.AllDirectories);
            var assemblyPath = fileMatches.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(assemblyPath) || !File.Exists(assemblyPath))
            {
                return null;
            }

            return Assembly.LoadFrom(assemblyPath);
        }

        protected virtual void OnIoCRegistration(IDependencyInjectionProvider provider)
        {
            var args = new IocRegistrationHandlerArgs(provider);
            var handler = this.IoCRegistration;
            if (handler != null)
            {
                handler.Invoke(this, args);
            }
        }
    }
}
