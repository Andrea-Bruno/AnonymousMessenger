﻿using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Request
{
    public class BlockchainTransactionGetRequest : RequestBase
    {
        public BlockchainTransactionGetRequest() : base()
        {
            Method = "blockchain.transaction.get";            
        }
    }
}
