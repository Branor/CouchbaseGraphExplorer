using System;

namespace QvFacebookConnector
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main(string[] args)
        {
            if (args != null && args.Length >= 2)
            {
                new QvFacebookServer().Run(args[0], args[1]);
            }
        }
    }
}
