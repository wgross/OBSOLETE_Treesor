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
        public void Get_descendants_at_level1_from_root()
        {
            // ARRANGE

            this.httpTest.RespondWithJson(new HierarchyNodeCollectionBody
            {
                path = null,
                nodes = new[]
                {
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
            // start traversal at root level and get the roots children

            var result = this.remoteHierarchy.Traverse(HierarchyPath.Create<string>()).Children().ToArray();

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/nodes/root/children")
                .WithVerb(HttpMethod.Get);

            Assert.AreEqual(HierarchyPath.Create("a"), result[0].Path);
            Assert.AreEqual(HierarchyPath.Create("b"), result[1].Path);
        }

        [Test]
        public void Get_descendants_of_level1_from_inner_node()
        {
            // ARRANGE

            this.httpTest.RespondWithJson(new HierarchyNodeCollectionBody
            {
                path = "a",
                nodes = new[]
                {
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
            // start traversal at path /a

            var result = this.remoteHierarchy.Traverse(HierarchyPath.Create("a")).Children().ToArray();

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/nodes/a/children")
                .WithVerb(HttpMethod.Get);

            Assert.AreEqual(HierarchyPath.Create("a", "a"), result[0].Path);
            Assert.AreEqual(HierarchyPath.Create("a", "b"), result[1].Path);
        }

        [Test]
        public void Get_descendants_of_level2_from_inner_node()
        {
            // ARRANGE

            this.httpTest.RespondWithJson(new HierarchyNodeCollectionBody
            {
                path = "a",
                nodes = new[]
                {
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

            var children = this.remoteHierarchy.Traverse(HierarchyPath.Create("a"));

            this.httpTest.RespondWithJson(new HierarchyNodeCollectionBody
            {
                path = "a/a",
                nodes = new[]
                {
                    new HierarchyNodeBody
                    {
                        path = "a/a/a"
                    },
                    new HierarchyNodeBody
                    {
                        path = "a/a/b"
                    }
                }
            });

            // ACT
            // after starting traversal at "/" and fetching the children i'm fetch the children of "/a"

            var result = children.Children().ToArray().ElementAt(0).Children().ToArray();

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/nodes/a/a/children")
                .WithVerb(HttpMethod.Get);

            Assert.AreEqual(HierarchyPath.Create("a", "a", "a"), result[0].Path);
            Assert.AreEqual(HierarchyPath.Create("a", "a", "b"), result[1].Path);
        }
    }
}