namespace SquirrelCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public abstract class InterfaceFinderServiceBase<TInterface>
    {
        private readonly List<TInterface> _implementations;

        protected InterfaceFinderServiceBase(IEnumerable<TInterface> implementations)
        {
            ArgumentNullException.ThrowIfNull(implementations);

            _implementations = implementations.ToList();
        }

        protected IEnumerable<TInterface> GetAvailableItems()
        {
            return _implementations.ToList();
        }
    }
}
