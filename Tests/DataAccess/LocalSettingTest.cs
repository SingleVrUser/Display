using System.Diagnostics;
using Display.Setting;
using Display.Setting.Impl;
using Display.Setting.Interfaces;

namespace Tests.DataAccess;

[TestClass]
public class LocalSettingTest
{
    [ClassInitialize]
    public static void Initialize(TestContext context)
    {
        Debug.WriteLine(context);
    }

    [TestMethod]
    public void SettingTest()
    {
    }
}