using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class ServerVersionResponse : ResponseBase
    {
        [JsonProperty("result")]
        public List<ResponseResult> Result { get; set; }

        public static ServerVersionResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<ServerVersionResponse>(json, Converter<ResponseResult>.Settings);
    }
}
