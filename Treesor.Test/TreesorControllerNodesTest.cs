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

        [Test]
        public void Get_children_of_empty_root_node()
        {
            // ARRANGE

            this.service.Setup(s => s.DescendantsOrSelf(2)).Returns(Enumerable.Empty<KeyValuePair<HierarchyPath<string>, object>>());

            // ACT

            var result = this.controller.Get() as OkNegotiatedContentResult<HierarchyNodeCollectionBody>;

            // ASSERT

            Assert.IsNotNull(result);
        }

        [Test]
        public void Get_descandants_of_root_node()
        {
            // ARRANGE

            Func<HierarchyPath<string>, string, KeyValuePair<HierarchyPath<string>, object>> kv = (k, v) => new KeyValuePair<HierarchyPath<string>, object>(k, v);

            var descandants = new[]
            {
                kv(HierarchyPath.Create<string>(),"root"),
                kv(HierarchyPath.Create("a"), "a"),
                kv(HierarchyPath.Create("b"), "b"),
                kv(HierarchyPath.Create("a","b"), "a/b")
            };

            this.service.Setup(s => s.DescendantsOrSelf(2)).Returns(descandants);

            // ACT

            var result = (OkNegotiatedContentResult<HierarchyNodeCollectionBody>)this.controller.Get();

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Content.nodes.Length);
            CollectionAssert.AreEqual(new[] { string.Empty, "a", "b", "a/b" }, result.Content.nodes.Select(n => n.path).ToArray());

        }
    }
}