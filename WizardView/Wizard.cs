namespace Wizard.WizardView
{
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Threading;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    using Brushes = System.Windows.Media.Brushes;

    public class Wizard : Grid
    {
        private static readonly DependencyProperty SharedObjectProperty =
            DependencyProperty.RegisterAttached(
                "SharedObject",
                typeof(object),
                typeof(Wizard),
                new PropertyMetadata(null));

        private static readonly DependencyProperty WizardItemsProperty =
            DependencyProperty.RegisterAttached(
                "WizardItems",
                typeof(IList<object>),
                typeof(Wizard),
                new PropertyMetadata(new List<object>()));
        
        private static readonly DependencyProperty OkCommandProperty = DependencyProperty.Register(
            "OkCommand",
            typeof(ICommand),
            typeof(Wizard),
            new PropertyMetadata(default(ICommand)));

        private static readonly DependencyProperty CancelCommandProperty =
            DependencyProperty.RegisterAttached(
                "CancelCommand",
                typeof(ICommand),
                typeof(Wizard),
                new PropertyMetadata(default(ICommand)));

        private static readonly DependencyProperty FinalButtonTextProperty =
            DependencyProperty.RegisterAttached(
                "FinalButtonText",
                typeof(string),
                typeof(Wizard),
                new PropertyMetadata(default(string)));
        
        private readonly object lockObject = new object();

        private Button buttonBack;
        private Button buttonNext;
        private Button buttonCancel;

        public Wizard()
        {
            this.PreviewKeyDown += this.WizardKeyDown;
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.5, GridUnitType.Star) });
            this.RowDefinitions.Add(new RowDefinition { Height = new GridLength(35) });

            this.Loaded += this.WizardLoaded;
            this.IsVisibleChanged += this.WizardIsVisibleChanged;

            this.SetContent();
            this.SetFooter();
        }
        
        public static Dock TabOrientation { get; set; }

        public object SharedObject
        {
            get
            {
                return this.GetValue(SharedObjectProperty);
            }

            set
            {
                this.SetValue(SharedObjectProperty, value);
            }
        }
        
        public IList<object> WizardItems
        {
            get
            {
                return (IList<object>)this.GetValue(WizardItemsProperty);
            }

            set
            {
                this.SetValue(WizardItemsProperty, value);
            }
        }

        public ICommand OkCommand
        {
            get
            {
                return (ICommand)this.GetValue(OkCommandProperty);
            }

            set
            {
                this.SetValue(OkCommandProperty, value);
            }
        }

        public string FinalButtonText
        {
            get
            {
                return (string)this.GetValue(FinalButtonTextProperty);
            }

            set
            {
                this.SetValue(FinalButtonTextProperty, value);
            }
        }

        public ICommand CancelCommand
        {
            get
            {
                return (ICommand)this.GetValue(CancelCommandProperty);
            }

            set
            {
                this.SetValue(CancelCommandProperty, value);
            }
        }

        public Dock Orientation
        {
            get
            {
                return TabOrientation;
            }

            set
            {
                TabOrientation = value;
            }
        }

        public TabControl WizardTab { get; set; }

        private void WizardIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var visibility = (bool)e.NewValue;
            if (visibility)
            {
                this.Focusable = true;
                this.Focus();
            }
        }

        private void WizardKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.Key == Key.Enter) || (e.Key == Key.Right))
            {
                this.HandleNext();
                e.Handled = true;
                this.Focus();
            }
            else if (e.Key == Key.Left)
            {
                this.HandleBackCommand();
                e.Handled = true;
                this.Focus();
            }
        }

        private void WizardLoaded(object sender, RoutedEventArgs e)
        {
            if (Monitor.TryEnter(this.lockObject))
            {
                this.PrepareWizard();
            }
        }
        
        private void PrepareWizard()
        {
            int count = 0;
            int maxTabWidth = 0;
            IList<FrameworkElement> list = new List<FrameworkElement>();

            if (this.WizardItems == null)
            {
                return;
            }

            foreach (var wizardItem in this.WizardItems)
            {
                if (wizardItem is FrameworkElement)
                {
                    list.Add(wizardItem as FrameworkElement);
                }
            }

            this.WizardItems = list.ToList<object>();
            this.WizardTab.Items.Clear();

            foreach (object o in this.WizardItems)
            {
                var frameworkElement = o as FrameworkElement;

                if (frameworkElement != null)
                {
                    var wizardItem = frameworkElement.DataContext as IWizardItem;

                    var header = new WizardItemHeader
                                     {
                                         ItemNumber = ++count,
                                         ItemHeader =
                                             (wizardItem != null) ? wizardItem.GetHeader() : string.Empty
                                     };

                    var ti = new TabItem { Header = header };

                    if (count == 1)
                    {
                        header.Visited = true;
                    }

                    if (!string.IsNullOrEmpty(header.ItemHeader))
                    {
                        int width = this.GetWidth(header.ItemHeader);
                        if (maxTabWidth < width)
                        {
                            maxTabWidth = width;
                        }
                    }

                    ti.Style = this.CreateStyle();
                    ti.HeaderTemplate = this.Create();
                    ti.Content = frameworkElement;

                    ti.IsEnabled = header.Visited;

                    this.WizardTab.Items.Add(ti);
                }
            }

            if (this.WizardTab.Items.Count > 0)
            {
                var wizardItem = this.WizardTab.Items[0] as TabItem;
                if (wizardItem != null)
                {
                    var baseFrameworkElement = wizardItem.Content as FrameworkElement;
                    if (baseFrameworkElement != null)
                    {
                        var baseWizard = baseFrameworkElement.DataContext as IWizardItem;

                        if (baseWizard != null)
                        {
                            baseWizard.CollectSharedObject(this.SharedObject);
                        }
                        
                        if (baseWizard != null)
                        {
                            bool autoAcknoledgeNextButton = false;
                            baseWizard.OnWizardItemNavigatedTo(ref autoAcknoledgeNextButton);
                            if (autoAcknoledgeNextButton)
                            {
                                this.HandleNextCommand();
                            }
                        }
                    }
                }
            }

            this.WizardTab.TabStripPlacement = TabOrientation;
            if (TabOrientation == Dock.Left)
            {
                // move the button to right so that it is aligned to tab body when left oriented.
                this.buttonBack.Margin = new Thickness(maxTabWidth + 10, 0, 10, 0);
            }

            foreach (var item in this.WizardTab.Items)
            {
                var tabItem = item as TabItem;
                if (tabItem != null)
                {
                    tabItem.Width = (maxTabWidth > 100) ? maxTabWidth : 100;
                }
            }
        }
        
        private int GetWidth(string header)
        {
            var textBitmap = new Bitmap(1, 1);
            Graphics drawGraphics = Graphics.FromImage(textBitmap);
            var drawFont = new Font("Arial", 10);

            int width = (int)drawGraphics.MeasureString(header, drawFont).Width;
            return width;
        }

        private Style CreateStyle()
        {
            var controlTemplate = new ControlTemplate(typeof(TabItem));
            var grid = new FrameworkElementFactory(typeof(Grid));

            var contentPresenter = new FrameworkElementFactory(typeof(ContentPresenter));

            contentPresenter.SetValue(NameProperty, "ContentSite");
            contentPresenter.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenter.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            contentPresenter.SetValue(ContentPresenter.ContentSourceProperty, "Header");

            var selectedSetter2 = new Setter { Property = ZIndexProperty, Value = -500 };

            var selectedTrigger = new Trigger { Property = TabItem.IsSelectedProperty, Value = true };
            selectedTrigger.Setters.Add(selectedSetter2);

            controlTemplate.Triggers.Add(selectedTrigger);

            grid.AppendChild(contentPresenter);
            controlTemplate.VisualTree = grid;

            var setter = new Setter { Property = Control.TemplateProperty, Value = controlTemplate };

            var style = new Style();
            style.Setters.Add(setter);

            return style;
        }

        private DataTemplate Create()
        {
            // create the data template
            var cardLayout = new DataTemplate { DataType = typeof(TabItem) };

            // set up the stack panel
            var stackPanel = new FrameworkElementFactory(typeof(StackPanel)) { Name = "myComboFactory" };

            // set up the border
            var mainBorder = new FrameworkElementFactory(typeof(Border)) { Name = "Border" };
            mainBorder.SetValue(HeightProperty, 40.0);
            mainBorder.SetValue(WidthProperty, 40.0);
            mainBorder.SetValue(MarginProperty, new Thickness(10, 5, 10, 12)); // 10, 5, 17, 12
            mainBorder.SetValue(Border.BorderBrushProperty, Brushes.Black);
            mainBorder.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            mainBorder.SetValue(Border.CornerRadiusProperty, new CornerRadius(20));

            // set up the number Textblock
            var textBlockInsideBorder = new FrameworkElementFactory(typeof(TextBlock)) { Name = "TextBlock" };
            textBlockInsideBorder.SetValue(TextBlock.FontWeightProperty, FontWeights.ExtraBold);
            textBlockInsideBorder.SetValue(TextBlock.ForegroundProperty, Brushes.White);
            textBlockInsideBorder.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            textBlockInsideBorder.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlockInsideBorder.SetBinding(TextBlock.TextProperty, new Binding("ItemNumber"));

            // add text block inside border
            mainBorder.AppendChild(textBlockInsideBorder);

            stackPanel.AppendChild(mainBorder);

            // set up the number Textblock
            var headerTextBlock = new FrameworkElementFactory(typeof(TextBlock)) { Name = "HeaderTextBlock" };
            headerTextBlock.SetValue(TextBlock.ForegroundProperty, Brushes.DarkGray);
            headerTextBlock.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
            headerTextBlock.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
            headerTextBlock.SetBinding(TextBlock.TextProperty, new Binding("ItemHeader"));

            stackPanel.AppendChild(headerTextBlock);

            // set the visual tree of the data template
            cardLayout.VisualTree = stackPanel;

            // visited events and setters.
            var visitedSetter = new Setter
            {
                TargetName = "Border",
                Property = Border.BackgroundProperty,
                Value = Brushes.Gray
            };

            var visitedTrigger = new Trigger { Property = IsEnabledProperty, Value = false };
            visitedTrigger.Setters.Add(visitedSetter);

            var setter = new Setter
            {
                TargetName = "Border",
                Property = Border.BackgroundProperty,
                Value = Brushes.DarkGray
            };

            var trigger = new Trigger { Property = IsEnabledProperty, Value = false };
            trigger.Setters.Add(setter);

            var isEnablesSetter2 = new Setter
            {
                TargetName = "Border",
                Property = Border.BackgroundProperty,
                Value = Brushes.CornflowerBlue
            };

            var isEnablesSetter3 = new Setter
            {
                TargetName = "HeaderTextBlock",
                Property = TextBlock.FontWeightProperty,
                Value = FontWeights.ExtraBold
            };

            var trigger2 = new Trigger { Property = IsEnabledProperty, Value = true };
            trigger2.Setters.Add(isEnablesSetter2);
            trigger2.Setters.Add(isEnablesSetter3);

            cardLayout.Triggers.Add(trigger);
            cardLayout.Triggers.Add(trigger2);

            return cardLayout;
        }

        private void HandleBackCommand()
        {
            int selectedIndex = this.WizardTab.SelectedIndex;
            if (selectedIndex > 0)
            {
                int newIndex = selectedIndex - 1;
                var tabItem = this.WizardTab.Items[newIndex] as TabItem;
                this.WizardTab.SelectedIndex = newIndex;
                
                if (tabItem != null && !tabItem.IsEnabled)
                {
                    this.HandleBackCommand();
                }
            }
        }

        private void HandleNextCommand()
        {
            int selectedIndex = (this.WizardTab.SelectedIndex < 0) ? 0 : this.WizardTab.SelectedIndex;
            if ((this.WizardTab.Items.Count - 1) > selectedIndex)
            {
                int newIndex = selectedIndex + 1;
                object currentWizardItemObject = this.WizardItems[selectedIndex];

                if (currentWizardItemObject != null)
                {
                    var element = currentWizardItemObject as FrameworkElement;
                    if (element != null)
                    {
                        var item = element.DataContext as IWizardItem;
                        if (item != null)
                        {
                            if (item.CanDisplay())
                            {
                                bool canNavigate = true;
                                item.OnWizardItemNavigatedFrom(ref canNavigate);
                                if (!canNavigate)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }

                object nextWizardItemObject = this.WizardItems[newIndex];

                var tabItem = this.WizardTab.Items[newIndex] as TabItem;
                if (nextWizardItemObject != null)
                {
                    var frameworkElement = nextWizardItemObject as FrameworkElement;
                    {
                        if (frameworkElement != null)
                        {
                            var wizardItem = frameworkElement.DataContext as IWizardItem;
                            if (wizardItem != null)
                            {
                                wizardItem.CollectSharedObject(this.SharedObject);
                            }

                            bool canDisplay = wizardItem != null && wizardItem.CanDisplay();
                            
                            this.WizardTab.SelectedIndex = newIndex;

                            if (!canDisplay)
                            {
                                if (tabItem != null)
                                {
                                    tabItem.IsEnabled = false;
                                }

                                this.HandleNextCommand();
                            }
                            else
                            {
                                if (tabItem != null)
                                {
                                    tabItem.IsEnabled = true;
                                }

                                bool autoAcknoledgeNextButton = false;
                                wizardItem.OnWizardItemNavigatedTo(ref autoAcknoledgeNextButton);
                                if (autoAcknoledgeNextButton)
                                {
                                    this.HandleNextCommand();
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                if (this.OkCommand != null)
                {
                    this.OkCommand.Execute(null); 
                }
            }
        }

        private void SetContent()
        {
            this.WizardTab = new TabControl();
            this.WizardTab.IsSynchronizedWithCurrentItem = true;
            SetRow(this.WizardTab, 0);
            this.Children.Add(this.WizardTab);
            this.WizardTab.SelectionChanged += this.TabSelectionChanged;
        }

        private void TabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = e.Source as TabControl;

            if (tabControl != null)
            {
                int selectedIndex = tabControl.SelectedIndex;
                for (int i = selectedIndex + 1; i < tabControl.Items.Count; i++)
                {
                    var tabItem = tabControl.Items[i] as TabItem;
                    if (tabItem != null)
                    {
                        tabItem.IsEnabled = false;
                    }
                }

                this.buttonBack.IsEnabled = selectedIndex != 0;
                if (tabControl.Items.Count == selectedIndex + 1)
                {
                    if (string.IsNullOrEmpty(this.FinalButtonText))
                    {
                        this.buttonNext.Content = "Next";
                    }
                    else
                    {
                        this.buttonNext.Content = this.FinalButtonText;
                    }
                }
                else
                {
                    this.buttonNext.Content = "Next";
                }
            }  

            e.Handled = true;
        }

        private void SetFooter()
        {
            var docPanel = new DockPanel();
            this.buttonBack = new Button
            {
                Height = 25,
                Width = 100,
                Content = "Back",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10, 0, 10, 0),
                Visibility = Visibility.Visible,
                ToolTip = "Shortcut: LEFT ARROW"
            };
            this.buttonBack.Click += this.OnBackButtonClick;

            this.buttonNext = new Button
            {
                Height = 25,
                Width = 100,
                Content = "Next",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(10, 0, 10, 0),
                Visibility = Visibility.Visible,
                ToolTip = "Shortcut: RIGHT ARROW or ENTER"
            };

            this.buttonNext.Click += this.OnNextButtonClick;

            this.buttonCancel = new Button
            {
                Height = 25,
                Width = 100,
                Content = "Cancel",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10, 0, 10, 0),
                Visibility = Visibility.Visible
            };
            this.buttonCancel.Click += this.OnCancelButtonClick;

            docPanel.Children.Add(this.buttonBack);
            docPanel.Children.Add(this.buttonNext);
            docPanel.Children.Add(this.buttonCancel);

            SetRow(docPanel, 1);
            this.Children.Add(docPanel);
        }

        private void OnCancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (this.CancelCommand != null)
            {
                this.CancelCommand.Execute(null);
            }
        }

        private void OnBackButtonClick(object sender, RoutedEventArgs e)
        {
            this.HandleBackCommand();
        }

        private void OnNextButtonClick(object sender, RoutedEventArgs e)
        {
            this.HandleNext();
        }

        private void HandleNext()
        {
            if (this.WizardTab.Items.Count == this.WizardTab.SelectedIndex + 1)
            {
                if (this.WizardTab.Items.Count > 0)
                {
                    var wizardItem = this.WizardTab.Items[this.WizardTab.SelectedIndex] as TabItem;
                    if (wizardItem != null)
                    {
                        var baseWizard = wizardItem.Content as FrameworkElement;
                        if (baseWizard != null)
                        {
                            var item = baseWizard.DataContext as IWizardItem;
                            if (item != null)
                            {
                                bool canNavigate = true;
                                item.OnWizardItemNavigatedFrom(ref canNavigate);
                                if (!canNavigate)
                                {
                                    return;
                                }
                            }
                        }
                    }
                }

                if (this.OkCommand != null)
                {
                    this.OkCommand.Execute(null);
                }
            }
            else
            {
                this.HandleNextCommand();
            }
        }
    }
}
