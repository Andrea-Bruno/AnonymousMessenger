using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainEstimatefeeResponse : ResponseBase
    {
        [JsonProperty("result")]
        public decimal Result { get; set; }

        public static BlockchainEstimatefeeResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainEstimatefeeResponse>(json, Converter<ResponseResult>.Settings);
    }
}
