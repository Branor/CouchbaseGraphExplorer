using Couchbase;
using Couchbase.Configuration.Client;
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
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dmloader <log1> [log2] [log3]");
                return;
            }

            List<Task> tasks = new List<Task>();
            foreach(var log in args) {
                var task = ParseLogAsync(log).ContinueWith(t => BulkUpsertEntitiesAsync(t.Result));
                tasks.Add(task);
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

                    if(entities == null || string.IsNullOrEmpty(entities.Value) || entities.Exception != null)
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
            return Task.Run(() =>
            {
                Dictionary<string, string> json = new Dictionary<string, string>();

                if (!File.Exists(path))
                    return json;

                using (StreamReader sr = new StreamReader(path))
                {
                    var header = sr.ReadLine();
                    var fields = UniqueHeaders(header.Split(','));
                    json.Add("schema", JsonConvert.SerializeObject(fields));
                    string line = null;
                    while ((line = sr.ReadLine()) != null)
                    {
                        var values = line.Split(',');
                        if (fields.Length != values.Length)
                            continue;

                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        for (int i = 0; i < fields.Length; i++)
                            dict.Add(fields[i], ParseValue(values[i]));

                        json.Add(Guid.NewGuid().ToString(), JsonConvert.SerializeObject(dict));
                    }
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

        private static object ParseValue(string p)
        {
            decimal d = 0;
            if (decimal.TryParse(p, out d))
                return d;
            else
                return p;
        }

        private static async Task BulkUpsertEntitiesAsync(Dictionary<string, string> entities)
        {
            using (var cluster = new Cluster("couchbaseClients/couchbase"))
            {
                using (var bucket = cluster.OpenBucket())
                {
                    var tasks = entities.Select(e => bucket.UpsertAsync(e.Key, e.Value)).ToArray();
                    await Task.WhenAll(tasks);
                }
            }
        }
    }
}
