using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainBlockHeaderRequest : RequestBase
    {
        public BlockchainBlockHeaderRequest() : base()
        {
            Method = "blockchain.block.header";
            Parameters = new object[2] { 4, 1 };
        }
    }
}
