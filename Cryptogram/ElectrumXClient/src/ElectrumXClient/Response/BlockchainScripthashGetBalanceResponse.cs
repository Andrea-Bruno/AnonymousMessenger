using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainScripthashGetBalanceResponse : ResponseBase
    {
        [JsonProperty("result")]
        public BlockchainScripthashGetBalanceResult Result { get; set; }

        public static BlockchainScripthashGetBalanceResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainScripthashGetBalanceResponse>(json, Converter<ResponseResult>.Settings);

        public class BlockchainScripthashGetBalanceResult
        {
            [JsonProperty("confirmed")]
            public string Confirmed { get; set; }

            [JsonProperty("unconfirmed")]
            public string Unconfirmed { get; set; }
        }
    }
}
