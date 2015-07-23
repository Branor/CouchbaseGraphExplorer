using System;
using System.Collections.Generic;
using System.Text;
using QlikView.Qvx.QvxLibrary;
using Facebook;
using QvFacebookConnector.Constants;

namespace QvFacebookConnector
{
    /// <summary>
    /// The mandatory class that extends the QvxConnection.
    /// </summary>
    public class QvFacebookConnection : QvxConnection
    {
        public FacebookOAuthResult QvFacebookOAuthResult { get; private set; }

        /// <summary>
        /// Initialize the QvFacebookConnection.
        /// </summary>
        public override void Init()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Init()");

            // Log in to facebook
            LoginToFacebook();

            MTables = new List<QvxTable>();

            // Create all supported tables
            var tableDictionary = new Dictionary<FacebookMetadataTag, Func<FacebookMetadataTag, QvxTable>>
                {
                    { FacebookMetadataTag.friends, x => CreatePersonsTable(x) },
                    { FacebookMetadataTag.family, x => CreatePersonsTable(x) },
                    { FacebookMetadataTag.movies, x => CreateEntertainmentTable(x) },
                    { FacebookMetadataTag.television, x => CreateEntertainmentTable(x) },
                    { FacebookMetadataTag.likes, x => CreateEntertainmentTable(x) },
                    { FacebookMetadataTag.books, x => CreateEntertainmentTable(x) },
                    { FacebookMetadataTag.music, x => CreateEntertainmentTable(x) },
                    { FacebookMetadataTag.games, x => CreateEntertainmentTable(x) },
                };

            if (tableDictionary.Keys.Count != Enum.GetNames(typeof(FacebookMetadataTag)).Length)
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Warning, "Init() - Mismatch between the number of supported metadata tags and tables");
            }

            foreach (var key in tableDictionary.Keys)
            {
                MTables.Add(tableDictionary[key](key));
            }
        }

        /// <summary>
        /// Find index of the table specified by tablename.
        /// </summary>
        /// <param name="tableName">Name of the table to be found.</param>
        /// <returns>Index of the table if found. Otherwise -1.</returns>
        public int IndexOfTable(string tableName)
        {
            for (var i = 0; i < MTables.Count; i++)
            {
                if (MTables[i].TableName == tableName)
                {
                    return i;
                }
            }
            return -1;
        }

        #region Helpers

        /// <summary>
        /// Log in to facebook.
        /// </summary>
        private void LoginToFacebook()
        {
            if (QvFacebookOAuthResult == null || !QvFacebookOAuthResult.IsSuccess)
            {
                // Show the facebook login dialog
                var facebookLoginDialog = new FacebookLoginDialog();
                facebookLoginDialog.ShowDialog();

                QvFacebookOAuthResult = facebookLoginDialog.FacebookOAuthResult;

                if (QvFacebookOAuthResult == null)
                {
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "Init() - Facebook Login error. The user may have canceled the operation or there could be network errors.");
                    // Tell QlikView that the login has failed
                    throw new Exception("Facebook login error. The user may have canceled the operation or there could be network errors.");
                }
                if (!QvFacebookOAuthResult.IsSuccess)
                {
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, String.Format("Init() - Facebook Login error: {0}", QvFacebookOAuthResult.ErrorDescription));
                    // Tell QlikView that the login has failed
                    throw new Exception(QvFacebookOAuthResult.ErrorDescription);
                }
            }
        }

        /// <summary>
        /// Connect to facebook and ask for data based on the tag and fields.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <param name="fields">The fields.</param>
        /// <returns>The data from facebook.</returns>
        public dynamic LoadConnectionData(string tag, string fields)
        {
            if (!Enum.IsDefined(typeof(FacebookMetadataTag), tag))
            {
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, String.Format("LoadConnectionData: Tag {0} is not implemented: ", tag));
                throw new NotSupportedException(String.Format("The tag {0} is not implemented", tag));
            }

            dynamic connectionData = null;

            if (QvFacebookOAuthResult != null && QvFacebookOAuthResult.IsSuccess)
            {
                var facebookClient = new FacebookClient(QvFacebookOAuthResult.AccessToken);

                // Create the connection string with the tag and fields
                var connectionString = new StringBuilder();
                connectionString.Append("/me/");
                connectionString.Append(tag);
                if (!String.IsNullOrEmpty(fields))
                {
                    connectionString.Append("?");
                    connectionString.Append(fields);
                }

                try
                {
                    connectionData = facebookClient.Get(connectionString.ToString());
                }
                catch (Exception ex)
                {
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Error, String.Format("LoadConnectionData: {0}", ex.Message));
                    // Forward the error message to QlikView
                    throw;
                }
            }

            return connectionData;
        }

        /// <summary>
        /// Create a PersonsTable.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>The created table.</returns>
        private QvxTable CreatePersonsTable(FacebookMetadataTag tag)
        {
            var tableCreator = new PersonsTableCreator(tag.ToString(), this);
            return tableCreator.GetTable();
        }

        /// <summary>
        /// Create an EntertainmentTable.
        /// </summary>
        /// <param name="tag">The tag.</param>
        /// <returns>The created table.</returns>
        private QvxTable CreateEntertainmentTable(FacebookMetadataTag tag)
        {
            var tableCreator = new EntertainmentTableCreator(tag.ToString(), this);
            return tableCreator.GetTable();
        }

        #endregion Helpers
    }
}
