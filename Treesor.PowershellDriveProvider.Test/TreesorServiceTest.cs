using Elementary.Hierarchy;
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
        private Mock<IHierarchy<string, object>> remoteHierarchy;
        private HttpTest httpTest;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.httpTest = new HttpTest();
            this.remoteHierarchy= new Mock<IHierarchy<string, object>>();
            this.treesorService = new TreesorService(this.remoteHierarchy.Object);
        }

        [TearDown]
        public void CleanupAllTests()
        {
            this.httpTest.Dispose();
        }

        [Test]
        public void Set_value_at_hierarchy_root()
        {
            // ACT

            var result = this.treesorService.SetValue(TreesorNodePath.Create(), "value");

            // ASSERT

            this.remoteHierarchy.VerifySet(h => h[HierarchyPath.Create<string>()] = "value", Times.Once);

            Assert.IsNotNull(result);
            Assert.IsNull(result.Name);
        }

        [Test]
        public void Set_value_at_hierarchy_node()
        {
            // ACT

            var result = this.treesorService.SetValue(TreesorNodePath.Create("a"), "value");

            // ASSERT

            this.remoteHierarchy.VerifySet(h => h[HierarchyPath.Create("a")] = "value", Times.Once);
            
            Assert.IsNotNull(result);
            Assert.AreEqual("a", result.Name);
        }

        [Test]
        public void Get_value_at_hierarchy_root()
        {
            // ARRANGE

            object remoteValue;

            this.remoteHierarchy
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out remoteValue))
                .Returns(true);

            // ACT

            TreesorContainerNode resultNode;
            var result = this.treesorService.TryGetContainer(TreesorNodePath.Create(), out resultNode);

            // ASSERT

            Assert.IsTrue(result);
            Assert.IsNull(resultNode.Name);

            this.remoteHierarchy.Verify(h => h.TryGetValue(HierarchyPath.Create<string>(), out remoteValue), Times.Once);
        }

        [Test]
        public void Get_value_at_hierarchy_node()
        {
            // ARRANGE

            object remoteValue;

            this.remoteHierarchy
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out remoteValue))
                .Returns(true);

            // ACT

            TreesorContainerNode resultNode;
            var result = this.treesorService.TryGetContainer(TreesorNodePath.Create("a"), out resultNode);

            // ASSERT
            
            Assert.IsTrue(result);
            Assert.AreEqual("a", resultNode.Name);

            this.remoteHierarchy.Verify(h => h.TryGetValue(HierarchyPath.Create("a"), out remoteValue), Times.Once);
        }
    }
}