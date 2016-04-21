using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Flurl.Http.Testing;
using Moq;
using NUnit.Framework;
using System.Linq;

namespace Treesor.PowershellDriveProvider.Test
{
    [TestFixture]
    public class TreesorServiceTest
    {
        private TreesorService treesorService;
        private Mock<IHierarchy<string, object>> remoteHierarchy;
        
        [SetUp]
        public void ArrangeAllTests()
        {
            this.remoteHierarchy = new Mock<IHierarchy<string, object>>();
            this.treesorService = new TreesorService(this.remoteHierarchy.Object);
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

        #region TryGetContainer

        [Test]
        public void TryGet_value_from_hierarchy_root()
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
        public void TryGet_value_from_hierarchy_node()
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

        #endregion TryGetContainer

        #region GetContainer/GetContainerChildren

        [Test]
        public void Get_value_from_hierarchy_root()
        {
            // ARRANGE

            object remoteValue;

            this.remoteHierarchy
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out remoteValue))
                .Returns(true);

            // ACT

            var result = this.treesorService.GetContainer(TreesorNodePath.Create());

            // ASSERT

            Assert.IsNotNull(result);
            Assert.IsNull(result.Name);

            this.remoteHierarchy.Verify(h => h.TryGetValue(HierarchyPath.Create<string>(), out remoteValue), Times.Once);
        }

        [Test]
        public void Get_value_from_hierarchy_node()
        {
            // ARRANGE

            object remoteValue;

            this.remoteHierarchy
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out remoteValue))
                .Returns(true);

            // ACT

            var result = this.treesorService.GetContainer(TreesorNodePath.Create("a"));

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("a", result.Name);

            this.remoteHierarchy.Verify(h => h.TryGetValue(HierarchyPath.Create("a"), out remoteValue), Times.Once);
        }

        [Test]
        public void Get_children_of_root_container()
        {
            // ARRANGE

            var fakeHierarchy = new MutableHierarchy<string, object>();
            fakeHierarchy.Add(HierarchyPath.Create("a"), "v1");
            fakeHierarchy.Add(HierarchyPath.Create("b"), "v2");
            fakeHierarchy.Add(HierarchyPath.Create("a","b"), "v2");
            
            this.remoteHierarchy
                .Setup(h => h.Traverse(HierarchyPath.Create<string>()))
                .Returns(fakeHierarchy.Traverse(HierarchyPath.Create<string>()));
            
            // ACT

            var result = this.treesorService.GetContainerChildren(TreesorNodePath.Create()).ToArray();

            // ASSERT

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("a", result.ElementAt(0).Name);
            Assert.AreEqual("b", result.ElementAt(1).Name);
        }

        #endregion GetContainer

        [Test]
        public void CreateContainer_without_value_under_root()
        {
            // ACT

            var result = this.treesorService.CreateContainer(TreesorNodePath.Create("a"), null);

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("a", result.Name);
            this.remoteHierarchy.VerifySet(h => h[HierarchyPath.Create("a")] = null);
        }
    }
}