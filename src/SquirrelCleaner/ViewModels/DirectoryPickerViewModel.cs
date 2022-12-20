namespace SquirrelCleaner.ViewModels
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Catel;
    using Catel.MVVM;
    using Catel.Services;
    using Orc.FileSystem;

    public class DirectoryPickerViewModel : ViewModelBase
    {
        #region Constructors
        public DirectoryPickerViewModel(ISelectDirectoryService selectDirectoryService, IProcessService processService,
            IDirectoryService directoryService)
        {
            ArgumentNullException.ThrowIfNull(selectDirectoryService);
            ArgumentNullException.ThrowIfNull(processService);
            ArgumentNullException.ThrowIfNull(directoryService);

            _selectDirectoryService = selectDirectoryService;
            _processService = processService;
            _directoryService = directoryService;

            OpenDirectory = new Command(OnOpenDirectoryExecute, OnOpenDirectoryCanExecute);
            SelectDirectory = new TaskCommand(OnSelectDirectoryExecuteAsync);
        }
        #endregion

        #region Fields
        private readonly IProcessService _processService;
        private readonly IDirectoryService _directoryService;
        private readonly ISelectDirectoryService _selectDirectoryService;
        #endregion

        #region Properties
        public double LabelWidth { get; set; }

        public string LabelText { get; set; }

        public string SelectedDirectory { get; set; }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the OpenDirectory command.
        /// </summary>
        public Command OpenDirectory { get; private set; }

        /// <summary>
        /// Method to check whether the OpenDirectory command can be executed.
        /// </summary>
        /// <returns><c>true</c> if the command can be executed; otherwise <c>false</c></returns>
        private bool OnOpenDirectoryCanExecute()
        {
            if (string.IsNullOrWhiteSpace(SelectedDirectory))
            {
                return false;
            }

            // Don't allow users to write text that they can "invoke" via our software
            if (!_directoryService.Exists(SelectedDirectory))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Method to invoke when the OpenDirectory command is executed.
        /// </summary>
        private void OnOpenDirectoryExecute()
        {
            var fullPath = Path.GetFullPath(SelectedDirectory);
            _processService.StartProcess(fullPath);
        }

        /// <summary>
        /// Gets the SelectDirectory command.
        /// </summary>
        public TaskCommand SelectDirectory { get; private set; }

        /// <summary>
        /// Method to invoke when the SelectOutputDirectory command is executed.
        /// </summary>
        private async Task OnSelectDirectoryExecuteAsync()
        {
            var context = new DetermineDirectoryContext
            {

            };

            if (!string.IsNullOrEmpty(SelectedDirectory))
            {
                context.InitialDirectory = Path.GetFullPath(SelectedDirectory);
            }

            var result = await _selectDirectoryService.DetermineDirectoryAsync(context);
            if (result.Result)
            {
                SelectedDirectory = result.DirectoryName;
            }
        }
        #endregion
    }
}
