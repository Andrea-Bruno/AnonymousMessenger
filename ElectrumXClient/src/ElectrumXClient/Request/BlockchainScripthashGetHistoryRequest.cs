using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainScripthashGetHistoryRequest : RequestBase
    {
        public BlockchainScripthashGetHistoryRequest() : base()
        {
            Method = "blockchain.scripthash.get_history";
            Parameters = null;
        }
    }
}
