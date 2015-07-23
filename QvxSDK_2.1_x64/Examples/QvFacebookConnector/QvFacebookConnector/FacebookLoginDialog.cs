using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Facebook;
using QvFacebookConnector.Constants;

namespace QvFacebookConnector
{
    /// <summary>
    /// The facebook login dialog.
    /// </summary>
    public partial class FacebookLoginDialog : Form
    {
        // The facebook application Id.
        private const string FacebookApplicationId = "Enter your facebook application Id here";
        private readonly FacebookClient _facebookClient;
        private readonly Uri _loginUrl;

        /// <summary>
        /// Constructor for FacebookLoginDialog.
        /// </summary>
        public FacebookLoginDialog()
        {
            InitializeComponent();

            // The facebook login page may have problem with script debugging. Disable script debugging for the browser.
            WebBrowser.ScriptErrorsSuppressed = true;

            // Check that facebook application Id has been specified.
            // Please remove this check when the facebook application Id has been specified.
            if (FacebookApplicationId.StartsWith("Enter"))
            {
                MessageBox.Show("Please specify your facebook application Id in FacebookLoginDialog.cs before compiling and running the QvFacebookConnector!",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new Exception("Please specify your facebook application Id in FacebookLoginDialog.cs before compiling and running the QvFacebookConnector!");
            }

            // Create the login parameters.
            var loginParameters = new Dictionary<string, object>
                {
                    { "client_id", FacebookApplicationId },
                    { "redirect_uri", "https://www.facebook.com/connect/login_success.html" },
                    { "response_type", "token" },
                    { "display", "popup" }
                };

            var permissionsStrings = Enum.GetNames(typeof(FacebookPermissions));

            if (permissionsStrings.Length > 0)
            {
                var scope = new StringBuilder();
                scope.Append(string.Join(",", permissionsStrings));
                loginParameters["scope"] = scope.ToString();
            }

            _facebookClient = new FacebookClient();
            _loginUrl = _facebookClient.GetLoginUrl(loginParameters);
        }

        /// <summary>
        /// The dialog has been loaded. Navigate the webbrowser to
        /// facebooks login page.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void FacebookLoginDialogLoad(object sender, EventArgs e)
        {
            WebBrowser.Navigate(_loginUrl.AbsoluteUri);
        }

        /// <summary>
        /// The webbrowser has navigated to facebook login page and has begun loading it.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event arguments.</param>
        private void WebBrowserNavigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            FacebookOAuthResult result;
            if (_facebookClient.TryParseOAuthCallbackUrl(e.Url, out result))
            {
                FacebookOAuthResult = result;
                DialogResult = result.IsSuccess ? DialogResult.OK : DialogResult.No;
            }
            else
            {
                FacebookOAuthResult = null;
            }
        }

        /// <summary>
        /// The FacebookOAuthResult.
        /// </summary>
        public FacebookOAuthResult FacebookOAuthResult { get; private set; }
    }
}
