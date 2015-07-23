using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using CsvHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiGetSample
{
    class Program
    {
        private static Cluster _cluster;
        private static IBucket _bucket;
        private static bool _debug = false;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dmloader <log1> [log2] [log3]");
                return;
            }

            _debug = ConfigurationManager.AppSettings["Debug"] == "true";
            _cluster = new Cluster("couchbaseClients/couchbase");
            _bucket = _cluster.OpenBucket();
            
            List<Task> tasks = new List<Task>();
            foreach (var log in args)
            {
                if (_debug)
                    Enumerable.Range(1, 1000).ToList().ForEach(i =>
                    {
                        var task = ParseLogAsync(log).ContinueWith(t => BulkUpsertEntitiesAsync(t.Result));
                        tasks.Add(task);
                    });
                else
                {
                    var task = ParseLogAsync(log).ContinueWith(t => BulkUpsertEntitiesAsync(t.Result));
                    tasks.Add(task);
                }
            }

            tasks.Add(GenerateViews());

            Task.WaitAll(tasks.ToArray());
        }

        private static async Task GenerateViews()
        {
            using (var cluster = new Cluster("couchbaseClients/couchbase"))
            {
                using (var bucket = cluster.OpenBucket())
                {
                    var manager = bucket.CreateManager(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);
                    var entities = await manager.GetDesignDocumentAsync("entities");
                    var links = await manager.GetDesignDocumentAsync("links");

                    if (entities == null || string.IsNullOrEmpty(entities.Value) || entities.Exception != null)
                    {
                        using (StreamReader sr = new StreamReader("entities.json"))
                        {
                            var json = await sr.ReadToEndAsync();
                            var res = await manager.InsertDesignDocumentAsync("entities", json);
                        }
                    }

                    if (links == null || string.IsNullOrEmpty(links.Value) || links.Exception != null)
                    {
                        using (StreamReader sr = new StreamReader("links.json"))
                        {
                            var json = await sr.ReadToEndAsync();
                            var res = await manager.InsertDesignDocumentAsync("links", json);
                        }
                    }
                }
            }
        }

        private static Task<Dictionary<string, string>> ParseLogAsync(string path)
        {
            return Task.Run(async () =>
            {
                Dictionary<string, string> json = new Dictionary<string, string>();

                if (!File.Exists(path))
                    return json;

                string line = null;
                using (StreamReader sr = new StreamReader(path))
                {
                    var header = sr.ReadLine();
                    var fields = UniqueHeaders(header.Split(','));

                    json.Add("schema", JsonConvert.SerializeObject(fields));

                    do
                    {
                        var counterT = _bucket.IncrementAsync("counter");
                        line = await sr.ReadLineAsync();
                        if (line == null)
                            break;

                        var values = line.Split(',');
                        if (fields.Length != values.Length)
                            continue;

                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        for (int i = 0; i < fields.Length; i++)
                            dict.Add(fields[i], ParseValue(values[i]));

                        #region Debug
                        if (_debug)
                        {
                            if (dict.ContainsKey("Log_Time"))
                            {
                                var date = DateTime.Parse(dict["Log_Time"].ToString());
                                var newDate = date.AddMinutes(new Random().Next(43200) - 43200 / 2);
                                dict["Log_Time"] = newDate.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                        }
                        #endregion

                        var counter = await counterT;
                        json.Add(counter.Value.ToString(), JsonConvert.SerializeObject(dict));
                    }
                    while (line != null);
                }

                return json;
            });
        }

        private static string[] UniqueHeaders(string[] nonUniqueHeaders)
        {
            List<string> headers = new List<string>(nonUniqueHeaders);

            int count = 1;
            for (int i = 0; i < headers.Count; i++)
                if (headers[i] == "FUTURE_USE")
                    headers[i] = "FUTURE_USE" + count++;

            return headers.ToArray();
        }


        private static string[] MergeSchema(string[] nonUniqueHeaders)
        {
            List<string> headers = new List<string>();

            var res = _bucket.Get<string>("schema");
            if (res.Success && res.Value != null) 
            {
                var schema = JsonConvert.DeserializeObject<List<string>>(res.Value);
                schema.ForEach(s =>
                {
                    if (!headers.Contains(s))
                        headers.Add(s);
                });
            }

            nonUniqueHeaders.ToList().ForEach(h =>
            {
                if (!headers.Contains(h))
                    headers.Add(h);
            });

            return headers.ToArray();
        }

        private static object ParseValue(string p)
        {
            var value = p.Trim(',', ' ', '"');
            decimal d = 0;
            if (decimal.TryParse(value, out d))
                return d;
            else
                return value;
        }

        private static async Task BulkUpsertEntitiesAsync(Dictionary<string, string> entities)
        {
            var tasks = entities.Select(e => _bucket.UpsertAsync(e.Key, e.Value)).ToArray();
            await Task.WhenAll(tasks);
        }
    }
}
