using Moq;
using NUnit.Framework;
using System.Web.Http.Results;
using Treesor.Application;
using Treesor.Service.Endpoints;
using Elementary.Hierarchy;

namespace Treesor.Test
{
    [TestFixture]
    public class WriteAndReadValues
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
    }
}