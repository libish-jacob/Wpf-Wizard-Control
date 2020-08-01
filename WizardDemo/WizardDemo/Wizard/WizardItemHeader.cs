// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WizardItemHeader.cs" company="Vestas Technology R&D">
//   WizardItemHeader
// </copyright>
// <summary>
//   Defines the WizardItemHeader type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace WizardDemo.Wizard
{
    internal class WizardItemHeader
    {
        public int ItemNumber { get; set; }

        public string ItemHeader { get; set; }

        public bool Visited { get; set; }
    }
}
