namespace SquirrelCleaner.Tests
{
    using Catel;
    using Catel.MVVM;
    using Microsoft.Extensions.DependencyInjection;

    internal static class ServiceCollectionHelper
    {
        public static IServiceCollection CreateServiceCollection()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging();
            serviceCollection.AddCatelCore();
            serviceCollection.AddCatelMvvm();

            return serviceCollection;
        }
    }
}
