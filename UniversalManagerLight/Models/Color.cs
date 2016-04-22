using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Color
    {
        public Color()
        {
            R = 1;
            G = 1;
            B = 1;
        }

        [JsonProperty(PropertyName = "r")]
        public int R { get; set; }
        [JsonProperty(PropertyName = "g")]
        public int G { get; set; }
        [JsonProperty(PropertyName = "b")]
        public int B { get; set; }
        
        public void SetRedColor()
        {
            R = 1;
            G = 0;
            B = 0;
        }

        public void SetGreenColor()
        {
            R = 0;
            G = 1;
            B = 0;
        }

        public void SetBlueColor()
        {
            R = 0;
            G = 0;
            B = 1;
        }
    }
}
