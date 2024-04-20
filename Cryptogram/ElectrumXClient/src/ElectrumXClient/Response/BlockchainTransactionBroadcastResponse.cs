using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainTransactionBroadcastResponse : ResponseBase
    {
        [JsonProperty("result")]
        public String Result { get; set; }

        public static BlockchainTransactionBroadcastResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainTransactionBroadcastResponse>(json, Converter<ResponseResult>.Settings);

    }
}
