using System;
using System.Text.Json.Serialization;

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

        public string Message { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public object Data { get; set; }

        [JsonPropertyName("error")]
        public string Error { get; set; }
    }
}
