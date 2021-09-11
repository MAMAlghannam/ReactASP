using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReactASP.Model
{
    public class BookInformation
    {
        [Required]
        public string Date { get; set; }
        [Required]
        public string Time { get; set; }
    }
}
