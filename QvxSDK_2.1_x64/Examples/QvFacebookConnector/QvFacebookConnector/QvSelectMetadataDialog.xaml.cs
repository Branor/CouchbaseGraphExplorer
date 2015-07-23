using System;
using System.Text;
using System.Windows;
using QvFacebookConnector.Constants;

namespace QvFacebookConnector
{
    /// <summary>
    /// Interaction logic for QvSelectMetadataDialog.xaml
    /// </summary>
    public partial class QvSelectMetadataDialog : Window
    {
        public QvSelectMetadataDialog()
        {
            InitializeComponent();

            foreach (var name in Enum.GetNames(typeof(FacebookMetadataTag)))
            {
                _MetadataListBox.Items.Add(name);
            }
        }

        /// <summary>
        /// The generated statement corresponding to the users selections.
        /// </summary>
        public string Statement { get; private set; }

        /// <summary>
        /// The user has pressed the Ok button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void okBbutton_Click(object sender, EventArgs e)
        {
            var script = new StringBuilder();

            // Depending on the users selections, generate the corresponding statement.
            foreach (var item in _MetadataListBox.SelectedItems)
            {
                var tag = item.ToString();

                script.AppendLine(String.Format("SQL SELECT *\r\nFROM {0};\r\n", tag));
            }

            Statement = script.ToString();

            DialogResult = true;
            Close();
        }
    }
}
