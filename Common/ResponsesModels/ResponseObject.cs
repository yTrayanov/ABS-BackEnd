using Newtonsoft.Json;

namespace ABS_Common.ResponsesModels
{
    public class ResponseObject
    {
        public ResponseObject(string message)
        {
            this.Message = message;
        }
        public ResponseObject(string message, object data)
        {
            this.Message = message;
            this.Data = data;
        }

        public ResponseObject(string message , string error)
        {
            this.Message = message;
            this.Error = error;
        }


        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
