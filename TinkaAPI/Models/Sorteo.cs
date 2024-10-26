using Newtonsoft.Json;
using System;

namespace ProyeccionTinka.Models
{
    public class Sorteo
    {
        [JsonProperty("id")]
        public DateTime Fecha { get; set; }
        public int Bolilla1 { get; set; }
        public int Bolilla2 { get; set; }
        public int Bolilla3 { get; set; }
        public int Bolilla4 { get; set; }
        public int Bolilla5 { get; set; }
        public int Bolilla6 { get; set; }
    }




}
