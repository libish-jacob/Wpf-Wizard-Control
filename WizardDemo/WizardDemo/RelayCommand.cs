using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardDemo
{
    using System.Windows.Input;

    class RelayCommand : ICommand
    {
        public RelayCommand(Action<object> handler)
        {
            Handler = handler;
        }
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            Handler(parameter);
        }

        public Action<object> Handler { get; set; }
    }
}
