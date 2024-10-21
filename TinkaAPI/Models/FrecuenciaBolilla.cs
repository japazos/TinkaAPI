using Newtonsoft.Json;

namespace ProyeccionTinka.Models
{
    public class FrecuenciaBolilla
    {
        [JsonProperty("id")]
        public string Id { get; set; } = string.Empty; // Asegura que Id no es NULL
        public int Bolilla { get; set; }
        public int NumVeces { get; set; }
    }


}
