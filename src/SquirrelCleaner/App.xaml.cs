namespace SquirrelCleaner
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using Catel.Logging;
    using Orc.Theming;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
#if DEBUG
            LogManager.AddDebugListener(true);
#endif
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            FontImage.RegisterFont("FontAwesome", new FontFamily(new Uri("pack://application:,,,/SquirrelCleaner;component/Resources/Fonts/", UriKind.RelativeOrAbsolute), "./#FontAwesome"));
            FontImage.DefaultFontFamily = "FontAwesome";

            // This shows the StyleHelper, but uses a *copy* of the Orchestra themes. The default margins for controls are not defined in
            // Orc.Theming since it's a low-level library. The final default styles should be in the shell (thus Orchestra makes sense)
            StyleHelper.CreateStyleForwardersForDefaultStyles();
            ThemeManager.Current.SynchronizeTheme();
        }
    }
}
