using LocalMessage;
using System.Net.Sockets;

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
        UdpClient udpClient = new UdpClient();
        udpClient.Connect(q, 5000);//不能是127.0.0.1
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes("aaa");
        udpClient.Send(bytes);
        Assert.Pass();
    }
}
