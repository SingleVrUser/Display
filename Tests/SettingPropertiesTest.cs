using Display.Models.Data;

namespace Tests
{
    //[TestClass]
    public class SettingPropertiesTest
    {
        // 无法在Test中获取AppSettings
        public void GetCookieTest()
        {
            var cookie = AppSettings._115_Cookie;
            Assert.IsNotNull(cookie);
        }
    }
}
