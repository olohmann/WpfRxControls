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
    using System.Net.Http;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    using WpfRxControls.TestApp.Utils;

    public class RottenTomatoesAutoCompleteQuerySource : IAutoCompleteQuerySource
    {
        private const string ApiKey = "";

        public string Title
        {
            get
            {
                return "Rotten Tomatoes";
            }
        }       

        public Func<AutoCompleteQuery, IObservable<IEnumerable<IAutoCompleteQueryResult>>> QueryResultFunction
        {
            get
            {
                return query => Observable.FromAsync(() => this.SearchRottenTomatoes(query.Term));
            }
        }

        private async Task<IEnumerable<AutoCompleteQueryResult>> SearchRottenTomatoes(string term)
        {
            if (string.IsNullOrWhiteSpace(ApiKey))
            {
                return await 
                    Task.FromResult(new[] 
                            {
                                new AutoCompleteQueryResult()
                                    {
                                        Thumbnail = "http://placehold.it/100x100/ff0000/ffffff/&text=API+Key",
                                        Title = "Api Key Missing! Update in RottenTomatoesAutoCompleteQuerySource.ApiKey"
                                    }
                            });
            }

            string address = string.Format("http://api.rottentomatoes.com/api/public/v1.0/movies.json/?apikey={0}&q={1}&page_limit={2}&page={3}", ApiKey, term, 10, 1);

            var client = new HttpClient(new LegacyJsonMediaTypeConverterDelegatingHandler() { InnerHandler = new HttpClientHandler() });

            HttpResponseMessage response = await client.GetAsync(address);
            response.EnsureSuccessStatusCode();
            JObject content = await response.Content.ReadAsAsync<JObject>();

            var res = new List<AutoCompleteQueryResult>();
            foreach (var movieDescription in content["movies"])
            {
                var movie = new AutoCompleteQueryResult();
                movie.Title = movieDescription["title"].Value<string>();
                movie.Thumbnail = movieDescription["posters"]["thumbnail"].Value<string>();
                res.Add(movie);
            }

            return res;
        }
    }
}
