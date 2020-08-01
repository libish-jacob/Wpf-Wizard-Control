using System.Collections.Generic;

namespace WizardDemo
{
    using System.Windows;
    using System.Windows.Input;

    class MainWindowViewModel
    {
        public MainWindowViewModel()
        {
            WizardItems = new List<object> { new View1(), new View2(), new View3() };
        }

        public IList<object> WizardItems { get; set; }

        public ICommand CancelCommand {
            get
            {
                return new RelayCommand((o)=>MessageBox.Show("Cancel command fired."));
            }
        }

        public ICommand OkCommand
        {
            get
            {
                return new RelayCommand((o) => MessageBox.Show("OK command fired."));
            }
        }
    }
}
