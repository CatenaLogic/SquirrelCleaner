﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ApplicationInitializationService.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2015 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Services
{
    using System;
    using System.Threading.Tasks;
    using Catel;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.Threading;

    public class ApplicationInitializationService : IApplicationInitializationService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IServiceLocator _serviceLocator;
        private readonly ITypeFactory _typeFactory;

        public ApplicationInitializationService(ITypeFactory typeFactory, IServiceLocator serviceLocator)
        {
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => serviceLocator);

            _typeFactory = typeFactory;
            _serviceLocator = serviceLocator;
        }

        public async Task Initialize()
        {
            await TaskHelper.RunAndWaitAsync(new Func<Task>[]
            {
                //InitializeAnalytics
            });
        }

        //[Time]
        //private async Task InitializeAnalytics()
        //{
        //    Log.Info("Initializing analytics");

        //    var googleAnalyticsService = _serviceLocator.ResolveType<IGoogleAnalyticsService>();
        //    googleAnalyticsService.AccountId = Analytics.AccountId;
        //}
    }
}