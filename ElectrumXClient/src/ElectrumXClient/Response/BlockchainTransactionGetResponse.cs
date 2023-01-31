using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public class BlockchainTransactionGetResponse : ResponseBase
    {
        [JsonProperty("result")]
        public BlockchainTransactionGetResult Result { get; set; }

        public static BlockchainTransactionGetResponse FromJson(string json) =>
            JsonConvert.DeserializeObject<BlockchainTransactionGetResponse>(json, Converter<ResponseResult>.Settings);

        public class BlockchainTransactionGetResult
        {
            [JsonProperty("hex")]
            public string Hex { get; set; }

            [JsonProperty("txid")]
            public string Txid { get; set; }

            [JsonProperty("size")]
            public long Size { get; set; }

            [JsonProperty("version")]
            public long Version { get; set; }

            [JsonProperty("locktime")]
            public long Locktime { get; set; }

            [JsonProperty("vin")]
            public List<Vin> VinValue { get; set; }

            [JsonProperty("vout")]
            public List<Vout> VoutValue { get; set; }

            [JsonProperty("blockhash")]
            public string Blockhash { get; set; }

            [JsonProperty("height")]
            public long Height { get; set; }

            [JsonProperty("confirmations")]
            public long Confirmations { get; set; }

            [JsonProperty("time")]
            public long Time { get; set; }

            [JsonProperty("blocktime")]
            public long Blocktime { get; set; }

            public class Vin
            {
                [JsonProperty("coinbase")]
                public string Coinbase { get; set; }

                [JsonProperty("sequence")]
                public long Sequence { get; set; }
            }

            public class Vout
            {
                [JsonProperty("value")]
                public double Value { get; set; }

                [JsonProperty("valueSat")]
                public long ValueSat { get; set; }

                [JsonProperty("n")]
                public long N { get; set; }

                [JsonProperty("scriptPubKey")]
                public ScriptPubKey ScriptPubKey { get; set; }
            }

            public class ScriptPubKey
            {
                [JsonProperty("asm")]
                public string Asm { get; set; }

                [JsonProperty("hex")]
                public string Hex { get; set; }

                [JsonProperty("reqSigs")]
                public long ReqSigs { get; set; }

                [JsonProperty("type")]
                public string Type { get; set; }

                [JsonProperty("addresses")]
                public List<string> Addresses { get; set; }
            }
        }
    }
}
