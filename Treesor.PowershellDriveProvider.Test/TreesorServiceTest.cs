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
        private HttpTest httpTest = new HttpTest();

        [SetUp]
        public void ArrangeAllTests()
        {
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

            var result = this.treesorService.SetValue(TreesorNodePath.Create(), "value").Result;

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api")
                .WithVerb(HttpMethod.Post)
                .WithContentType("application/json")
                .With(c => c.RequestBody.Contains("\"value\":\"value\""));
            
            Assert.IsNotNull(result);
            Assert.AreEqual(null, result.Name);
        }
    }
}