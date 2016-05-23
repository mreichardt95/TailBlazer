﻿using System;
using System.IO;
using System.Text;
using log4net.Config;
using StructureMap;
using TailBlazer.Domain.FileHandling;
using TailBlazer.Domain.FileHandling.Search;
using TailBlazer.Domain.Formatting;
using TailBlazer.Domain.Infrastructure;
using TailBlazer.Domain.Settings;
using TailBlazer.Infrastucture;
using TailBlazer.Infrastucture.AppState;
using TailBlazer.Properties;
using TailBlazer.Views.Tail;

namespace TailBlazer
{
    internal class AppRegistry : Registry
    {
        public AppRegistry()
        {
            //set up logging
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config");
            if (!File.Exists(path))
            {
                // should use the default config which is a resource
                using (var stream = new MemoryStream(Encoding.ASCII.GetBytes(Resources.log4net)))
                {
                    XmlConfigurator.Configure(stream);
                }
            }
            else
            {
                XmlConfigurator.ConfigureAndWatch(new FileInfo(path));
            }
            For<ILogger>().Use<Log4NetLogger>().Ctor<Type>("type").Is(x => x.ParentType).AlwaysUnique();

            For<ISelectionMonitor>().Use<SelectionMonitor>();
            For<ISearchInfoCollection>().Use<SearchInfoCollection>();
            For<ISearchMetadataCollection>().Use<SearchMetadataCollection>().Transient();
            For<ITextFormatter>().Use<TextFormatter>();
            For<ISettingsStore>().Use<FileSettingsStore>().Singleton();
            For<IFileWatcher>().Use<FileWatcher>();


            For<UhandledExceptionHandler>().Singleton();
            For<ObjectProvider>().Singleton();
            Forward<ObjectProvider, IObjectProvider>();
            Forward<ObjectProvider, IObjectRegister>();


            For<ViewFactoryService>().Singleton();
            Forward<ViewFactoryService, IViewFactoryRegister>();
            Forward<ViewFactoryService, IViewFactoryProvider>();

            //For<ViewFactoryService>().Singleton();
            //Forward<ViewFactoryService, IViewFactoryRegister>();
            //Forward<ViewFactoryService, IViewFactoryProvider>();

            For<ApplicationStateBroker>().Singleton();
            Forward<ApplicationStateBroker, IApplicationStateNotifier>();
            Forward<ApplicationStateBroker, IApplicationStatePublisher>();


            For<TailViewModelFactory>().Singleton();


            Scan(scanner =>
            {
                scanner.ExcludeType<ILogger>();

                //to do, need a auto-exclude these from AppConventions
                scanner.ExcludeType<SelectionMonitor>();
                scanner.ExcludeType<SearchInfoCollection>();
                scanner.ExcludeType<SearchMetadataCollection>();
                scanner.ExcludeType<ITextFormatter>();
                scanner.ExcludeType<ViewFactoryService>();

                scanner.ExcludeType<FileWatcher>();
                scanner.LookForRegistries();
                scanner.Convention<AppConventions>();

                scanner.AssemblyContainingType<ILogFactory>();
                scanner.AssemblyContainingType<AppRegistry>();
            });
        }

    }
}

