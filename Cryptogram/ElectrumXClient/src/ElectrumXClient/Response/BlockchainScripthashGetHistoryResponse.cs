using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainScripthashGetHistoryResponse : ResponseBase
    {
        [JsonProperty("result")]
        public List<BlockchainScripthashGetHistoryResult> Result { get; set; }

        public static BlockchainScripthashGetHistoryResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainScripthashGetHistoryResponse>(json, Converter<ResponseResult>.Settings);

        public class BlockchainScripthashGetHistoryResult
        {
            [JsonProperty("height")]
            public int Height { get; set; }

            [JsonProperty("tx_hash")]
            public string TxHash { get; set; }
        }
    }
}
