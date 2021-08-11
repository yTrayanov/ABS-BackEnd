using Newtonsoft.Json;

namespace ABS_Common.ResponsesModels
{
    public class ResponseObject
    {
        public ResponseObject(bool success, string message)
        {
            this.Success = success;
            this.Message = message;
        }
        public ResponseObject(bool success, string message, object data)
        {
            this.Success = success;
            this.Message = message;
            this.Data = data;
        }

        public ResponseObject(bool success , string message , string error)
        {
            this.Success = success;
            this.Message = message;
            this.Error = error;
        }


        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "data")]
        public object Data { get; set; }

        [JsonProperty(PropertyName = "error")]
        public string Error { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}
