namespace GraphExplorer.Controllers
{
    using Microsoft.Azure.Graphs;
    using Microsoft.Azure.Documents;
    using System.Linq;
    using System.Web.Http;
    using GraphExplorer.Configuration;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System;
    using Microsoft.Azure.Documents.Client;

    public class GremlinController : ApiController
    {
        static DocDbConfig dbConfig = AppSettings.Instance.GetSection<DocDbConfig>("DocumentDBConfig");

        [HttpGet]
        public async Task<dynamic> Get(string query, string collectionId)
        {
            Database database = DocDbSettings.Client.CreateDatabaseQuery("SELECT * FROM d WHERE d.id = \"" + DocDbSettings.DatabaseId + "\"").AsEnumerable().FirstOrDefault();
            List<DocumentCollection> collections = DocDbSettings.Client.CreateDocumentCollectionQuery(database.SelfLink).ToList();
            DocumentCollection coll = collections.Where(x => x.Id == collectionId).FirstOrDefault();

            var tasks = new List<Task>();
            var results = new List<dynamic>();
            var queries = query.Split(';');

            //split query on ; to allow for multiple queries
            foreach (var q in queries)
            {
                if (!string.IsNullOrEmpty(q))
                {
                    var singleQuery = q.Trim();

                    await ExecuteQuery(coll, singleQuery)
                            .ContinueWith(
                                (task) =>
                                {
                                    results.Add(new { queryText = singleQuery, queryResult = task.Result.Item1, queryStats = task.Result.Item2 });
                                }
                            );
                }
            }

            return results;
        }

        private async Task<Tuple<List<dynamic>, QueryStats>> ExecuteQuery(DocumentCollection coll, string query)
        {
            var results = new List<dynamic>();
            var queryStats = new QueryStats();

            var gremlinQuery = DocDbSettings.Client.CreateGremlinQuery(coll, query, new FeedOptions() { PopulateQueryMetrics = true });
            var dt = DateTime.Now;
            while (gremlinQuery.HasMoreResults)
            {
                var feedResponse = await gremlinQuery.ExecuteNextAsync();
                queryStats.RequestCharge += feedResponse.RequestCharge;
                foreach (var result in feedResponse)
                {
                    results.Add(result);
                }
            }
            queryStats.ExecutionTime = DateTime.Now.Subtract(dt).TotalSeconds;
            return Tuple.Create(results, queryStats);
        }
    }

    public class QueryStats
    {
        public double RequestCharge { get; set; }
        /// <summary>
        /// Query execution time in s
        /// </summary>
        public double ExecutionTime { get; set; }
    }
}