using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainBlockHeaderResponse : ResponseBase
    {
        [JsonProperty("result")]
        public ResponseResult Result { get; set; }

        public static BlockchainBlockHeaderResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainBlockHeaderResponse>(json, Converter<ResponseResult>.Settings);
    }
}
