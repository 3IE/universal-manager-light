using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Light
    {
        [JsonProperty(PropertyName = "lightId")]
        public long LightId { get; set; }
        [JsonProperty(PropertyName = "color")]
        public Color Color { get; set; }
        [JsonProperty(PropertyName = "state")]
        public bool State { get; set; }
    }
}
