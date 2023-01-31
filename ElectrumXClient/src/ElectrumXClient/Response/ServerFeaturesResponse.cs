using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class ServerFeaturesResponse : ResponseBase
    {
        [JsonProperty("result")]
        public ServerFeaturesResult Result { get; set; }

        public static ServerFeaturesResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<ServerFeaturesResponse>(json);

        public class ServerFeaturesResult
        {
            [JsonProperty("hosts")]
            public dynamic Hosts { get; set; }

            [JsonProperty("pruning")]
            public dynamic Pruning { get; set; }

            [JsonProperty("server_version")]
            public string ServerVersion { get; set; }

            [JsonProperty("protocol_min")]
            public string ProtocolMin { get; set; }

            [JsonProperty("protocol_max")]
            public string ProtocolMax { get; set; }

            [JsonProperty("genesis_hash")]
            public string GenesisHash { get; set; }

            [JsonProperty("hash_function")]
            public string HashFunction { get; set; }
        }
    }
}
