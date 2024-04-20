using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class ServerPeersSubscribeRequest : RequestBase
    {
        public ServerPeersSubscribeRequest() : base()
        {
            base.Method = "server.peers.subscribe";
            base.Parameters = new string[0];
        }
    }
}
