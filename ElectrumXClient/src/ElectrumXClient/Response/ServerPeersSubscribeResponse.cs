using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ElectrumXClient.Response
{
    public class ServerPeersSubscribeResponse : ResponseBase
    {
        [JsonProperty("result")]
        public List<List<ResponseResult>> Result { get; set; }

        public static ServerPeersSubscribeResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<ServerPeersSubscribeResponse>(json, Converter<ResponseResult>.Settings);
    }
}
