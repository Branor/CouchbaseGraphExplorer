using Couchbase;
using Couchbase.Core;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GraphExplorer.Models;
using Northwoods.GoXam.Model;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Couchbase.N1QL;
using Couchbase.Views;

namespace GraphExplorer.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private GraphLinksModel<NodeData, String, String, LinkData> _graphModel;
        private Cluster _cluster;
        private IBucket _bucket;

        private ObservableCollection<NodeData> _nodes { get; set; }
        private ObservableCollection<LinkData> _links { get; set; }

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            _cluster = new Cluster("couchbaseClients/couchbase");
            _bucket = _cluster.OpenBucket();

            // model is a GraphLinksModel using instances of NodeData as the node data
            // and LinkData as the link data
            _graphModel = new GraphLinksModel<NodeData, String, String, LinkData>();
            _graphModel.Modifiable = true;

            _nodes = new ObservableCollection<NodeData>();
            _links = new ObservableCollection<LinkData>();
            //      _nodes = new ObservableCollection<NodeData>() {
            //        new NodeData() { Key="Alpha", Color="LightBlue" },
            //        new NodeData() { Key="Beta", Color="Orange" },
            //        new NodeData() { Key="Gamma", Color="LightGreen" },
            //        new NodeData() { Key="Delta", Color="Pink" }
            //      };

            //      _links = new ObservableCollection<LinkData>() {
            //    new LinkData() { From="Alpha", To="Beta" },
            //    new LinkData() { From="Alpha", To="Gamma" },
            //    new LinkData() { From="Beta", To="Beta" },
            //    new LinkData() { From="Gamma", To="Delta" },
            //    new LinkData() { From="Delta", To="Alpha" }
            //};

            _graphModel.NodesSource = _nodes;
            _graphModel.LinksSource = _links;

            SearchText = "d\\appnaciai3";

        }

        public GraphLinksModel<NodeData, String, String, LinkData> GraphModel
        {
            get { return _graphModel; }
        }


        private string _SearchText;
        public string SearchText
        {
            get { return _SearchText; }
            set
            {
                _SearchText = value;
                RaisePropertyChanged<string>(() => this.SearchText);
            }
        }


        private RelayCommand _LoadCommand;
        public RelayCommand LoadCommand
        {
            get
            {
                if (_LoadCommand == null)
                {
                    _LoadCommand = new RelayCommand(async () =>
                    {
                        _graphModel.StartTransaction("DistinctNodes");
                        _nodes.Clear();
                        _links.Clear();
                        await LoadAllNodesAndLinks(null);
                        _graphModel.CommitTransaction("DistinctNodes");
                    });
                }
                return _LoadCommand;
            }
        }


        private RelayCommand _SearchCommand;
        private const int LINK_TRAVERSAL_DEPTH = 2;
        public RelayCommand SearchCommand
        {
            get
            {
                if (_SearchCommand == null)
                {
                    _SearchCommand = new RelayCommand(() =>
                    {
                        _graphModel.StartTransaction("Clear");
                        _nodes.Clear();
                        _links.Clear();
                        _graphModel.CommitTransaction("Clear");
                        TraverseFromEntity(SearchText);
                    });
                }
                return _SearchCommand;
            }
        }

        private async void TraverseFromEntity(string filter)
        {
            List<LinkData> links = new List<LinkData>();
            List<NodeData> nodes = new List<NodeData>();

            var nodes1 = await GetEntities(filter);
            nodes1.ForEach(n => nodes.Add(n));
            var links1 = await GetLinks(filter);
            links1.ForEach(l => links.Add(l));

            var filter2 = links1.Select(l => l.To).ToList();
            foreach(var f in filter2)
            {
                var nodes2 = await GetEntities(f);
                nodes2.ForEach(n => nodes.Add(n));
                var links2 = await GetLinks(f);
                foreach(var l2 in links2)
                {
                    var nodes3 = await GetEntities(l2.To);
                    nodes3.ForEach(n3 => nodes.Add(n3));
                    links.Add(l2);
                    var links3 = await GetLinks(l2.To);
                    links3.ForEach(l3 => links.Add(l3));
                };
            };

            _graphModel.StartTransaction("Traversal");
            nodes.ForEach(n => _nodes.Add(n));
            links.ForEach(l => _links.Add(l));
            _graphModel.CommitTransaction("Traversal");
        }

        private Task<List<NodeData>> GetEntities(string filter)
        {
            var query = _bucket.CreateQuery("entities", "all").Group(true);
            if (!string.IsNullOrEmpty(filter))
                query = query.StartKey(new string[] { filter, null }).EndKey(new string[] { filter, "\uefff" }).InclusiveEnd(true);

            return _bucket.QueryAsync<dynamic>(query).ContinueWith(t =>
                {
                    if (!string.IsNullOrEmpty(t.Result.Error))
                        Console.WriteLine(t.Result.Error.ToString());
                    if (t.Result.Exception != null)
                        Console.WriteLine(t.Result.Exception.ToString());

                    return t.Result.Rows.Select(row => new NodeData { Key = row.Key[0], Type = row.Key[1] }).ToList();
                });
        }

        private Task<List<LinkData>> GetLinks(string filter)
        {
            var query = _bucket.CreateQuery("links", "all").Group(true);
            if (!string.IsNullOrEmpty(filter))
                query = query.StartKey(new string[] { filter, null }).EndKey(new string[] { filter, "\uefff" }).InclusiveEnd(true);

            return _bucket.QueryAsync<dynamic>(query).ContinueWith(t =>
            {
                if (!string.IsNullOrEmpty(t.Result.Error))
                    Console.WriteLine(t.Result.Error.ToString());
                if (t.Result.Exception != null)
                    Console.WriteLine(t.Result.Exception.ToString());

                return t.Result.Rows.Select(row => new LinkData { From = row.Key[0], To = row.Key[1], Type = row.Key[2] }).ToList();
            });
        }
        private Task LoadAllNodesAndLinks(string filter)
        {
            List<Task> tasks = new List<Task>();
            var entites = GetEntities(filter).ContinueWith(t =>
            {
                foreach (var node in t.Result)
                    _nodes.Add(node);
            }, TaskScheduler.FromCurrentSynchronizationContext());

            var links = GetLinks(filter).ContinueWith(t =>
            {
                foreach (var link in t.Result)
                    _links.Add(link);
            }, TaskScheduler.FromCurrentSynchronizationContext());

            tasks.Add(entites);
            tasks.Add(links);

            return Task.WhenAll(tasks.ToArray());
        }
    }
}