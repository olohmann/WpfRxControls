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

namespace WpfRxControls.TestApp.Model
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reflection;

    public class OfflineDictionaryAutoCompleteQuerySource : IAutoCompleteQuerySource
    {
        private readonly Func<string, IObservable<string[]>> dictionaryObservableFunc;

        public OfflineDictionaryAutoCompleteQuerySource()
        {
            // We scan the whole file each time to get some artifical delay. Caching would be an alternative.
            this.dictionaryObservableFunc = query => Observable.Using<string[], StreamReader>(
                () => new StreamReader(GetManifestResourceStream()),
                reader => Observable.FromAsync(reader.ReadToEndAsync)
                                    .SelectMany(
                                        str =>str.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries))
                                    .Where(str => str.ToLower().StartsWith(query.ToLower())).ToArray());
       }        
      
        public string Title
        {
            get
            {
                return "Offline Source";
            }
        }
      
        public Func<AutoCompleteQuery, IObservable<IEnumerable<IAutoCompleteQueryResult>>> QueryResultFunction
        {
            get
            {
                return q => dictionaryObservableFunc(q.Term)
                        .Select(words => words.Select(word => new AutoCompleteQueryResult() { Title = word, Thumbnail = "http://placehold.it/100x100/ff0000/ffffff/&text=Preview"}));
            }
        }

        private Stream GetManifestResourceStream()
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream("WpfRxControls.TestApp.Resources.EnglishDictionary.txt");
        }
    }
}
