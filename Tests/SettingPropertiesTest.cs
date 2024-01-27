using Display.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
