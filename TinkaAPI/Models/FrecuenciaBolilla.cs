using Newtonsoft.Json;

namespace ProyeccionTinka.Models
{
    public class FrecuenciaBolilla
    {
        [JsonProperty("id")]
        public int bolilla { get; set; }
        public int numVeces { get; set; }
        public string documentType { get; set; } = "FrecuenciaBolilla";
    }


}
