namespace SquirrelCleaner.Logging
{
    using Catel.Logging;

    public class LogViewerControlLogListener : LogListenerBase
    {
        public LogViewerControlLogListener()
        {
            IgnoreCatelLogging = true;
        }
    }
}
