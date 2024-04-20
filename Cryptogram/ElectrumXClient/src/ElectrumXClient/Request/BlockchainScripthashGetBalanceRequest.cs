using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainScripthashGetBalanceRequest : RequestBase
    {
        public BlockchainScripthashGetBalanceRequest() : base()
        {
            base.Method = "blockchain.scripthash.get_balance";
            base.Parameters = null;
        }
    }
}
