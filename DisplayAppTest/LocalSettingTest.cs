using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Tests.Models;

namespace Tests;

[TestClass]
public class LocalSettingTest
{
    [AssemblyInitialize]
    public static Task AssemblyInitialize(TestContext testContext)
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var settings = config.GetRequiredSection("Parent").Get<Parent>();

        Debug.WriteLine(settings);
        return Task.CompletedTask;
    }


    [TestMethod]
    public void SettingTest()
    {
        Debug.WriteLine("123");
    }
}