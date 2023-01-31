using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class ServerFeaturesRequest : RequestBase
    {
        public ServerFeaturesRequest() : base()
        {
            Method = "server.features";
            Parameters = new object[0];
        }
    }
}
