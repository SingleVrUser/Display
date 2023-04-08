using System.Collections.Generic;
using System.Linq;
using Display.Data;

namespace Display.Helper;

public class MatchHelper
{
    public static string Int32Subtract(int int1, int int2)
    {
        return (int1 - int2).ToString();
    }

    public static string GetSameFirstStringFromList(List<string> list)
    {
        var sameContent = string.Empty;

        if (list == null || list.Count <1) return sameContent;

        var firstStr = list.First();
        if (list.Count == 1) return firstStr;

        var otherContent = list.Skip(1).ToList();

        var isSame = true;
        HashSet<string> matchList = new();
        foreach (var c in firstStr)
        {
            if (!isSame)
            {
                sameContent = string.Empty;
            }
            var tmp = sameContent + c;

            isSame = otherContent.All(j => j.Contains(tmp));

            if (isSame)
            {
                sameContent = tmp;
            }
            else
            {
                matchList.Add(sameContent);
            }
        }

        sameContent = matchList.MaxBy(x => x.Length);
        return sameContent;
    }
}
