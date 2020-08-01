namespace WizardDemo.Wizard
{
    using System.Windows;

    public interface IWizardItem
    {
        /// <summary>
        /// This method should return the header for wizard item to display
        /// </summary>
        /// <returns> A string value.</returns>
        string GetHeader();

        /// <summary>
        /// This method will be invoked to check whether this item can be displayed or not.
        /// </summary>
        /// <returns>A boolean value indicating true or false status</returns>
        bool CanDisplay();

        /// <summary>
        /// This method will get invoked when the wizard item becomes the active item.
        /// </summary>
        void OnWizardItemNavigatedTo();

        /// <summary>
        /// This method will get invoked on the current wizard item when the control is moved to next wizard item.
        /// </summary>
        void OnWizardItemNavigatedFrom();
    }
}
