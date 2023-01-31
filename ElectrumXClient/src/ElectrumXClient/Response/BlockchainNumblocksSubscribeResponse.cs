using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainNumblocksSubscribeResponse : ResponseBase
    {
        [JsonProperty("result")]
        public ResponseResult Result { get; set; }

        public static BlockchainNumblocksSubscribeResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainNumblocksSubscribeResponse>(json, Converter<ResponseResult>.Settings);
    }
}
