using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainEstimatefeeRequest : RequestBase
    {
        public BlockchainEstimatefeeRequest() : base()
        {
            Method = "blockchain.estimatefee";
            Parameters = null;
        }
    }
}
