using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainScripthashGetBalanceRequest : RequestBase
    {
        public BlockchainScripthashGetBalanceRequest() : base()
        {
            Method = "blockchain.scripthash.get_balance";
            Parameters = null;
        }
    }
}
