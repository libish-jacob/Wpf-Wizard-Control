using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WizardDemo
{
    using WizardDemo.Wizard;

    class View2ViewModel : IWizardItem
    {
        public string GetHeader()
        {
            return "Wizard Item2";
        }

        public bool CanDisplay()
        {
            return true; ;
        }

        public void OnWizardItemNavigatedTo()
        {
        }

        public void OnWizardItemNavigatedFrom()
        {
        }
    }
}
