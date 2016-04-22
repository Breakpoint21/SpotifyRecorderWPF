using System;
using System.Windows.Input;

namespace SpotifyRecorderWPF.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execAction;
        private readonly Func<bool> _canExecute;

        public RelayCommand (Action execAction, Func<bool> canExecute )
        {
            _execAction = execAction;
            _canExecute = canExecute;
        }

        public bool CanExecute ( object parameter )
        {
            return _canExecute ( );
        }

        public void Execute ( object parameter )
        {
            _execAction ( );
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