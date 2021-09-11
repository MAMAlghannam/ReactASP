using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReactASP.Model
{
    public class Time
    {
        public string At { get; set; }
        public bool IsBooked { get; set; } = false;
        public string UserName { get; set; } = null;
    }
}
