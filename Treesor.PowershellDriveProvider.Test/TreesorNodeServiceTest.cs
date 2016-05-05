using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;

namespace Treesor.PowershellDriveProvider.Test
{
    [TestFixture]
    public class TreesorNodeServiceTest
    {
        private TreesorNodeService treesorService;
        private Mock<IHierarchy<string, object>> remoteHierarchy;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.remoteHierarchy = new Mock<IHierarchy<string, object>>();
            this.treesorService = new TreesorNodeService(this.remoteHierarchy.Object);
        }

        #region SetValue/RemoveValue/TryGetValue("/")

        [Test]
        public void TreesorNodeService_SetValue_root_fails_with_InvalidOperationException()
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.SetValue(TreesorNodePath.Create(), "value"));

            // ASSERT

            Assert.That(result.Message.Contains("Root may not have a value"));

            this.remoteHierarchy.VerifyAll();
        }

        [Test]
        public void TreesorNodeService_RemoveValue_root_fails_with_InvalidOperationException()
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.RemoveValue(TreesorNodePath.Create()));

            // ASSERT

            Assert.That(result.Message.Contains("Root may not have a value"));

            this.remoteHierarchy.VerifyAll();
        }

        [Test]
        public void TreesorNodeService_RemoveValue_get_root_returns_TreesorContainerItem()
        {
            // ARRANGE
            object rootValue = null;

            this.remoteHierarchy
                .Setup(h => h.TryGetValue(HierarchyPath.Create<string>(), out rootValue))
                .Returns(true);

            // ACT

            TreesorNode resultNode;
            var result = this.treesorService.TryGetNode(TreesorNodePath.RootPath, out resultNode);

            // ASSERT

            Assert.IsTrue(result);
            Assert.AreEqual(TreesorNodePath.RootPath, resultNode.Path);
            Assert.IsInstanceOf<TreesorContainerItem>(resultNode);

            this.remoteHierarchy.Verify(h => h.TryGetValue(HierarchyPath.Create<string>(), out rootValue), Times.Once);
            this.remoteHierarchy.VerifyAll();
        }

        #endregion SetValue/RemoveValue/TryGetValue("/")

        #region Set-Item -Value -> SetValue

        [Test]
        public void TreesorNodeService_SetValue_inner_node_creates_new_ValueItem()
        {
            // ARRANGE

            object a = new TreesorContainerItem();

            this.remoteHierarchy.Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out a)).Returns(true);

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.treesorService.SetValue(TreesorNodePath.Create("a"), "value"));

            // ASSERT

            this.remoteHierarchy.Verify(h => h.TryGetValue(HierarchyPath.Create("a"), out a), Times.Once);
            this.remoteHierarchy.VerifyAll();
        }

        #endregion Set-Item -Value -> SetValue

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

            TreesorNode resultNode;
            var result = this.treesorService.TryGetNode(TreesorNodePath.Create(), out resultNode);

            // ASSERT

            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, resultNode.Name);
            Assert.AreEqual(TreesorNodePath.RootPath, resultNode.Path);

            this.remoteHierarchy.Verify(h => h.TryGetValue(HierarchyPath.Create<string>(), out remoteValue), Times.Once);
            this.remoteHierarchy.VerifyAll();
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

            TreesorNode resultNode;
            var result = this.treesorService.TryGetNode(TreesorNodePath.Create("a"), out resultNode);

            // ASSERT

            Assert.IsTrue(result);
            Assert.AreEqual("a", resultNode.Name);
            Assert.AreEqual(TreesorNodePath.Create("a"), resultNode.Path);

            this.remoteHierarchy.Verify(h => h.TryGetValue(HierarchyPath.Create("a"), out remoteValue), Times.Once);
        }

        [Test]
        public void TryGet_value_from_hierarchy_node_returns_false_if_path_is_missing()
        {
            // ARRANGE

            object remoteValue = null;

            this.remoteHierarchy
                .Setup(h => h.TryGetValue(HierarchyPath.Create("a"), out remoteValue))
                .Returns(false);

            // ACT

            TreesorNode resultNode;
            var result = this.treesorService.TryGetNode(TreesorNodePath.Create("a"), out resultNode);

            // ASSERT

            Assert.IsFalse(result);
            Assert.IsNull(resultNode);

            this.remoteHierarchy.Verify(h => h.TryGetValue(HierarchyPath.Create("a"), out remoteValue), Times.Once);
            this.remoteHierarchy.VerifyAll();
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

            var result = this.treesorService.GetNode(TreesorNodePath.Create());

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.Name);
            Assert.AreEqual(TreesorNodePath.RootPath, result.Path);

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

            var result = this.treesorService.GetNode(TreesorNodePath.Create("a"));

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("a", result.Name);
            Assert.AreEqual(TreesorNodePath.Create("a"), result.Path);

            this.remoteHierarchy.Verify(h => h.TryGetValue(HierarchyPath.Create("a"), out remoteValue), Times.Once);
        }

        [Test]
        public void Get_children_of_root_container()
        {
            // ARRANGE

            var fakeHierarchy = new MutableHierarchy<string, object>();
            fakeHierarchy.Add(HierarchyPath.Create("a"), "v1");
            fakeHierarchy.Add(HierarchyPath.Create("b"), "v2");
            fakeHierarchy.Add(HierarchyPath.Create("a", "b"), "v2");

            this.remoteHierarchy
                .Setup(h => h.Traverse(HierarchyPath.Create<string>()))
                .Returns(fakeHierarchy.Traverse(HierarchyPath.Create<string>()));

            // ACT

            var result = this.treesorService.GetContainerChildren(TreesorNodePath.Create()).ToArray();

            // ASSERT

            Assert.AreEqual(2, result.Length);
            Assert.AreEqual("a", result.ElementAt(0).Name);
            Assert.AreEqual(TreesorNodePath.Create("a"), result.ElementAt(0).Path);
            Assert.AreEqual("b", result.ElementAt(1).Name);
            Assert.AreEqual(TreesorNodePath.Create("b"), result.ElementAt(1).Path);
        }

        #endregion GetContainer/GetContainerChildren

        #region HasChildNodes

        [Test]
        public void HasChildNodes_checks_if_any_children_are_there_TRUE()
        {
            // ARRANGE

            var fakeHierarchy = new MutableHierarchy<string, object>();
            fakeHierarchy.Add(HierarchyPath.Create("a"), "v1");
            fakeHierarchy.Add(HierarchyPath.Create("b"), "v2");
            fakeHierarchy.Add(HierarchyPath.Create("a", "b"), "v2");

            this.remoteHierarchy
                .Setup(h => h.Traverse(HierarchyPath.Create("a")))
                .Returns(fakeHierarchy.Traverse(HierarchyPath.Create("a")));

            // ACT

            var result = this.treesorService.HasChildNodes(TreesorNodePath.Create("a"));

            // ASSERT

            Assert.IsTrue(result);

            this.remoteHierarchy.Verify(h => h.Traverse(HierarchyPath.Create("a")));
            this.remoteHierarchy.VerifyAll();
        }

        [Test]
        public void HasChildNodes_checks_if_any_children_are_there_FALSE()
        {
            // ARRANGE

            var fakeHierarchy = new MutableHierarchy<string, object>();
            fakeHierarchy.Add(HierarchyPath.Create("a"), "v1");

            this.remoteHierarchy
                .Setup(h => h.Traverse(HierarchyPath.Create("a")))
                .Returns(fakeHierarchy.Traverse(HierarchyPath.Create("a")));

            // ACT

            var result = this.treesorService.HasChildNodes(TreesorNodePath.Create("a"));

            // ASSERT

            Assert.IsFalse(result);

            this.remoteHierarchy.Verify(h => h.Traverse(HierarchyPath.Create("a")));
            this.remoteHierarchy.VerifyAll();
        }

        #endregion HasChildNodes

        [Test]
        public void CreateContainer_without_value_under_root()
        {
            // ACT

            var result = this.treesorService.CreateContainer(TreesorNodePath.Create("a"));

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("a", result.Name);
            Assert.AreEqual(TreesorNodePath.Create("a"), result.Path);
            this.remoteHierarchy.VerifySet(h => h[HierarchyPath.Create("a")] = null);
        }

        [Test]
        public void RemoveContainer_non_recursive()
        {
            // ARRANGE

            this.remoteHierarchy
                .Setup(h => h.Remove(HierarchyPath.Create("a"), 1))
                .Returns(true);

            // ACT

            var result = this.treesorService.RemoveContainer(TreesorNodePath.Create("a"), false);

            // ASSERT

            Assert.IsTrue(result);

            this.remoteHierarchy.Verify(h => h.Remove(HierarchyPath.Create("a"), 1), Times.Once);
        }

        [Test]
        public void RemoveContainer_recursive()
        {
            // ARRANGE

            this.remoteHierarchy
                .Setup(h => h.Remove(HierarchyPath.Create("a"), int.MaxValue))
                .Returns(true);

            // ACT

            var result = this.treesorService.RemoveContainer(TreesorNodePath.Create("a"), true);

            // ASSERT

            Assert.IsTrue(result);

            this.remoteHierarchy.Verify(h => h.Remove(HierarchyPath.Create("a"), int.MaxValue), Times.Once);
        }
    }
}