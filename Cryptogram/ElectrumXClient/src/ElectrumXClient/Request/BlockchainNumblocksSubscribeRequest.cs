using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainNumblocksSubscribeRequest:RequestBase
    {
        public BlockchainNumblocksSubscribeRequest() : base()
        {
            base.Method = "blockchain.numblocks.subscribe";
            base.Parameters = new string[0];
        }
    }
}
