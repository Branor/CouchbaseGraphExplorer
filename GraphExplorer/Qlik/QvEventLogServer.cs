using System;
using QlikView.Qvx.QvxLibrary;
using System.Windows.Interop;
using System.Collections.Generic;
using GraphExplorer.Models;
using Couchbase.Core;

namespace GraphExplorer.Qlik
{
    internal class QvCouchbaseEntityServer : QvxServer
    {
        private List<EntityModel> _models;
        private IBucket _bucket;

        public QvCouchbaseEntityServer(IBucket bucket, List<EntityModel> models):base()
        {
            _bucket = bucket;
            _models = models;
        }

        public override QvxConnection CreateConnection()
        {
            return new QvCouchbaseEntityConnection(_bucket, _models);
        }

        public override string CreateConnectionString()
        {
           var connectionString = String.Format("Server={0};UserId={1};Password={2}", "localhost", "user", "password");

            return connectionString;
        }
    }
}
