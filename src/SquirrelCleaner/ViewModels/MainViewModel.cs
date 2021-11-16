// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2015 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Collections;
    using Catel.Configuration;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Reflection;
    using Catel.Services;
    using Catel.Threading;
    using MethodTimer;
    using Models;
    using Services;

    internal class MainViewModel : ViewModelBase
    {
        #region Constants
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        #endregion

        #region Fields
        private readonly ICleanerService _cleanerService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IConfigurationService _configurationService;
        private readonly IChannelService _channelService;
        #endregion

        #region Constructors
        public MainViewModel(ICleanerService cleanerService, IDispatcherService dispatcherService,
            IConfigurationService configurationService, IChannelService channelService)
        {
            Argument.IsNotNull(() => cleanerService);
            Argument.IsNotNull(() => dispatcherService);
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => channelService);

            _cleanerService = cleanerService;
            _dispatcherService = dispatcherService;
            _configurationService = configurationService;
            _channelService = channelService;

            Channels = new FastObservableCollection<Channel>();
            FilteredChannels = new FastObservableCollection<Channel>();

            Analyze = new Command(OnAnalyzeExecute, OnAnalyzeCanExecute);
            FakeCleanUp = new Command(OnFakeCleanUpExecute, OnCleanUpCanExecute);
            CleanUp = new Command(OnCleanUpExecute, OnCleanUpCanExecute);

            var entryAssembly = AssemblyHelper.GetEntryAssembly();
            Title = string.Format("{0} - v{1}", entryAssembly.Title(), entryAssembly.InformationalVersion());
        }
        #endregion

        #region Properties
        public string ChannelsRoot { get; set; }

        public string ChannelFilter { get; set; }

        public FastObservableCollection<Channel> Channels { get; private set; }

        public FastObservableCollection<Channel> FilteredChannels { get; private set; }

        public bool IsBusy { get; private set; }

        public int Progress { get; private set; }
        #endregion

        #region Commands
        public Command Analyze { get; private set; }

        private bool OnAnalyzeCanExecute()
        {
            if (string.IsNullOrWhiteSpace(ChannelsRoot))
            {
                return false;
            }

            if (IsBusy)
            {
                return false;
            }

            return true;
        }

        private async void OnAnalyzeExecute()
        {
            await FindChannelsAsync();
        }

        public Command FakeCleanUp { get; private set; }

        private async void OnFakeCleanUpExecute()
        {
            await CleanAsync(true);
        }

        public Command CleanUp { get; private set; }

        private bool OnCleanUpCanExecute()
        {
            if (Channels is null || Channels.Count == 0)
            {
                return false;
            }

            if (IsBusy)
            {
                return false;
            }

            return true;
        }

        private async void OnCleanUpExecute()
        {
            await CleanAsync(false);
        }
        #endregion

        #region Methods
        protected override async Task InitializeAsync()
        {
            ChannelsRoot = _configurationService.GetRoamingValue<string>(Settings.Application.General.LastChannelsRoot);

            await FindChannelsAsync();
        }

        private void OnChannelFilterChanged()
        {
            FilterChannels();
        }

        [Time]
        private async Task FindChannelsAsync()
        {
            var channelsRoot = ChannelsRoot;
            if (string.IsNullOrWhiteSpace(channelsRoot))
            {
                return;
            }

            Log.Info("Start calculating cleanable releases");

            Progress = 0;

            using (CreateIsBusyScope())
            {
                var channels = new List<Channel>();

                await TaskHelper.Run(async () => { channels.AddRange(await _channelService.FindChannelsAsync(channelsRoot)); });

                if (channels.Count > 0)
                {
                    using (Channels.SuspendChangeNotifications())
                    {
#pragma warning disable 618
                        Channels.ReplaceRange(channels);
#pragma warning restore 618
                    }
                }
                else
                {
                    Channels = null;
                }

                _configurationService.SetRoamingValue(Settings.Application.General.LastChannelsRoot, channelsRoot);

                FilterChannels();

                var totalChannels = channels.Count;
                var completedChannels = 0;

                foreach (var channel in channels.OrderBy(x => x.Name))
                {
                    await channel.CalculateCleanableSpaceAsync();

                    completedChannels++;

                    var percentage = ((double)completedChannels / totalChannels) * 100;
                    Progress = (int)percentage;
                }
            }

            Log.Info("Finished calculating cleanable releases");

            Progress = 100;
        }

        private async Task CleanAsync(bool isFakeClean)
        {
            Progress = 0;

            using (CreateIsBusyScope())
            {
                var channels = Channels.ToList();

                var totalChannels = channels.Count;
                var completedChannels = 0;

                await _cleanerService.CleanAsync(channels, isFakeClean,
                    () => _dispatcherService.BeginInvoke(() =>
                    {
                        completedChannels++;

                        var percentage = ((double)completedChannels / totalChannels) * 100;
                        Progress = (int)percentage;
                    }));

                ViewModelCommandManager.InvalidateCommands(true);
            }

            Progress = 100;

            if (!isFakeClean)
            {
                Analyze.Execute(null);
            }
        }

        private void FilterChannels()
        {
            var channels = Channels;
            if (channels is null)
            {
                using (FilteredChannels.SuspendChangeNotifications())
                {
                    FilteredChannels.Clear();
                }

                return;
            }

            using (FilteredChannels.SuspendChangeNotifications())
            {
                var filteredChannels = channels.Where(x => true);

                var filter = ChannelFilter;
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filteredChannels = filteredChannels.Where(x => x.MatchesFilter(filter));
                }

#pragma warning disable 618
                FilteredChannels.ReplaceRange(filteredChannels);
#pragma warning restore 618
            }
        }

        private IDisposable CreateIsBusyScope()
        {
            return new DisposableToken<MainViewModel>(this, x =>
            {
                x.Instance._dispatcherService.BeginInvoke(() => x.Instance.IsBusy = true);
            }, x =>
            {
                x.Instance._dispatcherService.BeginInvoke(() => x.Instance.IsBusy = false);
            });
        }
        #endregion
    }
}
