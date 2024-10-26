using Newtonsoft.Json;

namespace ProyeccionTinka.Models
{
    public class FrecuenciaBolilla
    {
        [JsonProperty("id")]
        public int Bolilla { get; set; }
        public int NumVeces { get; set; }
    }


}
