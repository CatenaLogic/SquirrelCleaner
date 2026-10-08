namespace SquirrelCleaner
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Media;
    using Catel;
    using Catel.Configuration;
    using Catel.IoC;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Orc;
    using Orc.Theming;
    using Orchestra;
    using SquirrelCleaner.Cleaners;
    using SquirrelCleaner.Services;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#pragma warning disable IDISP006 // Implement IDisposable
        private readonly IHost _host;
#pragma warning restore IDISP006 // Implement IDisposable

        public App()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureServices((_, services) =>
                {
                    services.AddLogging(loggingBuilder =>
                    {
#if DEBUG
                        loggingBuilder.SetMinimumLevel(LogLevel.Debug);
#endif
                        loggingBuilder.AddDebug();
                        loggingBuilder.AddInMemory();
                    });

                    services.AddCatelCore();
                    services.AddCatelMvvm();
                    services.AddOrcControls();
                    services.AddOrcFileSystem();
                    services.AddOrcLogViewer();
                    services.AddOrcNotifications();
                    services.AddOrcTheming();
                    services.AddOrchestraCore();

                    services.AddSingleton<ICleanerService, CleanerService>();
                    services.AddSingleton<IChannelService, ChannelService>();
                    services.AddTransient<ICleaner, PackageCleaner>();
                });

            _host = hostBuilder.Build();
            IoCContainer.ServiceProvider = _host.Services;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceProvider = IoCContainer.ServiceProvider;

            FontImage.RegisterFont("FontAwesome", new FontFamily(new Uri("pack://application:,,,/SquirrelCleaner;component/Resources/Fonts/", UriKind.RelativeOrAbsolute), "./#FontAwesome"));
            FontImage.DefaultFontFamily = "FontAwesome";

            var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
            await configurationService.LoadAsync();
            serviceProvider.CreateTypesThatMustBeConstructedAtStartup();

            // This shows the StyleHelper, but uses a *copy* of the Orchestra themes. The default margins for controls are not defined in
            // Orc.Theming since it's a low-level library. The final default styles should be in the shell (thus Orchestra makes sense)
            StyleHelper.CreateStyleForwardersForDefaultStyles();
            ThemeManager.Current.SynchronizeTheme();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            await _host.StopAsync();
            _host.Dispose();

            base.OnExit(e);
        }
    }
}
