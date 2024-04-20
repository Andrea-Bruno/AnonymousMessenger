using System;
using System.Collections.Generic;
using System.Text;

namespace ElectrumXClient.Response
{
    public struct ResponseResult
    {
        public string Value;
        public List<string> ValueArray;

        public static implicit operator ResponseResult(string Value) => new ResponseResult { Value = Value };
        public static implicit operator ResponseResult(List<string> ValueArray) => new ResponseResult { ValueArray = ValueArray };
    }
}
