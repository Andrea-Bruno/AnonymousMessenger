using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainScripthashListunspentResponse : ResponseBase
    {
        [JsonProperty("result")]
        public List<BlockchainScripthashListunspentResult> Result { get; set; }

        public static BlockchainScripthashListunspentResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainScripthashListunspentResponse>(json, Converter<ResponseResult>.Settings);

        public class BlockchainScripthashListunspentResult
        {
            [JsonProperty("height")]
            public int Height { get; set; }

            [JsonProperty("tx_pos")]
            public int TxPos { get; set; }

            [JsonProperty("tx_hash")]
            public string TxHash { get; set; }

            [JsonProperty("value")]
            public int Value { get; set; }
        }
    }
}
