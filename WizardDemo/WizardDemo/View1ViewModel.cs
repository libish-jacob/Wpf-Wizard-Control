using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardDemo
{
    using WizardDemo.Wizard;

    class View1ViewModel : IWizardItem
    {
        public string GetHeader()
        {
            return "Wizard Item1";
        }

        public bool CanDisplay()
        {
            return true;
        }

        public void OnWizardItemNavigatedTo()
        {
        }

        public void OnWizardItemNavigatedFrom()
        {
        }
    }
}
