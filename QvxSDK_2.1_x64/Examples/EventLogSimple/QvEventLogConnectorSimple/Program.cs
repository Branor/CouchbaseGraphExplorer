using System;
using QvEventLogConnectorSimple;

namespace QvEventLogConnectorElaborate
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args != null && args.Length >= 2)
            {
                new QvEventLogServer().Run(args[0], args[1]);
            }       
        }
    }
}
