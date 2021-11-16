// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SummaryViewModel.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2015 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Collections;
    using Catel.Data;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Services;
    using Models;
    using Services;

    internal class SummaryViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly FastObservableCollection<Channel> _channels;
        private readonly ChangeNotificationWrapper _changeNotificationWrapper;

        private bool _hasPendingUpdates;

        public SummaryViewModel(FastObservableCollection<Channel> channels)
        {
            Argument.IsNotNull(() => channels);

            _channels = channels;

            _changeNotificationWrapper = new ChangeNotificationWrapper(channels);
        }

        public int ChannelsToClean { get; private set; }

        public long TotalSize { get; private set; }

        public bool IsBusy { get; private set; }

        protected override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            _changeNotificationWrapper.CollectionChanged += OnChannelsChanged;
            _changeNotificationWrapper.CollectionItemPropertyChanged += OnChannelPropertyChanged;

            await UpdateAsync();
        }

        protected override async Task CloseAsync()
        {
            _changeNotificationWrapper.CollectionChanged -= OnChannelsChanged;
            _changeNotificationWrapper.CollectionItemPropertyChanged -= OnChannelPropertyChanged;

            await base.CloseAsync();
        }

        private async void OnChannelsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await UpdateAsync();
        }

        private async void OnChannelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            await UpdateAsync();
        }

        private async Task UpdateAsync()
        {
            if (IsBusy)
            {
                _hasPendingUpdates = true;
                return;
            }

            using (CreateIsBusyScope())
            {
                //Log.Debug("Updating summary");

                var channels = _channels.Where(x => x.IsIncluded).ToList();

                ChannelsToClean = channels.Count;

                TotalSize = 0L;

                foreach (var channel in channels)
                {
                    if (channel.CleanableSize.HasValue)
                    {
                        TotalSize += channel.CleanableSize.Value;
                    }
                }

                //Log.Debug("Updated summary");
            }

            if (_hasPendingUpdates)
            {
                _hasPendingUpdates = false;
                await UpdateAsync();
            }
        }

        private IDisposable CreateIsBusyScope()
        {
            return new DisposableToken<SummaryViewModel>(this, x => x.Instance.IsBusy = true, x => x.Instance.IsBusy = false);
        }
    }
}
