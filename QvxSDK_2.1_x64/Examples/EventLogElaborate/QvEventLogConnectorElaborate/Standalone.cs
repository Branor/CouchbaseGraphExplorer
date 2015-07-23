using System;
using System.Windows.Forms;
using QlikView.Qvx.QvxLibrary;

namespace QvEventLogConnectorElaborate
{
    public partial class Standalone : Form
    {
        private QvEventLogServer _qvsos;
        private string _connectString = "";
        private string _sPath = "";

        public Standalone()
        {
            InitializeComponent();
            _qvsos = new QvEventLogServer();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "connectButton_Click()");

            if (_qvsos != null)
            {
                _connectString = _qvsos.CreateConnectionString();
                logTextBox.AppendText(String.Format("Connect string: {0}\n", _connectString));
                logTextBox.AppendText(Environment.NewLine);
            }
        }

        private void setPathButton_Click(object sender, EventArgs e)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "setPathButton_Click()");

            var myFile = new SaveFileDialog { Filter = @"QlikView Applications (*.qvx)|*.qvx", FilterIndex = 1, RestoreDirectory = true };

            if (myFile.ShowDialog() == DialogResult.OK)
            {
                _sPath = myFile.FileName;
                var msg = String.Format("Target path: {0}\n", _sPath);
                logTextBox.AppendText(msg);
                logTextBox.AppendText(Environment.NewLine);
                QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, String.Format("generateButton_Click() - {0}", msg));
            }
        }

        private void generateButton_Click(object sender, EventArgs e)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "generateButton_Click()");

            var query = sqlStatementTextBox.Text;

            if (_connectString == "")
            {
                MessageBox.Show("Connection string is empty");
            }
            else if (_sPath == "")
            {
                MessageBox.Show("Target path is empty");
            }
            else if (query == "")
            {
                MessageBox.Show("Query is empty");
            }
            else
            {
                // Checkin if the last character is a ; and if so, removes it below.
                // The reason is that QlikView does this before it sends the query 
                // to the connector.
                var lastCharIx = query.Length - 1;
                var lastCharacter = query[lastCharIx];

                if (lastCharacter != ';')
                {
                    MessageBox.Show("Last character is not a semicolon");
                }
                else
                {
                    query = query.Remove(lastCharIx, 1);

                    EnableWindow(false);
                    Cursor.Current = Cursors.WaitCursor;
                    var msg = "Generating QVX file, please wait...";
                    logTextBox.AppendText(msg);
                    logTextBox.AppendText(Environment.NewLine);
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, String.Format("generateButton_Click() - {0}", msg));

                    try
                    {
                        new QvEventLogServer().RunStandalone("", "StandalonePipe", _connectString, _sPath, query);
                    }
                    catch (Exception exception)
                    {
                        MessageBox.Show(exception.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        logTextBox.AppendText(Environment.NewLine);
                        logTextBox.AppendText(String.Format("Error: {0}", exception.Message));
                        logTextBox.AppendText(Environment.NewLine);
                    }

                    Cursor.Current = Cursors.Default;
                    EnableWindow(true);
                    logTextBox.AppendText(Environment.NewLine);
                    logTextBox.AppendText(String.Format("Done!"));
                    logTextBox.AppendText(Environment.NewLine);
                    QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Notice, "generateButton_Click() - Done!");
                }
            }
        }

        private void EnableWindow(bool state)
        {
            connectBox.Enabled = state;
            targetPathBox.Enabled = state;
            sqlStatementBox.Enabled = state;
            generateQvxBox.Enabled = state;
        }

        private void doneButton_Click(object sender, EventArgs e)
        {
            QvxLog.Log(QvxLogFacility.Application, QvxLogSeverity.Debug, "doneButton_Click()");

            Application.Exit();
        }
    }
}
