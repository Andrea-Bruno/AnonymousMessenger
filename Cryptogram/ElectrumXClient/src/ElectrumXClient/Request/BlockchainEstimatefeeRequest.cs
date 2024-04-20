using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainEstimatefeeRequest : RequestBase
    {
        public BlockchainEstimatefeeRequest() : base()
        {
            base.Method = "blockchain.estimatefee";
            base.Parameters = null;
        }
    }
}
