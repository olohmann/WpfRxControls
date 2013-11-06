namespace WpfRxControls
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;

    /// <summary>
    /// This class is a wrapper to work around strange issues in the WPF Designer. The WPF designer started to complain
    /// about a dependency property of the Func type without any obvious reason (no runtime issues at all).
    /// </summary>
    public class AutoCompleteQueryResultProvider
    {
        private static readonly Func<AutoCompleteQuery, IObservable<IEnumerable<IAutoCompleteQueryResult>>>
            EmptyResultSet = q => Observable.Empty<IEnumerable<IAutoCompleteQueryResult>>();

        private static readonly AutoCompleteQueryResultProvider EmptyResultProvider = 
            new AutoCompleteQueryResultProvider(EmptyResultSet);

        public static AutoCompleteQueryResultProvider Empty
        {
            get
            {
                return EmptyResultProvider;
            }
        }

        public AutoCompleteQueryResultProvider()
        {
            GetResults = EmptyResultSet;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoCompleteQueryResultProvider" /> class.
        /// </summary>
        /// <param name="getResults">The get results.</param>
        public AutoCompleteQueryResultProvider(Func<AutoCompleteQuery, IObservable<IEnumerable<IAutoCompleteQueryResult>>> getResults)
        {
            this.GetResults = getResults;
        }

        public Func<AutoCompleteQuery, IObservable<IEnumerable<IAutoCompleteQueryResult>>> GetResults { get; private set; }
    }
}
