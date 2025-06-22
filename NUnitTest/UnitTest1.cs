using MulticastLocalMessage.ServersClients;
using System.Threading.Tasks;

namespace NUnitTest
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }
        /// <summary>
        /// dotnet test --no-build
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task Test1()
        {
            FileSenderClient client = new FileSenderClient();
            try
            {
                await client.SendFile("127.0.0.1", 8082, "C:\\Users\\Lenovo\\Downloads\\������\\����B_S�ܹ��ĸ�Уѧ�����ʹ���ϵͳ���о���ʵ��.pdf");
                Assert.Pass();

            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            //
        }
    }
}