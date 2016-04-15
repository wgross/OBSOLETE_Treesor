using Flurl.Http.Testing;
using NUnit.Framework;

namespace Treesor.Client.Test
{
    [TestFixture]
    public class TreesorClientNodesTest
    {
        private HttpTest httpTest;
        private RemoteHierarchy remoteHierarchy;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.httpTest = new HttpTest();
            this.remoteHierarchy = new RemoteHierarchy("http://localhost:9002/api");
        }

        [TearDown]
        public void CleanupAllTests()
        {
            this.httpTest.Dispose();
        }

        [Test]
        public void Get_child_nodes_of_root()
        {
            // ACT

            // var result = this.remoteHierarchy.Children();

            // ASSERT

            Assert.Fail("Implementation is still missing");
        }
    }
}