using Newtonsoft.Json;

namespace ProyeccionTinka.Models
{
    public class FrecuenciaBolilla
    {
        [JsonProperty("id")]
        public string Id => Bolilla.ToString();
        public int Bolilla { get; set; }
        public int NumVeces { get; set; }
    }

}
