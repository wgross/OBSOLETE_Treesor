using Moq;
using NUnit.Framework;
using System.Web.Http.Results;
using Treesor.Application;
using Treesor.Service.Endpoints;
using Elementary.Hierarchy;

namespace Treesor.Test
{
    [TestFixture]
    public class TreesorControllerValuesTest
    {
        private HierarchyController controller;
        private Mock<ITreesorService> service;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.service = new Mock<ITreesorService>();
            this.controller = new HierarchyController(this.service.Object);
        }

        [Test]
        public void Write_a_string_to_hierachy_root()
        {
            // ARRANGE

            object value = new { test = "text" };
            
            // ACT

            var result = this.controller.Post("", new HierarchyNodeRequestBody { Value = value }) as CreatedAtRouteNegotiatedContentResult<HierarchyNodeBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("", result.Content.Path);
            Assert.AreSame(value, result.Content.Value);
            Assert.AreEqual("", result.RouteValues["path"]);

            this.service.Verify(s => s.SetValue(HierarchyPath.Create<string>(), value), Times.Once);
        }

        [Test]
        public void Write_a_string_to_a_subnode()
        {
            object value = new { test = "text" };

            // ACT

            var result = this.controller.Post("a/b", new HierarchyNodeRequestBody { Value = value }) as CreatedAtRouteNegotiatedContentResult<HierarchyNodeBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("a/b", result.Content.Path);
            Assert.AreSame(value, result.Content.Value);
            Assert.AreEqual("a/b", result.RouteValues["path"]);

            this.service.Verify(s => s.SetValue(HierarchyPath.Create("a","b"), value), Times.Once);
        }

        [Test]
        public void Read_a_value_from_a_subnode()
        {
            // ARRANGE
            
            object value = new { test = "text" };

            this.service.Setup(s => s.TryGetValue(HierarchyPath.Create("a", "b"), out value)).Returns(true);
            
            // ACT

            var result = this.controller.Get("a/b") as OkNegotiatedContentResult<HierarchyNodeBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("a/b", result.Content.Path);
            Assert.AreSame(value, result.Content.Value);

            this.service.Verify(s => s.TryGetValue(HierarchyPath.Create("a", "b"), out value), Times.Once);
        }
        
        [Test]
        public void Read_a_missing_value_from_a_subnode_fails_with_404()
        {
            // ARRANGE

            object value = null;

            this.service.Setup(s => s.TryGetValue(HierarchyPath.Create("a", "b"), out value)).Returns(false);

            // ACT

            var result = this.controller.Get("a/b") as NotFoundResult;

            // ASSERT

            Assert.IsNotNull(result);
            
            this.service.Verify(s => s.TryGetValue(HierarchyPath.Create("a", "b"), out value), Times.Once);
        }
    }
}