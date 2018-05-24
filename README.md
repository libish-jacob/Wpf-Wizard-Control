# Wpf-Wizard-Control
A wizard control for wpf

You can use this wizard in wpf as shown below.
<wizardView:Wizard CancelCommand="{Binding CancelCommand}"
      OkCommand="{Binding OkCommand}"
      SharedObject="{Binding SharedObject}"       
      Orientation="Top"
      FinalButtonText="Done"
      WizardItems="{Binding WizardItems}" />


you have to implement the interface in all your view model of your wizardItemView.

public interface IWizardItem
    {
        /// <summary>
        /// This method should return the header for wizard item to display
        /// </summary>
        /// <returns> A string value.</returns>
        string GetHeader();

        /// <summary>
        /// This method will be the first to get invoked even before the OnViewActivated call. This method will pass a shard object to views in wizard where they can collect the shared object.
        /// </summary>
        /// <param name="sharedObject">
        /// This is the shared object. This will contain information from other views in the wizard.
        /// </param>

        void CollectSharedObject(object sharedObject);

        /// <summary>
        /// This method will be invoked to check whether this item can be displayed or not.
        /// </summary>
        /// <returns>A boolean value indicating true or false status</returns>

        bool CanDisplay();

        /// <summary>
        /// This method will get invoked when the wizard item becomes the active item.
        /// </summary>
        /// <param name="autoAcknoledgeNext">
        /// The auto Acknoledge Next. Setting this to true will help wizard skip this view and take you to the next page automatically.
        /// </param>

        void OnWizardItemNavigatedTo(ref bool autoAcknoledgeNext);

        /// <summary>
        /// This method will get invoked on the current wizard item when the control is moved to next wizard item.
        /// </summary>
        /// <param name="canNavigateAway">
        /// The can Navigate Away. This value when set to false, will not allow navigation from this view. You can use this to do some validation before you navigate to next view.
        /// </param>

        void OnWizardItemNavigatedFrom(ref bool canNavigateAway);
    }