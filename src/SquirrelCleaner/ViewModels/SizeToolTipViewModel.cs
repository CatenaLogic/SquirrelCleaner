namespace SquirrelCleaner.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Text;
    using System.Threading.Tasks;
    using Catel;
    using Catel.MVVM;
    using Catel.Text;
    using Cleaners;
    using Humanizer;
    using Models;

    internal class SizeToolTipViewModel : ViewModelBase
    {
        private readonly Channel _channel;

        public SizeToolTipViewModel(Channel channel)
        {
            ArgumentNullException.ThrowIfNull(channel);

            _channel = channel;

            Items = new ObservableCollection<string>();
        }

        public bool IsBusy { get; private set; }

        public ObservableCollection<string> Items { get; private set; }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            await UpdateAsync();
        }

        private async Task UpdateAsync()
        {
            using (CreateIsBusyScope())
            {
                Items.Clear();

                foreach (var cleaner in _channel.Cleaners)
                {
                    var size = await cleaner.CalculateCleanableSpaceAsync(_channel);
                    var line = string.Format("{0} => {1}", cleaner.Name, (size == 0L) ? "0 bytes" : size.Bytes().Humanize("#.#"));

                    Items.Add(line);
                }
            }
        }

        private IDisposable CreateIsBusyScope()
        {
            return new DisposableToken<SizeToolTipViewModel>(this, x => x.Instance.IsBusy = true, x => x.Instance.IsBusy = false);
        }
    }
}
