using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Project_UI
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        // Конструктор для команды без проверки CanExecute (всегда можно выполнить)
        public RelayCommand(Action<object?> execute)
            : this(execute, null)
        {
        }

        // Конструктор для команды с проверкой CanExecute
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // Событие, которое срабатывает при изменении состояния CanExecute
        // CommandManager.RequerySuggested помогает автоматически обновлять CanExecute
        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        // Определяет, может ли команда быть выполнена
        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        // Выполняет логику команды
        public void Execute(object? parameter)
        {
            _execute(parameter);
        }
    }
}
