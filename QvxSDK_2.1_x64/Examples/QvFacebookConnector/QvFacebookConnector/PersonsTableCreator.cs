using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QlikView.Qvx.QvxLibrary;
using QvFacebookConnector.Constants;

namespace QvFacebookConnector
{
    /// <summary>
    /// This class creates a QvxTable from the friends and family facebook tags.
    /// </summary>
    public class PersonsTableCreator : IFacebookTableCreator
    {
        private readonly QvFacebookConnection _facebookConnection;
        private readonly string _tableName;
        private QvxTable _table;

        /// <summary>
        /// Constructor for PersonsTableCreator.
        /// </summary>
        /// <param name="tableName">Name of the QvxTable.</param>
        /// <param name="facebookConnection">The facebook connection.</param>
        public PersonsTableCreator(string tableName, QvFacebookConnection facebookConnection)
        {
            _facebookConnection = facebookConnection;
            _tableName = tableName;
            CreateTable();
        }

        /// <summary>
        /// Get the QvxTable.
        /// </summary>
        /// <returns>The QvxTable.</returns>
        public QvxTable GetTable()
        {
            return _table;
        }

        /// <summary>
        /// Create the QvxTable.
        /// </summary>
        private void CreateTable()
        {
            _table = new QvxTable();

            _table.TableName = _tableName;
            _table.GetRows = GetRows;

            var friendsFields = new List<QvxField>();

            foreach (var field in Enum.GetNames(typeof(PersonsFields)))
            {
                friendsFields.Add(new QvxField(field, QvxFieldType.QVX_TEXT, QvxNullRepresentation.QVX_NULL_FLAG_SUPPRESS_DATA, FieldAttrType.ASCII));
            }

            _table.Fields = friendsFields.ToArray();
        }

        /// <summary>
        /// Return all rows in the QvxTable.
        /// </summary>
        /// <returns>IEnumerable with all QvxDataRow elements of m_Table member.</returns>
        private IEnumerable<QvxDataRow> GetRows()
        {
            var connectionString = new StringBuilder();

            // The default return fields for a friend is id and name. We need more information for the QlikView example application.
            connectionString.Append("fields=");

            var supportedFields = Enum.GetNames(typeof(PersonsFields));
            foreach (var field in supportedFields)
            {
                connectionString.Append(field);
                if (field != supportedFields.Last())
                {
                    connectionString.Append(",");
                }
            }

            var rows = new List<QvxDataRow>();

            try
            {
                var connectionData = _facebookConnection.LoadConnectionData(_tableName, connectionString.ToString());

                foreach (var item in connectionData["data"])
                {
                    var row = new QvxDataRow();

                    for (int i = 0; i < supportedFields.Length; i++)
                    {
                        var field = supportedFields[i];
                        var tableIndex = _facebookConnection.IndexOfTable(_tableName);

                        try
                        {
                            if (field != "hometown" && field != "languages")
                            {
                                row[_facebookConnection.MTables[tableIndex].Fields[i]] = item[field];
                            }
                            else if (field == "hometown")
                            {
                                // Get the name of hometown
                                row[_facebookConnection.MTables[tableIndex].Fields[i]] = item[field]["name"];
                            }
                            else
                            {
                                // Just take the first language in the list
                                row[_facebookConnection.MTables[tableIndex].Fields[i]] = item[field][0]["name"];
                            }
                        }
                        catch (KeyNotFoundException)
                        {
                            // If the field does not exist set it to empty (the person has not specified the information)
                            row[_facebookConnection.MTables[tableIndex].Fields[i]] = "";
                        }
                    }

                    rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, String.Format("FriendsOrFamilyTable GetRows for {0} :{1}", _tableName, ex.Message));
                // Forward the error message to QlikView
                throw;
            }

            return rows;
        }
    }
}
