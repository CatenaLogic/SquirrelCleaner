namespace SquirrelCleaner.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Fody;
    using Catel.MVVM;
    using Models;
    using Services;

    internal partial class ChannelViewModel : FeaturedViewModelBase
    {
        [InjectedService]
        private readonly ICleanerService _cleanerService;

        [InjectedModel]
        [Model(SupportIEditableObject = false)]
        [Expose("Name")]
        [Expose("Product")]
        [Expose("Directory")]
        [Expose("IsIncluded")]
        public Channel Channel { get; private set; }

        public long CleanableSpace { get; private set; }

        public bool IsBusy { get; protected set; }

        protected override async Task InitializeAsync()
        {
            _cleanerService.ChannelCleaning += OnCleanerServiceChannelCleaning;
            _cleanerService.ChannelCleaned += OnCleanerServiceChannelCleaned;

            CleanableSpace = await Task.Run(() => Channel.CalculateCleanableSpaceAsync());
        }

        protected override async Task CloseAsync()
        {
            _cleanerService.ChannelCleaning -= OnCleanerServiceChannelCleaning;
            _cleanerService.ChannelCleaned -= OnCleanerServiceChannelCleaned;
        }

        private void OnCleanerServiceChannelCleaning(object sender, ChannelEventArgs e)
        {
            if (ReferenceEquals(Channel, e.Channel))
            {
                IsBusy = true;
            }
        }

        private void OnCleanerServiceChannelCleaned(object sender, ChannelEventArgs e)
        {
            if (ReferenceEquals(Channel, e.Channel))
            {
                IsBusy = false;
            }
        }
    }
}
