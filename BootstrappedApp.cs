using System.Reflection;

namespace Codefarts.WpfAppBootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition;
    using System.ComponentModel.Composition.Hosting;
    using System.ComponentModel.Composition.Primitives;
    using System.ComponentModel.Composition.ReflectionModel;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using Codefarts.AppCore;
    using Codefarts.AppCore.Interfaces;
    using Codefarts.IoC;
    using Codefarts.Settings.XmlDoc.Unity;
    using Codefarts.ViewMessaging;

    /// <summary>
    /// Interaction logic for BootstrappedApp.xaml
    /// </summary>
    public partial class BootstrappedApp : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            /*
            //   AppDomain.CurrentDomain.TypeResolve += (s, e) => { };
            AppDomain.CurrentDomain.AssemblyResolve += (s, re) =>
            {
                var nugetPackagePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget", "packages");
                var nameParts = re.Name.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries); //	"Microsoft.EntityFrameworkCore, Version=3.1.3.0, Culture=neutral, PublicKeyToken=adb9793829ddae60"	 
                var asmName = nameParts[0];
                var version = nameParts[1].Substring(nameParts[1].IndexOf("=") + 1);
                var versionParts = version.Split('.');
                var asmPath = Path.Combine(nugetPackagePath, asmName, string.Join(".", versionParts.Take(3)), "lib");
                var desiredAsmName = asmName + ".dll";

                // check if folder exists before moving forward
                if (!Directory.Exists(asmPath))
                {
                    throw new DirectoryNotFoundException($"No Folder Available: {asmPath}");
                }

                var filesThatExist = Directory.GetFiles(asmPath, "*.dll", SearchOption.AllDirectories);
                var item = filesThatExist.Where(x =>
                {
                    var asmFileName = Path.GetFileName(x);
                    return asmFileName.Equals(desiredAsmName, StringComparison.OrdinalIgnoreCase);
                }).FirstOrDefault();

                if (item == null)
                {
                    throw new FileLoadException();
                }

                return Assembly.LoadFile(asmPath);
                // return null;
            };     */

            var ioc = new Container();
            this.Properties["IoC"] = ioc;

            var viewService = new WpfViewService();
            ioc.Register<IViewService>(() => viewService);
            ioc.Register<IPlatformProvider>(() => new WpfPlatformProvider());
            //ioc.Register<ILocalizationProvider>(()=> new WpfViewService());

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appName = Path.GetFileNameWithoutExtension(Environment.GetCommandLineArgs()[0]);
            var fileName = Path.Combine(appData, appName, "settings.xml");
            ioc.Register<ISettingsProvider>(() => new XmlDocSettings(fileName));

            //var parts = LoadPlugins();
            //foreach (var dictionary in parts.ResourceDictionaries)
            //{
            //    this.Resources.MergedDictionaries.Add(dictionary);
            //}

            // show main window
            var mainView = viewService.CreateView("Application");
            var window = mainView.ViewReference as Window;
            this.MainWindow = window;
            var args = GenericMessageArguments.Build(
                GenericMessageArguments.Show);
            // GenericMessageArguments.SetModel(applicationViewModel));
            mainView.SendMessage(args);
        }

        //internal class MEFComponents
        //{
        //    [ImportMany(typeof(ResourceDictionary))]
        //    public IEnumerable<ResourceDictionary> ResourceDictionaries
        //    {
        //        get; set;
        //    }
        //}

        //private MEFComponents LoadPlugins()
        //{
        //    var parts = new MEFComponents();
        //    // var pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        //    var pluginsPath = AppDomain.CurrentDomain.BaseDirectory;
        //    // Directory.CreateDirectory(pluginsPath);
        //    var searchPaths = Directory.GetDirectories(pluginsPath, "*.*", SearchOption.TopDirectoryOnly);
        //    var composer = this.Compose(searchPaths, parts);

        //    // var types= GetExportedTypes<>()

        //    //parts.FilterPlugins = this.GetPluginInformation<ISearchFilter>(composer.Catalog, appModel);
        //    //parts.SourcePlugins = this.GetPluginInformation<ISourcePlugin>(composer.Catalog, appModel);
        //    //parts.GeneralPluginInformation = this.GetGeneralPluginInformation(composer.Catalog, appModel);
        //    //parts.ResultPlugins = this.GetPluginInformation<IResultsPlugin>(composer.Catalog, appModel);
        //    return parts;
        //}

        //public IEnumerable<Type> GetExportedTypes<T>(ComposablePartCatalog catalog)
        //{
        //    return catalog.Parts.Select(part => this.ComposablePartExportType<T>(part)).Where(t => t != null).ToArray();
        //}

        //private Type ComposablePartExportType<T>(ComposablePartDefinition part)
        //{
        //    if (part.ExportDefinitions.Any(
        //        def => def.Metadata.ContainsKey("ExportTypeIdentity") &&
        //               def.Metadata["ExportTypeIdentity"].Equals(typeof(T).FullName)))
        //    {
        //        return ReflectionModelServices.GetPartType(part).Value;
        //    }

        //    return null;
        //}

        ///// <summary>
        ///// Composes MEF parts.
        ///// </summary>
        ///// <param name="parts">The composable object parts.</param>
        //private void Compose(params object[] parts)
        //{
        //    Compose(null, parts);
        //}

        ///// <summary>
        ///// Composes MEF parts.
        ///// </summary>
        ///// <param name="searchFolders">Provides a series of search folders to search for *.dll files.</param>
        ///// <param name="parts">The composable object parts.</param>
        //private CompositionContainer Compose(IEnumerable<string> searchFolders, params object[] parts)
        //{
        //    // setup composition container
        //    var catalog = new AggregateCatalog();

        //    // check if folders were specified
        //    if (searchFolders != null)
        //    {
        //        // add search folders
        //        foreach (var folder in searchFolders.Where(Directory.Exists))
        //        {
        //            catalog.Catalogs.Add(new DirectoryCatalog(folder, "*.dll"));
        //        }
        //    }

        //    catalog.Catalogs.Add(new ApplicationCatalog());

        //    // compose and create plug ins
        //    var composer = new CompositionContainer(catalog);
        //    composer.ComposeParts(parts);
        //    return composer;
        //}
    }
}
