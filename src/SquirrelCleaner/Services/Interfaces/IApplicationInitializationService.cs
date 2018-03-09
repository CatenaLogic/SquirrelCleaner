﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IApplicationInitializationService.cs" company="CatenaLogic">
//   Copyright (c) 2014 - 2015 CatenaLogic. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace SquirrelCleaner.Services
{
    using System.Threading.Tasks;

    public interface IApplicationInitializationService
    {
        Task Initialize();
    }
}