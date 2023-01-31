using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class ServerPeersSubscribeRequest : RequestBase
    {
        public ServerPeersSubscribeRequest() : base()
        {
            Method = "server.peers.subscribe";
            Parameters = new string[0];
        }
    }
}
