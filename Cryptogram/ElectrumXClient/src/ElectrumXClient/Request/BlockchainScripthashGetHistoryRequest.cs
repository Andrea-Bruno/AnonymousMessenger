using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainScripthashGetHistoryRequest : RequestBase
    {
        public BlockchainScripthashGetHistoryRequest() : base()
        {
            base.Method = "blockchain.scripthash.get_history";
            base.Parameters = null;
        }
    }
}
