using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class ServerVersionRequest : RequestBase
    {
        public ServerVersionRequest() : base()
        {
            Method = "server.version";
            Parameters = new string[0];
        }
    }
}
