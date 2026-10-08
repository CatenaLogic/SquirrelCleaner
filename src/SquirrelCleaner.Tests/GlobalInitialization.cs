namespace SquirrelCleaner.Tests
{
    using System.Diagnostics;
    using System.Globalization;
    using System.Threading;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    [SetUpFixture]
    public class GlobalInitialization
    {
        private static ServiceProvider _serviceProvider;
        private static ILoggerFactory _loggerFactory;

        [OneTimeSetUp]
        public void SetUp()
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                if (Debugger.IsAttached)
                {
                    builder.SetMinimumLevel(LogLevel.Debug);
                    builder.AddDebug();
                }

                builder.AddConsole();
            });

            LogManager.FallbackLoggerFactory = _loggerFactory;

            var culture = new CultureInfo("en-US");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            TypeCache.InitializeTypes(allowMultithreadedInitialization: false);

            var serviceCollection = ServiceCollectionHelper.CreateServiceCollection();
            _serviceProvider = serviceCollection.BuildServiceProvider();
            IoCContainer.ServiceProvider = _serviceProvider;
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            IoCContainer.ServiceProvider = null;
            LogManager.FallbackLoggerFactory = null;
#pragma warning disable IDISP007 // Dispose test-owned providers
            _serviceProvider.Dispose();
            _loggerFactory.Dispose();
#pragma warning restore IDISP007
        }
    }
}
