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

namespace WpfRxControls.TestApp
{
    using System.Collections.ObjectModel;
    using System.Linq;

    using GalaSoft.MvvmLight;

    using WpfRxControls;
    using WpfRxControls.TestApp.Model;

    public class MainWindowViewModel : ViewModelBase
    {
        private readonly ObservableCollection<IAutoCompleteQuerySource> autoCompleteSources;

        private IAutoCompleteQuerySource currentAutoCompleteQuerySource;

        public MainWindowViewModel()
        {
            this.autoCompleteSources = new ObservableCollection<IAutoCompleteQuerySource>();
            this.autoCompleteSources.Add(new OfflineDictionaryAutoCompleteQuerySource());
            this.autoCompleteSources.Add(new RottenTomatoesAutoCompleteQuerySource());
            this.CurrentAutoCompleteQuerySource = autoCompleteSources.First();
        }

        public ObservableCollection<IAutoCompleteQuerySource> AutoCompleteSources
        {
            get
            {
                return autoCompleteSources;
            }
        }

        public AutoCompleteQueryResultProvider AutoCompleteQueryResultProvider
        {
            get
            {
                return new AutoCompleteQueryResultProvider(this.currentAutoCompleteQuerySource.QueryResultFunction);
            }
        }

        public IAutoCompleteQuerySource CurrentAutoCompleteQuerySource
        {
            get
            {
                return this.currentAutoCompleteQuerySource;
            }

            set
            {
                var val = value ?? this.AutoCompleteSources.First();

                if (this.currentAutoCompleteQuerySource != val)
                {
                    this.RaisePropertyChanging(() => this.CurrentAutoCompleteQuerySource);
                    this.RaisePropertyChanging(() => this.AutoCompleteQueryResultProvider);
                    this.currentAutoCompleteQuerySource = val;
                    this.RaisePropertyChanged(() => this.CurrentAutoCompleteQuerySource);
                    this.RaisePropertyChanged(() => this.AutoCompleteQueryResultProvider);
                }
            }
        }
    }
}
