using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SystemWspomaganiaNauczania.Models;

namespace SystemWspomaganiaNauczania.ViewModel
{
    public class SolvedViewModel
    {
        public OrderTask OrderTask { get; set; }
        public GroupTask GroupTask { get; set; }
        public bool Solved { get; set; }
    }
}