using ElectrumXClient;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;

namespace Tests
{
    public class UnitTestClient
    {
        [SetUp]
        public void Setup()
        {            
        }

        [Test]
        public void Test_CanCreateClient()
        {
            var config = new ConfigurationBuilder()
                 .AddJsonFile("client-secrets.json")
                 .Build();
            var host = config["ElectrumXHost"];
            var port = 50001;
            var useSSL = false;

            using (Client client = new Client(host, port, useSSL)) { }
        }
    }
}