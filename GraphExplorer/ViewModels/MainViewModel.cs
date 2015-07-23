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
using Newtonsoft.Json;
using System.IO;
using System.Configuration;
using Couchbase.Management;
using System.Windows;
using System.Windows.Input;
using GraphExplorer.Qlik;

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
        #region Fields
        private const int LINK_TRAVERSAL_DEPTH = 2;
        private int PageSize = 1000;
        
        private GraphLinksModel<NodeData, String, String, LinkData> _graphModel;
        private Cluster _cluster;
        private IBucket _bucket;
        private IBucketManager _manager;
        private ObservableCollection<NodeData> _nodes { get; set; }
        private ObservableCollection<LinkData> _links { get; set; }
        
        private string _SearchText;
        private RelayCommand _LoadCommand;
        private RelayCommand _SearchCommand;
        #endregion 

        #region Properties
        
        private bool _IsBusy;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set
            {
                _IsBusy = value;
                RaisePropertyChanged<bool>(() => this.IsBusy);
            }
        }

        
        private bool _IsIndeterminate;
        public bool IsIndeterminate
        {
            get { return _IsIndeterminate; }
            set
            {
                _IsIndeterminate = value;
                RaisePropertyChanged<bool>(() => this.IsIndeterminate);
            }
        }

        
        private long _MaximumProgress;
        public long MaximumProgress
        {
            get { return _MaximumProgress; }
            set
            {
                _MaximumProgress = value;
                RaisePropertyChanged<long>(() => this.MaximumProgress);
            }
        }

        
        private long _Progress;
        public long Progress
        {
            get { return _Progress; }
            set
            {
                _Progress = value;
                RaisePropertyChanged<long>(() => this.Progress);
            }
        }
        
        public GraphLinksModel<NodeData, String, String, LinkData> GraphModel
        {
            get { return _graphModel; }
        }
        public string SearchText
        {
            get { return _SearchText; }
            set
            {
                _SearchText = value;
                RaisePropertyChanged<string>(() => this.SearchText);
            }
        }
        
        private List<String> _Fields;
        public List<String> Fields
        {
            get { return _Fields; }
            set
            {
                _Fields = value;
                RaisePropertyChanged<List<String>>(() => this.Fields);
            }
        }

        
        private ObservableCollection<EntityModel> _EntityModels;
        public ObservableCollection<EntityModel> EntityModels
        {
            get { return _EntityModels; }
            set
            {
                _EntityModels = value;
                RaisePropertyChanged<ObservableCollection<EntityModel>>(() => this.EntityModels);
            }
        }
        
        private ObservableCollection<EntityRelationModel> _EntityRelationModels;
        public ObservableCollection<EntityRelationModel> EntityRelationModels
        {
            get { return _EntityRelationModels; }
            set
            {
                _EntityRelationModels = value;
                RaisePropertyChanged<ObservableCollection<EntityRelationModel>>(() => this.EntityRelationModels);
            }
        }

        
        private EntityModel _SelectedEntityModel;
        public EntityModel SelectedEntityModel
        {
            get { return _SelectedEntityModel; }
            set
            {
                _SelectedEntityModel = value;
                RaisePropertyChanged<EntityModel>(() => this.SelectedEntityModel);
            }
        }

        
        private string _SelectedAttribute;
        public string SelectedAttribute
        {
            get { return _SelectedAttribute; }
            set
            {
                _SelectedAttribute = value;
                RaisePropertyChanged<string>(() => this.SelectedAttribute);
            }
        }
        
                
        private string _SelectedField;
        public string SelectedField
        {
            get { return _SelectedField; }
            set
            {
                _SelectedField = value;
                RaisePropertyChanged<string>(() => this.SelectedField);
                AddEntityModelCommand.RaiseCanExecuteChanged();
                AddEntityRelationModelCommand.RaiseCanExecuteChanged();
            }
        }
                

        #endregion

        #region Commands
        public RelayCommand LoadCommand
        {
            get
            {
                if (_LoadCommand == null)
                {
                    _LoadCommand = new RelayCommand(async () =>
                    {
                        IsBusy = true;
                        _graphModel.StartTransaction("DistinctNodes");
                        _nodes.Clear();
                        _links.Clear();
                        await LoadAllNodesAndLinks(null);
                        _graphModel.CommitTransaction("DistinctNodes");
                        IsBusy = false;
                    });
                }
                return _LoadCommand;
            }
        }
        public RelayCommand SearchCommand
        {
            get
            {
                if (_SearchCommand == null)
                {
                    _SearchCommand = new RelayCommand(async () =>
                    {
                        IsBusy = true;
                        _graphModel.StartTransaction("Clear");
                        _nodes.Clear();
                        _links.Clear();
                        _graphModel.CommitTransaction("Clear");
                        await TraverseFromEntity(SearchText);
                        IsBusy = false;
                    });
                }
                return _SearchCommand;
            }
        }


        private RelayCommand _MaterializeDataModelCommand;
        public RelayCommand MaterializeDataModelCommand
        {
            get
            {
                if (_MaterializeDataModelCommand == null)
                {
                    _MaterializeDataModelCommand = new RelayCommand(async () =>
                    {
                        IsBusy = true;

                        await MaterializeDataModel();

                        IsBusy = false;
                    });
                }
                return _MaterializeDataModelCommand;
            }
        }
      
        
        private RelayCommand _AddEntityModelCommand;
        public RelayCommand AddEntityModelCommand
        {
            get
            {
                if (_AddEntityModelCommand == null)
                {
                    _AddEntityModelCommand = new RelayCommand(() =>
                    {
                        EntityModels.Add(new EntityModel { Field = SelectedField, Name = SelectedField, TimeField = Fields[0], Attributes = new ObservableCollection<string> { SelectedField } });
                    }, () => SelectedField != null);
                }
                return _AddEntityModelCommand;
            }
        }

        
        private RelayCommand _AddEntityRelationModelCommand;
        public RelayCommand AddEntityRelationModelCommand
        {
            get
            {
                if (_AddEntityRelationModelCommand == null)
                {
                    _AddEntityRelationModelCommand = new RelayCommand(() =>
                    {
                        EntityRelationModels.Add(new EntityRelationModel { From = SelectedField, To = SelectedField, Name = SelectedField + "_"  + SelectedField });
                    }, () => SelectedField != null);
                }
                return _AddEntityRelationModelCommand;
            }
        }

        
        private RelayCommand<object> _RemoveItemCommand;
        public RelayCommand<object> RemoveItemCommand
        {
            get
            {
                if (_RemoveItemCommand == null)
                {
                    _RemoveItemCommand = new RelayCommand<object>(item =>
                    {
                        if (item is EntityModel)
                            EntityModels.Remove(item as EntityModel);
                        else if (item is EntityRelationModel)
                            EntityRelationModels.Remove(item as EntityRelationModel);
                    });
                }
                return _RemoveItemCommand;
            }
        }

        
        private RelayCommand<string> _ExportQvxCommand;
        public RelayCommand<string> ExportQvxCommand
        {
            get
            {
                if (_ExportQvxCommand == null)
                {
                    _ExportQvxCommand = new RelayCommand<string>(name =>
                    {
                        GenerateQvx(name);
                    });
                }
                return _ExportQvxCommand;
            }
        }
      
        
        private RelayCommand _ApplyEntityModelsCommand;
        public RelayCommand ApplyEntityModelsCommand
        {
            get
            {
                if (_ApplyEntityModelsCommand == null)
                {
                    _ApplyEntityModelsCommand = new RelayCommand(async () =>
                    {
                        await ApplyEntityViews(EntityModels);
                    });
                }
                return _ApplyEntityModelsCommand;
            }
        }

        
        private RelayCommand _ApplyEntityRelationModelsCommand;
        public RelayCommand ApplyEntityRelationModelsCommand
        {
            get
            {
                if (_ApplyEntityRelationModelsCommand == null)
                {
                    _ApplyEntityRelationModelsCommand = new RelayCommand(async () =>
                    {
                        await ApplyEntityRelationViews(EntityRelationModels);
                    });
                }
                return _ApplyEntityRelationModelsCommand;
            }
        }

        
        private RelayCommand<KeyEventArgs> _KeyUpCommand;
        public RelayCommand<KeyEventArgs> KeyUpCommand
        {
            get
            {
                if (_KeyUpCommand == null)
                {
                    _KeyUpCommand = new RelayCommand<KeyEventArgs>(args =>
                    {
                        if (args.Key == Key.Delete || args.Key == Key.Back)
                            if (SelectedEntityModel != null && SelectedAttribute != null)
                                SelectedEntityModel.Attributes.Remove(SelectedAttribute);
                    });
                }
                return _KeyUpCommand;
            }
        }
      
        
        #endregion

        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (ViewModelBase.IsInDesignModeStatic)
                return;

            IsBusy = false;
            IsIndeterminate = true;
            EntityModels = new ObservableCollection<EntityModel>();
            EntityRelationModels = new ObservableCollection<EntityRelationModel>();

            _cluster = new Cluster("couchbaseClients/couchbase");
            _bucket = _cluster.OpenBucket();
            _manager = _bucket.CreateManager(ConfigurationManager.AppSettings["Username"], ConfigurationManager.AppSettings["Password"]);

            // model is a GraphLinksModel using instances of NodeData as the node data
            // and LinkData as the link data
            _graphModel = new GraphLinksModel<NodeData, String, String, LinkData>();
            _graphModel.Modifiable = true;

            _nodes = new ObservableCollection<NodeData>();
            _links = new ObservableCollection<LinkData>();

            _graphModel.NodesSource = _nodes;
            _graphModel.LinksSource = _links;
            
            SearchText = ConfigurationManager.AppSettings["DefaultEntity"];
            int.TryParse(ConfigurationManager.AppSettings["PageSize"], out PageSize);
            PageSize = PageSize > 0 ? PageSize : 1000;

            LoadSchema();
            LoadState();
        }

        private async void LoadState()
        {
            EntityModels = await LoadModelState<EntityModel>("entitymodels");
            EntityRelationModels = await LoadModelState<EntityRelationModel>("entityrelationmodels");
        }

        private void LoadSchema()
        {
            Task.Run(() =>
            {
                var schema = _bucket.Get<string>("schema");
                if (schema != null && schema.Success)
                    Fields = new List<string>(JsonConvert.DeserializeObject<string[]>(schema.Value));
            });
        }
        
        private async Task TraverseFromEntity(string filter)
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
            var query = _bucket.CreateQuery("entities", "all").GroupLevel(2);
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

        private async Task ApplyEntityViews(ObservableCollection<EntityModel> models)
        {
            await SaveModelState<EntityModel>("entitymodels", models);
            var entityTemplate = "['{0}', '{1}', '{2}']";
            var js = models.Select(m => string.Format(entityTemplate, m.Field, m.Name, m.TimeField)).Aggregate((s1, s2) => s1 + "," + s2);

            using (StreamReader sr = new StreamReader("entities.json"))
            {
                var designDocTemplate = await sr.ReadToEndAsync();
                var designDoc = designDocTemplate.Replace("{0}", js);
                var res = await _manager.UpdateDesignDocumentAsync("entities", designDoc);
                if (res.Success)
                    MessageBox.Show("Entity indexing updated.");
            }
        }
        private async Task ApplyEntityRelationViews(ObservableCollection<EntityRelationModel> models)
        {
            await SaveModelState<EntityRelationModel>("entityrelationmodels", models);
            var linkTemplate = "if(doc.{0} && doc.{1})  emit([doc.{0}, doc.{1}, '{2}']); ";

            var js = models.Select(m => string.Format(linkTemplate, m.From, m.To, m.Name)).Aggregate((s1, s2) => s1 + " " + s2);

            using (StreamReader sr = new StreamReader("links.json"))
            {
                var designDocTemplate = await sr.ReadToEndAsync();
                var designDoc = designDocTemplate.Replace("{0}", js);
                var res = await _manager.UpdateDesignDocumentAsync("links", designDoc);
                if (res.Success)
                    MessageBox.Show("Entity links indexing updated.");
            }
        }

        private async Task SaveModelState<T>(string name, ObservableCollection<T> models)
        {
            var state = JsonConvert.SerializeObject(models);
            using (StreamWriter sw = new StreamWriter(name + ".json"))
            {
                await sw.WriteAsync(state);
            }
        }
        private async Task<ObservableCollection<T>> LoadModelState<T>(string name)
        {
            if (!File.Exists(name + ".json"))
                return new ObservableCollection<T>();

            using (StreamReader sr = new StreamReader(name + ".json"))
            {
                var json = await sr.ReadToEndAsync();
                var state = JsonConvert.DeserializeObject<ObservableCollection<T>>(json);
                return state;
            }
        }

        private async Task MaterializeDataModel()
        {
            IsIndeterminate = false;
            var entityMap = EntityModels.ToLookup(e => e.Name);

            using (var materializedBucket = _cluster.OpenBucket("materialized"))
            {
                var query = _bucket.CreateQuery("entities", "all").Group(false).Reduce(true);
                MaximumProgress = (await _bucket.QueryAsync<long>(query)).Rows.First().Value;
                Progress = 0;

                var limit = PageSize;
                var offset = 0;
                int rows = 0;
                string name, type, id;
                EntityModel model;
                List<Task> tasks;

                do
                {
                    tasks = new List<Task>();
                    query = _bucket.CreateQuery("entities", "all").Reduce(false).Limit(limit).Skip(offset);
                    var result = await _bucket.QueryAsync<dynamic>(query);
                    var entities = new List<Dictionary<string, object>>();

                    rows = result.Rows.Count();
                    foreach (var row in result.Rows)
                    {
                        id = row.Id;
                        name = row.Key[0];
                        type = row.Key[1];
                        model = entityMap[type].FirstOrDefault();
                        tasks.Add(MaterializeSingleEntity(id, name, type, model, _bucket, materializedBucket));
                    }

                    await Task.WhenAll(tasks.ToArray());
                    tasks = new List<Task>();
                    offset += rows;
                    Progress = offset;
                }
                while (rows > 0);
            }

            IsIndeterminate = true;
        }

        private async Task MaterializeSingleEntity(string id, string name, string type, EntityModel model, IBucket _bucket, IBucket materializedBucket)
        {
            var docT =_bucket.GetAsync<string>(id);
            var counterT = materializedBucket.IncrementAsync(type + "::counter");

            var counter = await counterT;
            var doc = await docT;

            var raw = JsonConvert.DeserializeObject<Dictionary<string, object>>(doc.Value);
            var entity = new Dictionary<string, object>();
            entity.Add("Name", name);
            entity.Add("Type", type);
            foreach (var attribute in model.Attributes)
                if (raw.ContainsKey(attribute))
                    entity.Add(attribute, raw[attribute]);

            await materializedBucket.UpsertAsync(type + "::" + name + "::" + counter.Value.ToString(), entity);
        }

        private Task GenerateQvx(string entityName)
        {
            return Task.Run(() =>
            {
                IsBusy = true;
                var server = new QvCouchbaseEntityServer(_bucket, EntityModels.ToList());
                server.RunStandalone("", "StandalonePipe", "", string.Format(@"c:\temp\{0}.qvx", entityName), string.Format("select * from {0}", entityName));
                IsBusy = false;
            });
        }
    }
}