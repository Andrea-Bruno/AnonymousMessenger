using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainScripthashListunspentRequest : RequestBase
    {
        public BlockchainScripthashListunspentRequest() : base()
        {
            base.Method = "blockchain.scripthash.listunspent";
            base.Parameters = null;
        }
    }
}
