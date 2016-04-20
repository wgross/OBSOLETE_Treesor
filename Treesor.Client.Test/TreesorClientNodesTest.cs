using Elementary.Hierarchy;
using Flurl.Http.Testing;
using NUnit.Framework;
using System.Linq;
using System.Net.Http;
using Treesor.Service.Endpoints;

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
        public void Get_descendants_nodes_of_root()
        {
            // ARRANGE
            
            this.httpTest.RespondWithJson(new HierarchyNodeCollectionBody
            {
                nodes = new[]
                {
                    new HierarchyNodeBody
                    {
                        path = null
                    },
                    new HierarchyNodeBody
                    {
                        path = "a"
                    },
                    new HierarchyNodeBody
                    {
                        path = "b"
                    }
                }
            });

            // ACT

            var result = this.remoteHierarchy.Traverse(HierarchyPath.Create<string>());

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/nodes/root/children")
                .WithVerb(HttpMethod.Get);

            Assert.AreEqual(HierarchyPath.Create<string>(), result.Path);
            Assert.AreEqual(HierarchyPath.Create("a"), result.Children().ElementAt(0).Path);
            Assert.AreEqual(HierarchyPath.Create("b"), result.Children().ElementAt(1).Path);
        }

        [Test]
        public void Get_descendants_nodes_of_inner_node()
        {
            // ARRANGE

            this.httpTest.RespondWithJson(new HierarchyNodeCollectionBody
            {
                nodes = new[]
                {
                    new HierarchyNodeBody
                    {
                        path = "a"
                    },
                    new HierarchyNodeBody
                    {
                        path = "a/a"
                    },
                    new HierarchyNodeBody
                    {
                        path = "a/b"
                    }
                }
            });

            // ACT

            var result = this.remoteHierarchy.Traverse(HierarchyPath.Create("a"));

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/nodes/a/children")
                .WithVerb(HttpMethod.Get);

            Assert.AreEqual(HierarchyPath.Create("a"), result.Path);
            Assert.AreEqual(HierarchyPath.Create("a","a"), result.Children().ElementAt(0).Path);
            Assert.AreEqual(HierarchyPath.Create("a","b"), result.Children().ElementAt(1).Path);
        }
    }
}