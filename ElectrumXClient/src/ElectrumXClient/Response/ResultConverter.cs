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
    internal class ResponseConverter<T> : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(T);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.String:
                case JsonToken.Date:
                    var stringValue = serializer.Deserialize<string>(reader);
                    return new ResponseResult { Value = stringValue };
                case JsonToken.StartArray:
                    var arrayValue = serializer.Deserialize<List<string>>(reader);
                    return new ResponseResult { ValueArray = arrayValue };
                case JsonToken.StartObject:
                    return "";
            }
            throw new Exception("Cannot unmarshal response");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            var value = (ResponseResult)untypedValue;
            if (value.Value != null)
            {
                serializer.Serialize(writer, value.Value);
                return;
            }
            if (value.ValueArray != null)
            {
                serializer.Serialize(writer, value.ValueArray);
                return;
            }
            throw new Exception("Cannot marshal response");
        }

        public static readonly ResponseConverter<T> Singleton = new ResponseConverter<T>();
    }
}
