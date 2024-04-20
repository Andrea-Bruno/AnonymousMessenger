using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class ServerFeaturesRequest : RequestBase
    {
        public ServerFeaturesRequest() : base()
        {
            base.Method = "server.features";
            base.Parameters = new object[0];
        }
    }
}
