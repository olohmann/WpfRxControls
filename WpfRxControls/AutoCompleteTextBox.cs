// ==================================================================================
// This Sample Code is provided for the purpose of illustration only and is not 
// intended to be used in a production environment.  THIS SAMPLE CODE AND ANY RELATED 
// INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY 
// AND/OR FITNESS FOR A PARTICULAR PURPOSE.  We grant You a nonexclusive, 
// royalty-free right to use and modify the Sample Code and to reproduce and 
// distribute the object code form of the Sample Code, provided that You agree: 
// (i) to not use Our name, logo, or trademarks to market Your software product in 
// which the Sample Code is embedded; (ii) to include a valid copyright notice on 
// Your software product in which the Sample Code is embedded; and (iii) to 
// indemnify, hold harmless, and defend Us and Our suppliers from and against any 
// claims or lawsuits, including attorneys’ fees, that arise or result from the use 
// or distribution of the Sample Code.
// ==================================================================================

namespace WpfRxControls
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    using WpfRxControls.Utils;

    [TemplatePart(Name = PartTextBox, Type = typeof(TextBox))]
    [TemplatePart(Name = PartPopup, Type = typeof(Popup))]
    [TemplatePart(Name = PartListBox, Type = typeof(ListBox))]
    public class AutoCompleteTextBox : Control
    {
        public const string PartTextBox = "PART_TextBox";
        public const string PartPopup = "PART_Popup";
        public const string PartListBox = "PART_ListBox";

        private readonly List<IDisposable> subscriptions = new List<IDisposable>();

        private IObservable<string> textChangedObservable;

        private bool isTemplateApplied;
        private ListBox partListBox;
        private Popup partPopup;
        private TextBox partTextBox;

        private IConnectableObservable<IEnumerable<object>> publishedResultsObservable;
        private IDisposable publishedResultsObservableConnection;

        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));                
        }

        #region Dependency Properties
        #region PopupHeight
        /// <summary>
        /// The <see cref="PopupHeight" /> dependency property's name.
        /// </summary>
        public const string PopupHeightPropertyName = "PopupHeight";

        /// <summary>
        /// Gets or sets the value of the <see cref="PopupHeight" />
        /// property. This is a dependency property.
        /// </summary>
        public double PopupHeight
        {
            get
            {
                return (double)GetValue(PopupHeightProperty);
            }
            set
            {
                SetValue(PopupHeightProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="PopupHeight" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty PopupHeightProperty = DependencyProperty.Register(
            PopupHeightPropertyName,
            typeof(double),
            typeof(AutoCompleteTextBox),
            new UIPropertyMetadata(250.0));
        #endregion

        #region AutoCompleteQueryResultProvider
        /// <summary>
        /// The <see cref="AutoCompleteQueryResultProvider" /> dependency property's name.
        /// </summary>
        public const string AutoCompleteQueryResultProviderName = "AutoCompleteQueryResultProvider";

        /// <summary>
        /// Gets or sets the value of the <see cref="AutoCompleteQueryResultProvider" />
        /// property. This is a dependency property.
        /// </summary>
        public AutoCompleteQueryResultProvider AutoCompleteQueryResultProvider
        {
            get
            {
                return (AutoCompleteQueryResultProvider)GetValue(QueryResultFunctionProperty);
            }
            set
            {
                SetValue(QueryResultFunctionProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="AutoCompleteQueryResultProvider" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty QueryResultFunctionProperty = DependencyProperty.Register(
            AutoCompleteQueryResultProviderName,
            typeof(AutoCompleteQueryResultProvider),
            typeof(AutoCompleteTextBox),
            new UIPropertyMetadata(AutoCompleteQueryResultProvider.Empty, OnProviderFunctionChanged));
     
        private static void OnProviderFunctionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (AutoCompleteTextBox)d;
            self.OnConfigurationChanged();
        }

        #endregion       

        #region IsBusy (readonly)
        private static readonly DependencyPropertyKey IsBusyPropertyKey = DependencyProperty.RegisterReadOnly(
            "IsBusy",
            typeof(bool),
            typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsBusyProperty = IsBusyPropertyKey.DependencyProperty;

        public bool IsBusy
        {
            get { return (bool)GetValue(IsBusyProperty); }
            protected set { SetValue(IsBusyPropertyKey, value); }
        }
        #endregion

        #region Results (readonly)
        private static readonly DependencyPropertyKey ResultsPropertyKey = DependencyProperty.RegisterReadOnly(
            "Results",
            typeof(ObservableCollection<object>),
            typeof(AutoCompleteTextBox),
            new FrameworkPropertyMetadata(new ObservableCollection<object>()));

        public static readonly DependencyProperty ResultsProperty = ResultsPropertyKey.DependencyProperty;

        public ObservableCollection<object> Results
        {
            get { return (ObservableCollection<object>)GetValue(ResultsProperty); }
            protected set { SetValue(ResultsPropertyKey, value); }
        }
        #endregion

        #region Delay
        /// <summary>
        /// The <see cref="Delay" /> dependency property's name.
        /// </summary>
        public const string DelayPropertyName = "Delay";

        /// <summary>
        /// Gets or sets the value of the <see cref="Delay" />
        /// property. This is a dependency property.
        /// </summary>
        public int Delay
        {
            get
            {
                return (int)GetValue(DelayProperty);
            }
            set
            {
                SetValue(DelayProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="Delay" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty DelayProperty = DependencyProperty.Register(
            DelayPropertyName,
            typeof(int),
            typeof(AutoCompleteTextBox), new UIPropertyMetadata(200, OnDelayChanged, OnDelayCoerceValue));

        private static void OnDelayChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (AutoCompleteTextBox)d;
            self.OnConfigurationChanged();
        }

        private static object OnDelayCoerceValue(DependencyObject d, object baseValue)
        {
            var value = (int)baseValue;
            if (value <= 0)
            {
                return 0;
            }

            return value;
        }

        #endregion

        #region MinimumCharacters
        /// <summary>
        /// The <see cref="MinimumCharacters" /> dependency property's name.
        /// </summary>
        public const string MinimumCharactersPropertyName = "MinimumCharacters";

        /// <summary>
        /// Gets or sets the value of the <see cref="MinimumCharacters" />
        /// property. This is a dependency property.
        /// </summary>
        public int MinimumCharacters
        {
            get
            {
                return (int)GetValue(MinimumCharactersProperty);
            }
            set
            {
                SetValue(MinimumCharactersProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="MinimumCharacters" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty MinimumCharactersProperty = DependencyProperty.Register(
            MinimumCharactersPropertyName,
            typeof(int),
            typeof(AutoCompleteTextBox),
            new UIPropertyMetadata(2, OnMinimumCharactersChanged, OnMinimumCharactersCoerceValue));

        private static void OnMinimumCharactersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = (AutoCompleteTextBox)d;
            self.OnConfigurationChanged();
        }

        private static object OnMinimumCharactersCoerceValue(DependencyObject d, object baseValue)
        {
            var value = (int)baseValue;
            if (value <= 0)
            {
                return 1;
            }

            return value;
        }

        #endregion

        #region ItemTemplate
        /// <summary>
        /// The <see cref="ItemTemplate" /> dependency property's name.
        /// </summary>
        public const string ItemTemplatePropertyName = "ItemTemplate";

        /// <summary>
        /// Gets or sets the value of the <see cref="ItemTemplate" />
        /// property. This is a dependency property.
        /// </summary>
        [Bindable(true)]
        public DataTemplate ItemTemplate
        {
            get
            {
                return (DataTemplate)GetValue(ItemTemplateProperty);
            }
            set
            {
                SetValue(ItemTemplateProperty, value);
            }
        }

        /// <summary>
        /// Identifies the <see cref="ItemTemplate" /> dependency property.
        /// </summary>
        public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
            ItemTemplatePropertyName,
            typeof(DataTemplate),
            typeof(AutoCompleteTextBox),
            new UIPropertyMetadata(null));

        #endregion        
        #endregion

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            
            partTextBox = this.GetTemplateChild(PartTextBox) as TextBox;
            if (partTextBox == null)
            {
                throw new InvalidOperationException("Associated ControlTemplate has a bad part configuration. Expected a TextBox part.");
            }

            partPopup = this.GetTemplateChild(PartPopup) as Popup;
            if (partPopup == null)
            {
                throw new InvalidOperationException("Associated ControlTemplate has a bad part configuration. Expected a Popup part.");
            }

            this.partListBox = this.GetTemplateChild(PartListBox) as ListBox;
            if (this.partListBox == null)
            {
                throw new InvalidOperationException("Associated ControlTemplate has a bad part configuration. Expected a ListBox part.");
            }

            isTemplateApplied = true;

            this.textChangedObservable = Observable.FromEventPattern<TextChangedEventArgs>(partTextBox, "TextChanged")
                            .Select(evt => ((TextBox)evt.Sender).Text);

            this.CaptureWindowMovementForPopupPlacement();
            this.RegisterKeyboardAndMouseEventHandlers();
            this.OnConfigurationChanged();            
        }        

        private void OnConfigurationChanged()
        {
            if (isTemplateApplied)
            {
                this.partTextBox.Text = string.Empty;
                this.Results.Clear();

                this.ClearSubscriptions();
                
                this.TextChangesToPopupVisibility();
                
                this.SubscribeToQueryEvents();
            }
        }      

        private void SubscribeToQueryEvents()
        {
            var textChanged = this.textChangedObservable
                          .Where(text => text != null && text.Length >= this.MinimumCharacters)
                          .DistinctUntilChanged()
                          .Throttle(TimeSpan.FromMilliseconds(this.Delay));

            // Capture the function to avoid access to the dependency property on the subscriber thread.
            var getResultsFunc = this.AutoCompleteQueryResultProvider.GetResults;
            var resultsObservable = from searchTerm in textChanged
                                    from suggestions in
                                        getResultsFunc(new AutoCompleteQuery(searchTerm)).TakeUntil(textChanged)
                                    select suggestions;          

            // Feed results into result list
            this.publishedResultsObservable = resultsObservable.Publish();
            this.publishedResultsObservableConnection = this.publishedResultsObservable.Connect();

            var publishedResultsObservableSubscription = 
                this.publishedResultsObservable.Retry().ObserveOn(this).Subscribe(
                    results =>
                    {
                        IsBusy = false;
                        this.Results.Clear();

                        foreach (var result in results)
                        {
                            this.Results.Add(result);
                        }

                        if (this.partListBox.SelectedItem == null)
                        {
                            this.partListBox.SelectedItem = this.Results.FirstOrDefault();
                        }

                        if (this.partListBox.SelectedItem != null)
                        {
                            this.partListBox.ScrollIntoView(this.partListBox.SelectedItem);
                        }
                    });

            this.subscriptions.Add(publishedResultsObservableSubscription);
        }

        private void TextChangesToPopupVisibility()
        {
            var textChangedBelowQueryMinimum =
                this.textChangedObservable
                          .Where(text => text != null && text.Length < this.MinimumCharacters);

            var textChangedAboveQueryMinimum =
                this.textChangedObservable
                          .Where(text => text != null && text.Length >= this.MinimumCharacters);

            var textBelowMinimumSubscription =
                textChangedBelowQueryMinimum.SubscribeOn(this).Subscribe(ctx =>
                    {
                        this.partPopup.IsOpen = false;
                    });

            this.subscriptions.Add(textBelowMinimumSubscription);

            var textAboveMinimumSubscription =
                            textChangedAboveQueryMinimum.SubscribeOn(this).Subscribe(ctx =>
                                {
                                    this.IsBusy = true;
                                    this.partPopup.IsOpen = true;
                                });

            this.subscriptions.Add(textAboveMinimumSubscription);
        }

        private void SetResultText(IAutoCompleteQueryResult autoCompleteQueryResult)
        {
            this.publishedResultsObservableConnection.Dispose();
            this.partTextBox.Text = autoCompleteQueryResult.Title;
            this.partPopup.IsOpen = false;
            this.publishedResultsObservableConnection = this.publishedResultsObservable.Connect();
            this.partTextBox.CaretIndex = this.partTextBox.Text.Length;
            this.partTextBox.Focus();            
        }

        private void ClearSubscriptions()
        {
            foreach (var subscription in this.subscriptions)
            {
                subscription.Dispose();
            }

            this.subscriptions.Clear();
        }       

        private void CaptureWindowMovementForPopupPlacement()
        {
            var window = Window.GetWindow(this.partTextBox);
            if (window != null)
            {
                WeakEventManager<Window, EventArgs>.AddHandler(
                    window,
                    "LocationChanged",
                    (sender, args) =>
                        {
                            if (!this.partPopup.IsOpen)
                            {
                                return;
                            }

                            var offset = this.partPopup.HorizontalOffset;
                            this.partPopup.HorizontalOffset = offset + 1;
                            this.partPopup.HorizontalOffset = offset;
                        });
            }
        }

        /// <summary>
        /// Registers the keyboard and mouse event handlers.
        /// These event handlers take care of 
        /// </summary>
        private void RegisterKeyboardAndMouseEventHandlers()
        {
            this.partTextBox.PreviewKeyDown += (sender, args) =>
            {
                if (args.Key == Key.Up && this.partListBox.SelectedIndex > 0)
                {
                    this.partListBox.SelectedIndex--;
                }
                else if (args.Key == Key.Down && this.partListBox.SelectedIndex < this.partListBox.Items.Count - 1)
                {
                    this.partListBox.SelectedIndex++;
                }
                else if ((args.Key == Key.Return || args.Key == Key.Enter) && this.partListBox.SelectedIndex != -1)
                {
                    this.SetResultText((IAutoCompleteQueryResult)this.partListBox.SelectedItem);
                    args.Handled = true;
                }
                else if (args.Key == Key.Escape)
                {
                    this.partPopup.IsOpen = false;
                    args.Handled = true;
                }

                this.partListBox.ScrollIntoView(this.partListBox.SelectedItem);
            };

            this.partListBox.PreviewTextInput += (sender, args) => { this.partTextBox.Text += args.Text; };

            this.partListBox.PreviewKeyDown += (sender, args) =>
            {
                if ((args.Key == Key.Return || args.Key == Key.Enter) && this.partListBox.SelectedIndex != -1)
                {
                    this.SetResultText((IAutoCompleteQueryResult)this.partListBox.SelectedItem);
                    args.Handled = true;
                }
                else if (args.Key == Key.Escape)
                {
                    this.partPopup.IsOpen = false;
                    args.Handled = true;
                    this.partTextBox.CaretIndex = this.partTextBox.Text.Length;
                    this.partTextBox.Focus();
                }
            };

            this.partListBox.PreviewMouseDown += (sender, args) =>
            {
                var listboxItem = TreeUtils.FindParent<ListBoxItem>((DependencyObject)args.OriginalSource);
                if (listboxItem != null)
                {
                    this.SetResultText((IAutoCompleteQueryResult)listboxItem.DataContext);
                    args.Handled = true;
                }
            };
        }
    }
}
