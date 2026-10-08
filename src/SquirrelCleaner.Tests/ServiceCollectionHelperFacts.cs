namespace SquirrelCleaner.Tests
{
    using Microsoft.Extensions.DependencyInjection;
    using NUnit.Framework;

    [TestFixture]
    public class ServiceCollectionHelperFacts
    {
        [Test]
        public void CreateServiceCollection_Creates_Isolated_Service_Providers()
        {
            var firstRegistration = new object();
            var firstServiceCollection = ServiceCollectionHelper.CreateServiceCollection();
            firstServiceCollection.AddSingleton(firstRegistration);

            var secondServiceCollection = ServiceCollectionHelper.CreateServiceCollection();

            using var firstServiceProvider = firstServiceCollection.BuildServiceProvider();
            using var secondServiceProvider = secondServiceCollection.BuildServiceProvider();

            Assert.That(firstServiceProvider.GetService<object>(), Is.SameAs(firstRegistration));
            Assert.That(secondServiceProvider.GetService<object>(), Is.Null);
        }
    }
}
