using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;   
using QlikView.Qvx.QvxLibrary;

namespace QvEventLogConnectorSimple
{
    internal class QvEventLogConnection : QvxConnection
    {
        public override void Init()
        {
            QvxLog.SetLogLevels(true, true);

            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Init()");

            var eventLogFields = new QvxField[]
                {
                    new QvxField("Category", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("EntryType", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("Message", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("CategoryNumber", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("Index", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("MachineName", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("Source", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII),
                    new QvxField("TimeGenerated", QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII)
                };

            MTables = new List<QvxTable>
                {
                    new QvxTable
                        {
                            TableName = "ApplicationsEventLog",
                            GetRows = GetApplicationEvents,
                            Fields = eventLogFields
                        }
                };
        }

        private IEnumerable<QvxDataRow> GetApplicationEvents()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "GetApplicationEvents()");

            var ev = new EventLog("Application");

            foreach (var evl in ev.Entries)
            {
                yield return MakeEntry(evl as EventLogEntry, FindTable("ApplicationsEventLog", MTables));
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
            /* Make sure to remove your quotesuffix, quoteprefix, 
             * quotesuffixfordoublequotes, quoteprefixfordoublequotes
             * as defined in selectdialog.js somewhere around here.
             * 
             * In this example it is an escaped double quote that is
             * the quoteprefix/suffix
             */
            query = Regex.Replace(query, "\\\"", "");

            return base.ExtractQuery(query, qvxTables);
        }
    }
}
