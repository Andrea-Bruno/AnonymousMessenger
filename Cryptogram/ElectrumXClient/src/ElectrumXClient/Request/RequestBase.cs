using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ElectrumXClient.Request
{
    public class RequestBase
    {
        [JsonProperty("id")]
        public int MessageId { get; set; }

        [JsonProperty("method")]
        public string Method { get; set; }

        [JsonProperty("params")]
        public object Parameters { get; set; }

        public byte[] GetRequestData<T>()
        {
            byte[] data = System.Text.Encoding.ASCII.GetBytes(ToJson<T>());
            return data;
        }

        protected string ToJson<T>()
        {
            return JsonConvert.SerializeObject(this, Converter<T>.Settings) + "\n";
        }
    }
}
