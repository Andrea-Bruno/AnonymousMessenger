using Newtonsoft.Json;
using System.Collections.Generic;

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
