using Elementary.Hierarchy;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using Treesor.Application;
using Treesor.Service.Endpoints;

namespace Treesor.Test
{
    [TestFixture]
    public class TreesorControllerNodesTest
    {
        private HierarchyNodeController controller;
        private Mock<ITreesorService> service;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.service = new Mock<ITreesorService>();
            this.controller = new HierarchyNodeController(this.service.Object);
        }

        #region GetChildren

        [Test]
        public void Get_children_of_empty_root_node()
        {
            // ARRANGE

            this.service.Setup(s => s.DescendantsOrSelf(HierarchyPath.Create<string>(), 2)).Returns(Enumerable.Empty<KeyValuePair<HierarchyPath<string>, TreesorNodeValueBase>>());

            // ACT

            var result = this.controller.GetChildren() as OkNegotiatedContentResult<HierarchyNodeCollectionBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.IsNull(result.Content.path);
            Assert.IsNotNull(result.Content.nodes);
            Assert.IsFalse(result.Content.nodes.Any());
        }

        [Test]
        public void Get_children_of_root_node()
        {
            // ARRANGE

            Func<HierarchyPath<string>, TreesorNodeValueBase, KeyValuePair<HierarchyPath<string>, TreesorNodeValueBase>> kv = (k, v) => new KeyValuePair<HierarchyPath<string>, TreesorNodeValueBase>(k, v);

            var descandants = new[]
            {
                kv(HierarchyPath.Create<string>(), new TreesorNodeValue("root")),
                kv(HierarchyPath.Create("a"),new TreesorNodeValue("a")),
                kv(HierarchyPath.Create("b"), new TreesorNodeValue("b")),
            };

            this.service.Setup(s => s.DescendantsOrSelf(HierarchyPath.Create<string>(), 2)).Returns(descandants);

            // ACT

            var result = this.controller.GetChildren() as OkNegotiatedContentResult<HierarchyNodeCollectionBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.IsNull(result.Content.path);
            Assert.IsNotNull(result.Content.nodes);
            Assert.AreEqual(2, result.Content.nodes.Length);
            CollectionAssert.AreEqual(new[] { "a", "b" }, result.Content.nodes.Select(n => n.path).ToArray());
        }

        [Test]
        public void Get_children_of_inner_node()
        {
            // ARRANGE

            Func<HierarchyPath<string>, TreesorNodeValueBase, KeyValuePair<HierarchyPath<string>, TreesorNodeValueBase>> kv = (k, v) => new KeyValuePair<HierarchyPath<string>, TreesorNodeValueBase>(k, v);

            var descandants = new[]
            {
                kv(HierarchyPath.Create("a"), new TreesorNodeValue("a")),
                kv(HierarchyPath.Create("a","a"), new TreesorNodeValue("a/a")),
                kv(HierarchyPath.Create("a","b"), new TreesorNodeValue("a/b")),
            };

            this.service.Setup(s => s.DescendantsOrSelf(HierarchyPath.Create("a"), 2)).Returns(descandants);

            // ACT

            var result = (OkNegotiatedContentResult<HierarchyNodeCollectionBody>)this.controller.GetChildren("a");

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Content.path, "a");
            Assert.AreEqual(2, result.Content.nodes.Length);
            CollectionAssert.AreEqual(new[] { "a/a", "a/b" }, result.Content.nodes.Select(n => n.path).ToArray());
        }

        #endregion GetChildren
    }
}