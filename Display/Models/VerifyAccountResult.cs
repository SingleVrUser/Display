using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Display.Models
{
    public class VerifyAccountResult
    {
        public bool state { get; set; }
        public string error { get; set; }
        public int errno { get; set; }

    }
}
