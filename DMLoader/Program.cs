using Couchbase;
using Couchbase.Configuration.Client;
using Couchbase.Core;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
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
        private static ConcurrentBag<string> _filesToImport = new ConcurrentBag<string>();

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Usage: dmloader <log1> [log2] [log3]");
                Console.WriteLine("       dmloader -watch <directory>");
                return;
            }

            _debug = ConfigurationManager.AppSettings["Debug"] == "true";
            _cluster = new Cluster("couchbaseClients/couchbase");
            _bucket = _cluster.OpenBucket();

            if (args[0].StartsWith("-w"))
            {
                if (args.Length < 2 || !Directory.Exists(args[1]))
                {
                    Console.WriteLine("Please specify a valid directory.");
                    return;
                }

                GenerateViews().Wait();

                WatchDirectory(args[1]);
                Console.Read();
            }
            else
            {
                ImportFiles(args);
            }
        }

        private static void WatchDirectory(string directory)
        {
            var watcher = new FileSystemWatcher();
            watcher.Path = directory;
            watcher.Filter = "";
            watcher.Created += DirectoryChanged;
            watcher.Changed += DirectoryChanged;

            watcher.EnableRaisingEvents = true;
            Console.WriteLine(string.Format("Watching {0} for changes.", directory));
        }

        static void DirectoryChanged(object sender, FileSystemEventArgs e)
        {
            if (!_filesToImport.Contains(e.FullPath))
                _filesToImport.Add(e.FullPath);
            Task.Delay(1000).ContinueWith(t =>
            {
                var files = _filesToImport.ToArray();
                _filesToImport = new ConcurrentBag<string>();

                if(files.Length > 0)
                    ImportFiles(files);
            });
        }

        private static void ImportFiles(params string[] files)
        {
            Console.WriteLine("Importing files... ");
            List<Task> tasks = new List<Task>();
            foreach (var file in files)
            {
                Console.WriteLine("Importing " + file);
                if (_debug)
                    Enumerable.Range(1, 1000).ToList().ForEach(i =>
                    {
                        var task = ParseLogAsync(file).ContinueWith(t => BulkUpsertEntitiesAsync(t.Result));
                        tasks.Add(task);
                    });
                else
                {
                    var task = ParseLogAsync(file).ContinueWith(t => BulkUpsertEntitiesAsync(t.Result));
                    tasks.Add(task);
                }
            }

            tasks.Add(GenerateViews());

            Task.WaitAll(tasks.ToArray());
            Console.WriteLine("Done.");
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

                using (StreamReader sr = new StreamReader(path))
                {
                    var config = new CsvConfiguration();
                    config.HasHeaderRecord = true;
                    config.TrimFields = true;
                    config.TrimHeaders = true;
                    var parser = new CsvParser(sr, config);

                    string[] row = parser.Read();

                    var fields = UniqueHeaders(row);

                    do
                    {
                        var counterT = _bucket.IncrementAsync("counter");
                        row = parser.Read();
                        if (row == null)
                            break;

                        if (fields.Length != row.Length)
                        {
                            Console.WriteLine("Warning, header count does not match line count. Headers: {0}, Lines: {1}", fields.Length, row.Length);
                        }

                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        for (int i = 0; i < fields.Length && i < row.Length; i++)
                            dict.Add(fields[i], ParseValue(row[i]));

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
                    while (row != null);

                    fields = MergeSchema(fields);
                    json.Add("schema", JsonConvert.SerializeObject(fields));
                }

                return json;
            });
        }

        private static string[] UniqueHeaders(string[] nonUniqueHeaders)
        {
            List<string> headers = new List<string>(nonUniqueHeaders);
            var pairs = headers.ToLookup(h => h);
            foreach (var pair in pairs)
            {
                if (pair.Count() > 1)
                {
                    var count = 1;
                    for (int i = 0; i < headers.Count; i++)
                        if (headers[i] == pair.Key)
                            headers[i] += count++;
                }
            }

            return headers.ToArray();
        }


        private static string[] MergeSchema(string[] newHeaders)
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

            newHeaders.ToList().ForEach(h =>
            {
                if (!headers.Contains(h))
                    headers.Add(h);
            });

            headers.Sort();
            return headers.Where(h => !string.IsNullOrEmpty(h)).ToArray();
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
