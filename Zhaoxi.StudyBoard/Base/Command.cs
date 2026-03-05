using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Zhaoxi.StudyBoard.Base
{
    public class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            _action?.Invoke(parameter);
        }

        Action<object?> _action;
        public Command(Action<object?> action)
        {
            _action = action;
        }
    }
}
