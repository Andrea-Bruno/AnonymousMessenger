using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainNumblocksSubscribeRequest:RequestBase
    {
        public BlockchainNumblocksSubscribeRequest() : base()
        {
            Method = "blockchain.numblocks.subscribe";
            Parameters = new string[0];
        }
    }
}
