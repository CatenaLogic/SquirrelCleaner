﻿namespace SquirrelCleaner.Views
{
    using System.ComponentModel;
    using System.Windows;
    using Catel.Windows.Threading;

    public partial class ChannelView
    {
        public ChannelView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when a property on the current <see cref="P:Catel.Windows.Controls.UserControl.ViewModel"/> has changed.
        /// </summary>
        /// <param name="e">The <see cref="T:System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected override void OnViewModelPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnViewModelPropertyChanged(e);

            if (layoutRoot.Visibility != Visibility.Visible)
            {
                Dispatcher.BeginInvoke(() => layoutRoot.SetCurrentValue(FrameworkElement.VisibilityProperty, Visibility.Visible));
            }
        }
    }
}
