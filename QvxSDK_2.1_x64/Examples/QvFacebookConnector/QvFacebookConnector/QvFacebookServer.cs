using System;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using QlikView.Qvx.QvxLibrary;
using System.Windows.Forms;

namespace QvFacebookConnector
{
    /// <summary>
    /// The mandatory class that extends the QvxServer.
    /// </summary>
    public class QvFacebookServer : QvxServer
    {
        private QvFacebookConnection _qvxConnection;

        /// <summary>
        /// Create an instance of a class that implements QvxConnection,
        /// in this case a QvFacebookConnection.
        /// </summary>
        /// <returns>The created QvxConnection.</returns>
        public override QvxConnection CreateConnection()
        {
            if (_qvxConnection == null)
            {
                _qvxConnection = new QvFacebookConnection();
            }
            return _qvxConnection;
        }

        /// <summary>
        /// Create the connection string.
        /// </summary>
        /// <returns>The connection string.</returns>
        public override string CreateConnectionString()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "CreateConnectionString()");
            // No login string is needed. The facebook login page will ask for the username and password during the connect process.
            return "";
        }

        /// <summary>
        /// Set the label for the custom button in QlikView.
        /// </summary>
        public override string CustomCaption
        {
            get { return "Metadata..."; }
        }

        /// <summary>
        /// Create the select statement.
        /// </summary>
        /// <returns>The select statement.</returns>
        public override string CreateSelectStatement()
        {
            var metadataWindow = CreateSelectWindowHelper();
            metadataWindow.ShowDialog();

            string statement = null;
            if (metadataWindow.DialogResult.Equals(true))
            {
                statement = metadataWindow.Statement;
            }

            return statement;
        }

        private QvSelectMetadataDialog CreateSelectWindowHelper()
        {
            // Since the owner of the metadataWindow is a Win32 process we need to
            // use WindowInteropHelper to make it modal to its owner.
            var metadataWindow = new QvSelectMetadataDialog();
            var wih = new WindowInteropHelper(metadataWindow);
            wih.Owner = MParentWindow;

            return metadataWindow;
        }
    }
}
