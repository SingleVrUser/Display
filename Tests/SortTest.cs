using System.Text.RegularExpressions;

namespace Tests
{
    [TestClass]
    public class SortTest
    {
        private readonly Dictionary<string, string> _sampleFileDictionary = new()
        {
            { "hhd1080.com@1nhdtb00259hhb.wtbjj0e3", "1nhdtb00259hhb" },
            { "hotavxxx.com_092411-815-carib-whole_hd1", "092411-815" },
            { "hhd800.com@cawd00302hhb", "cawd00302" },
            { "hhd800.com@cjod00388hhb", "cjod00388" },
            { "URE-031-C", "URE-031" },
            { "ciel-001", "ciel-001" }
        };

        [TestMethod]
        public void MatchNameTest()
        {
            //hhd1080.com@1nhdtb00259hhb.wtbjj0e3
            const string matchRegex = "((\\d?[a-z]+-?\\d+)([_-]([2468]ks?([36]0fps)?)|hhb|.part)?(\\d?)(_8k)?)(\\.\\w{3,9})?$";

            foreach (var (src,dst) in _sampleFileDictionary)
            {
                var match = Regex.Match(src, matchRegex, RegexOptions.IgnoreCase);

                string? destinationName = null;
                if (match.Success)
                {
                    destinationName = match.Groups[1].Value;
                }

                Assert.IsNotNull(destinationName, "未匹配：" + src);
                Assert.AreEqual(destinationName, dst,$"匹配出错：{src}");
            }
        }
    }
}
