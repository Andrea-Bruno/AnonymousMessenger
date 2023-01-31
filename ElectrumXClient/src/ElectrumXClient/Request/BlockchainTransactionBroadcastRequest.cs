using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainTransactionBroadcastRequest : RequestBase
    {
        public BlockchainTransactionBroadcastRequest() : base()
        {
            Method = "blockchain.transaction.broadcast";            
        }
    }
}
