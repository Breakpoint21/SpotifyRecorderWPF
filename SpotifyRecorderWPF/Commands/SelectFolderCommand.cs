using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace SpotifyRecorderWPF.Commands
{
    public class SelectFolderCommand : ICommand
    {
        private readonly MainViewModel _viewModel;
        private readonly FolderBrowserDialog _folderBrowserDialog;

        public SelectFolderCommand ( MainViewModel vm )
        {
            _viewModel = vm;
            _folderBrowserDialog = new FolderBrowserDialog ( );
        }

        public bool CanExecute ( object parameter )
        {
            return !_viewModel.RecordingStarted;
        }

        public void Execute ( object parameter )
        {
            var dialogResult = _folderBrowserDialog.ShowDialog();
            if ( dialogResult == DialogResult.OK )
            {
                _viewModel.OutputFolder = _folderBrowserDialog.SelectedPath;
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }

            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }
    }
}