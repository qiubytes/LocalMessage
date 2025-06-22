using MulticastLocalMessage;

namespace NUnitTest;

public class UnitTestUtils
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void GetPrimaryIPv4Address()
    {
        var q = Utils.GetPrimaryIPv4Address();
        Assert.Pass();
    }
}
