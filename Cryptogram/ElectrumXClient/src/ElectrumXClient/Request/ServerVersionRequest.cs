using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class ServerVersionRequest : RequestBase
    {
        public ServerVersionRequest() : base()
        {
            base.Method = "server.version";
            base.Parameters = new string[0];
        }
    }
}
