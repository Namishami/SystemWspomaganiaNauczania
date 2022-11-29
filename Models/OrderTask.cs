using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SystemWspomaganiaNauczania.Models
{
    public class OrderTask
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string  Description { get; set; }
        public string Image { get; set; }
        public string Author { get; set; }
        public virtual List<Comment> Comments { get; set; }
    }
}