using Elementary.Hierarchy.Collections;
using Flurl.Http.Testing;
using Moq;
using NUnit.Framework;
using System.Net.Http;
using Treesor.Service.Endpoints;

namespace Treesor.PowershellDriveProvider.Test
{
    [TestFixture]
    public class TreesorServiceTest
    {
        private TreesorService treesorService;
        private Mock<IHierarchy<string, TreesorContainerNode>> hierarchyModel;
        private HttpTest httpTest;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.httpTest = new HttpTest();
            this.hierarchyModel = new Mock<IHierarchy<string, TreesorContainerNode>>();
            this.treesorService = new TreesorService(this.hierarchyModel.Object, "name");
        }

        [TearDown]
        public void CleanupAllTests()
        {
            this.httpTest.Dispose();
        }

        [Test]
        public void Set_value_at_root_node()
        {
            // ARRANGE

            this.httpTest.RespondWithJson(new HierarchyNodeBody
            {
                path = null,
                value = "value"
            });

            // ACT

            var result = this.treesorService.SetValue(TreesorNodePath.Create(), "value");

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api")
                .WithVerb(HttpMethod.Put)
                .WithContentType("application/json")
                .With(c => c.RequestBody.Contains("\"value\":\"value\""));
            
            Assert.IsNotNull(result);
            Assert.AreEqual(null, result.Name);
        }

        [Test]
        public void Get_value_at_root_node()
        {
            // ARRANGE

            this.httpTest.RespondWithJson(new HierarchyNodeBody
            {
                path = null,
                value = "value"
            });

            // ACT

            TreesorContainerNode resultNode;
            var result = this.treesorService.TryGetContainer(TreesorNodePath.Create(), out resultNode);

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api")
                .WithVerb(HttpMethod.Get);

            Assert.IsTrue(result);
            Assert.AreEqual(null, resultNode.Name);
        }
    }
}