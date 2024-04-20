using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ElectrumXClient.Response
{
    public class ResponseBase
    {
        [JsonProperty("jsonrpc")]
        protected string JsonRpcVersion { get; set; }

        [JsonProperty("id")]
        protected int MessageId { get; set; }

        protected ResponseBase()
        {
        }
    }
}
