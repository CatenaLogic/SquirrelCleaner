namespace SquirrelCleaner.ViewModels
{
    using System;
    using System.Threading.Tasks;
    using Catel.Fody;
    using Catel.MVVM;
    using Models;
    using Services;

    internal class ChannelViewModel : ViewModelBase
    {
        private readonly ICleanerService _cleanerService;

        public ChannelViewModel(Channel channel, ICleanerService cleanerService)
        {
            ArgumentNullException.ThrowIfNull(channel);
            ArgumentNullException.ThrowIfNull(cleanerService);

            Channel = channel;
            _cleanerService = cleanerService;
        }

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
