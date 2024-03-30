using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using Tests.Models;

namespace Tests.DataAccess;

[TestClass]
public class LocalSettingTest
{
    [AssemblyInitialize]
    public static async Task AssemblyInitialize(TestContext testContext)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var settings = config.GetRequiredSection("Parent").Get<Parent>();

        Debug.WriteLine(settings);
    }


    [TestMethod]
    public void SettingTest()
    {
        Debug.WriteLine("123");
    }
}