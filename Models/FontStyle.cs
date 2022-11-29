using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SystemWspomaganiaNauczania.Models
{
    public class FontStyle
    {
        public int ID { get; set; }
        public int Size { get; set; }
        public string FontFace { get; set; }
        public string Color { get; set; }
        public string asd { get; set; }
        public virtual Profile Profile { get; set; }
    }
}