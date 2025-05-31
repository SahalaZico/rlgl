using System.Collections;
using System.Collections.Generic;

namespace BestHTTP.SocketIO.JsonEncoders
{
    using Newtonsoft.Json;

    public class NETEncoder : IJsonEncoder
    {
        public List<object> Decode(string json)
        {
            return JsonConvert.DeserializeObject<List<object>>(json);
        }

        public string Encode(List<object> obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
