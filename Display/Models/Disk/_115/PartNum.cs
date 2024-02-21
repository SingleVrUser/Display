namespace Display.Models.Disk._115
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
