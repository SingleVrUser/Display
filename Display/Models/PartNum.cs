using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Display.Models
{
    public class PartNumCombo
    {
        public PartNum PartNum;

        public string Description;

        public PartNumCombo(PartNum partNum, string description)
        {
            PartNum = partNum;
            Description = description;
        }

        public override string ToString()
        {
            return Description;
        }
    }


    public enum PartNum
    {
        Chinese,
        Arabic,
        Roman,
        English,
        CapsEnglish,
    }
}
