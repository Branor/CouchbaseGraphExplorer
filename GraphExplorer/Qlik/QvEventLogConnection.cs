using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Authentication;
using System.Text.RegularExpressions;
using QlikView.Qvx.QvxLibrary;
using GraphExplorer.Models;
using Couchbase.Core;
using System.Configuration;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;

namespace GraphExplorer.Qlik
{
    class QvCouchbaseEntityConnection : QvxConnection
    {
        private List<EntityModel> _models;
        private IBucket _bucket;
        private int PageSize = 1000;

        public QvCouchbaseEntityConnection(IBucket bucket, List<EntityModel> models)
            : base()
        {
            _bucket = bucket;
            _models = models;

            int.TryParse(ConfigurationManager.AppSettings["PageSize"], out PageSize);
            PageSize = PageSize > 0 ? PageSize : 1000;

        }

        // Has been hardcoded, should preferably be done programatically.
        public override void Init()
        {
            List<QvxTable> tables = new List<QvxTable>();

            foreach (var model in _models)
            {
                List<QvxField> fields = new List<QvxField>();
                fields.Add(new QvxField("Name", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
                fields.Add(new QvxField(model.Field, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
                fields.Add(new QvxField(model.TimeField, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
                foreach (var attribute in model.Attributes)
                {
                    fields.Add(new QvxField(attribute, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
                }

                tables.Add(new QvxTable
                {
                    TableName = model.Name,
                    GetRows = () => GetEntities(model.Name),
                    Fields = fields.ToArray()
                });
            }

            MTables = tables;

            //var applicationsEventLogFields = new QvxField[]
            //    {
            //        new QvxField("Category", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("EntryType", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("Message", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("CategoryNumber", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("Index", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("MachineName", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("Source", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("TimeGenerated", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII)
            //    };

            //var systemEventLogFields = new QvxField[]
            //    {
            //        new QvxField("Category", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("EntryType", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("Message", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("CategoryNumber", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("Index", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("MachineName", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("Source", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
            //        new QvxField("TimeGenerated", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII)
            //    };

            //MTables = new List<QvxTable> {
            //    new QvxTable {
            //        TableName = "User",
            //        GetRows = GetUsers,
            //        Fields = applicationsEventLogFields
            //    },
            //    new QvxTable {
            //        TableName = "Host",
            //        GetRows = GetHosts,
            //        Fields = systemEventLogFields
            //    }
            //};
        }

        private IEnumerable<QvxDataRow> GetUsers()
        {
            return GetEvents("Application", "User");
        }

        private IEnumerable<QvxDataRow> GetHosts()
        {
            return GetEvents("System", "Host");
        }

        private IEnumerable<QvxDataRow> GetEntities(string entityName)
        {
            var entityMap = _models.ToLookup(e => e.Name);

            int limit = PageSize;
            int offset = 0;
            int rows = 0;
            EntityModel model;
            List<Task<QvxDataRow>> tasks;

            do
            {
                tasks = new List<Task<QvxDataRow>>();
                var query = _bucket.CreateQuery("entities", "raw").Reduce(false).Limit(limit).Skip(offset);
                var result = _bucket.Query<dynamic>(query);
                var entities = new List<Dictionary<string, object>>();

                rows = result.Rows.Count();
                foreach (var row in result.Rows)
                {
                    var id = row.Id;
                    model = entityMap[entityName].FirstOrDefault();

                    tasks.Add(MakeEntry(id,model, FindTable(entityName, MTables)));                    
                }

                Task.WaitAll(tasks.ToArray());
                foreach (var t in tasks)
                    if(t.Result != null)
                        yield return t.Result;

                offset += rows;
            }
            while (rows > 0);
        }

        private async Task<QvxDataRow> MakeEntry(string id, EntityModel model, QvxTable table)
        {
            var row = new QvxDataRow();
            var doc = await _bucket.GetAsync<string>(id);
            Dictionary<string, object> raw = null;

            try
            {
                raw = JsonConvert.DeserializeObject<Dictionary<string, object>>(doc.Value);
            }
            catch (Exception)
            {
                return null;
            }

            row[table.Fields[0]] = model.Name;
            for (int i = 1; i < table.Fields.Length; i++ )
                row[table.Fields[i]] = raw[table.Fields[i].FieldName].ToString();

            return row;
        }

        private IEnumerable<QvxDataRow> GetEvents(string log, string tableName)
        {
            if (!EventLog.Exists(log))
            {
                throw new QvxPleaseSendReplyException(QvxResult.QVX_TABLE_NOT_FOUND,
                    String.Format("There is no EventLog with name: {0}", tableName));
            }

            var ev = new EventLog(log);

            foreach (var evl in ev.Entries)
            {
                yield return MakeEntry(evl as EventLogEntry, FindTable(tableName, MTables));
            }
        }

        private QvxDataRow MakeEntry(EventLogEntry evl, QvxTable table)
        {
            var row = new QvxDataRow();
            row[table.Fields[0]] = evl.Category;
            row[table.Fields[1]] = evl.EntryType.ToString();
            row[table.Fields[2]] = evl.Message;
            row[table.Fields[3]] = evl.CategoryNumber.ToString();
            row[table.Fields[4]] = evl.Index.ToString();
            row[table.Fields[5]] = evl.MachineName;
            row[table.Fields[6]] = evl.Source;
            row[table.Fields[7]] = evl.TimeGenerated.ToString();
            return row;
        }

        public override QvxDataTable ExtractQuery(string query, List<QvxTable> qvxTables)
        {
            return base.ExtractQuery(query, qvxTables);
        }
    }
}
