using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Extensions
{
    public static class SerializeResponse
    {
        public static string ToJson<T>(this T self) => JsonConvert.SerializeObject(self, ElectrumXClient.Response.Converter<T>.Settings);
    }

    public static class SerializeRequest
    {
        public static string ToJson<T>(this T self) => JsonConvert.SerializeObject(self, ElectrumXClient.Request.Converter<T>.Settings);
    }
}
