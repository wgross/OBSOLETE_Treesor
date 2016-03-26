﻿using Elementary.Hierarchy;
using Moq;
using NUnit.Framework;
using System.Web.Http.Results;
using Treesor.Application;
using Treesor.Service.Endpoints;

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

        #region POST /api/{path}, POST /api

        [Test]
        public void Write_value_to_hierachy_with_empty_path_fails()
        {
            // ARRANGE

            object value = new { test = "text" };

            // ACT

            var result = this.controller.Post("", new HierarchyNodeRequestBody { Value = value }) as ExceptionResult;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Exception.Message.Contains("may not be null or empty"));

            this.service.Verify(s => s.SetValue(It.IsAny<HierarchyPath<string>>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void Write_new_value_to_root()
        {
            // ARRANGE

            object value = new { test = "text" };

            // ACT

            var result = this.controller.Post(new HierarchyNodeRequestBody { Value = value }) as CreatedAtRouteNegotiatedContentResult<HierarchyNodeBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("", result.Content.Path);
            Assert.AreSame(value, result.Content.Value);
            Assert.AreEqual("", result.RouteValues["path"]);

            this.service.Verify(s => s.SetValue(HierarchyPath.Create<string>(), value), Times.Once);
        }

        [Test]
        public void Write_value_to_a_subnode()
        {
            object value = new { test = "text" };

            // ACT

            var result = this.controller.Post("a/b", new HierarchyNodeRequestBody { Value = value }) as CreatedAtRouteNegotiatedContentResult<HierarchyNodeBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("a/b", result.Content.Path);
            Assert.AreSame(value, result.Content.Value);
            Assert.AreEqual("a/b", result.RouteValues["path"]);

            this.service.Verify(s => s.SetValue(HierarchyPath.Create("a", "b"), value), Times.Once);
        }

        #endregion POST /api/{path}, POST /api

        #region GET /api/{path}, GET /api

        [Test]
        public void Read_value_from_a_subnode()
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
        public void Read_missing_value_from_a_subnode_fails_with_404()
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

        [Test]
        public void Read_value_with_empty_path_fails_with_500()
        {
            // ACT

            var result = this.controller.Get("") as ExceptionResult;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Exception.Message.Contains("may not be null or empty"));

            this.service.Verify(s => s.SetValue(It.IsAny<HierarchyPath<string>>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void Read_value_from_root_node()
        {
            // ARRANGE

            object value = new { test = "text" };
            this.service.Setup(s => s.TryGetValue(HierarchyPath.Create<string>(), out value)).Returns(true);

            // ACT

            var result = this.controller.Get() as OkNegotiatedContentResult<HierarchyNodeBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result.Content.Path);
            Assert.AreSame(value, result.Content.Value);

            this.service.VerifyAll();
        }

        #endregion GET /api/{path}, GET /api

        #region DELETE /api/{path}, DELETE /api

        [Test]
        public void Delete_value_from_root_node()
        {
            // ACT

            var result = this.controller.Delete() as OkResult;

            // ASSERT

            Assert.IsNotNull(result);

            this.service.Verify(s => s.RemoveValue(HierarchyPath.Create<string>()), Times.Once);
        }

        [Test]
        public void Delete_value_from_a_sub_node()
        {
            // ACT

            var result = this.controller.Delete("a/b") as OkResult;

            // ASSERT

            Assert.IsNotNull(result);

            this.service.Verify(s => s.RemoveValue(HierarchyPath.Create("a", "b")), Times.Once);
        }

        [Test]
        public void Delete_value_with_empty_path_fails_with_500()
        {
            // ACT

            var result = this.controller.Delete("") as ExceptionResult;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Exception.Message.Contains("may not be null or empty"));
        }

        #endregion DELETE /api/{path}, DELETE /api

        #region PUT /api/{path}, PUT /api

        [Test]
        public void Update_value_of_a_sub_node()
        {
            // ARRANGE

            object value = new { test = "text" };

            // ACT

            var result = this.controller.Put("a/b", new HierarchyNodeRequestBody { Value = value }) as OkNegotiatedContentResult<HierarchyNodeBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreSame(value, result.Content.Value);
            Assert.AreEqual("a/b", result.Content.Path);

            this.service.Verify(s => s.SetValue(HierarchyPath.Create("a", "b"), value), Times.Once);
        }

        [Test]
        public void Update_value_with_empty_path_fails_with_500()
        {
            // ARRANGE

            object value = new { test = "text" };

            // ACT

            var result = this.controller.Put("", new HierarchyNodeRequestBody { Value = value }) as ExceptionResult;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Exception.Message.Contains("may not be null or empty"));

            this.service.Verify(s => s.SetValue(It.IsAny<HierarchyPath<string>>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public void Update_value_of_root()
        {
            // ARRANGE

            object value = new { test = "text" };

            // ACT

            var result = this.controller.Put(new HierarchyNodeRequestBody { Value = value }) as OkNegotiatedContentResult<HierarchyNodeBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreSame(value, result.Content.Value);
            Assert.AreEqual("", result.Content.Path);

            this.service.Verify(s => s.SetValue(HierarchyPath.Create<string>(), value), Times.Once);
        }

        #endregion PUT /api/{path}, PUT /api
    }
}