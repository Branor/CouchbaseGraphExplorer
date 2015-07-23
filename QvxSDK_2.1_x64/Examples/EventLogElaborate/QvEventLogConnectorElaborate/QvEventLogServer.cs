using System;
using System.Windows.Forms;
using QlikView.Qvx.QvxLibrary;
using System.Windows.Interop;

namespace QvEventLogConnectorElaborate
{
    internal class QvEventLogServer : QvxServer
    {
        public override QvxConnection CreateConnection()
        {
            return new QvEventLogConnection();
        }

        public override string CreateConnectionString()
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "CreateConnectionString()");

            var login = CreateLoginWindowHelper();
            login.ShowDialog();

            string connectionString = null;
            if (login.DialogResult.Equals(true))
            {
                connectionString = String.Format("Server={0};UserId={1};Password={2}",
                    login.GetServer(), login.GetUsername(), login.GetPassword());
            }

            return connectionString;
        }

        private Login CreateLoginWindowHelper()
        {
            // Since the owner of the loginWindow is a Win32 process we need to
            // use WindowInteropHelper to make it modal to its owner.
            var login = new Login();
            var wih = new WindowInteropHelper(login);
            wih.Owner = MParentWindow;

            return login;
        }
    }
}
