using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardDemo
{
    using WizardDemo.Wizard;

    class View3ViewModel : IWizardItem
    {
        public string GetHeader()
        {
            return "Wizard Item3";
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
