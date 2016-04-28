using Elementary.Hierarchy;
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
        private HierarchyValueController controller;
        private Mock<ITreesorService> service;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.service = new Mock<ITreesorService>();
            this.controller = new HierarchyValueController(this.service.Object);
        }

        #region POST /api/values/{path}, POST /api/values

        [Test]
        public void Write_value_to_hierarchy_with_empty_path_fails()
        {
            // ARRANGE

            object value = new { test = "text" };

            // ACT

            var result = this.controller.Post("", new HierarchyValueRequestBody { value = value }) as ExceptionResult;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Exception.Message.Contains("may not be null or empty"));

            this.service.Verify(s => s.SetValue(It.IsAny<HierarchyPath<string>>(), It.IsAny<TreesorNodeValueBase>()), Times.Never);
        }

        [Test]
        public void Write_value_to_root_fails()
        {
            // ARRANGE

            object value = "text";

            // ACT

            var result = this.controller.Post(new HierarchyValueRequestBody { value = value }) as ExceptionResult;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Exception.Message.Contains("Root may not have a value"));

            this.service.Verify(s => s.SetValue(HierarchyPath.Create<string>(), new TreesorNodeValue(value)), Times.Never);
            this.service.VerifyAll();
        }

        [Test]
        public void Write_value_to_a_subnode()
        {
            object value = new { test = "text" };

            // ACT

            var result = this.controller.Post("a/b", new HierarchyValueRequestBody { value = value }) as CreatedAtRouteNegotiatedContentResult<HierarchyValueBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("a/b", result.Content.path);
            Assert.AreSame(value, result.Content.value);
            Assert.AreEqual("a/b", result.RouteValues["path"]);

            this.service.Verify(s => s.SetValue(HierarchyPath.Create("a", "b"), new TreesorNodeValue(value)), Times.Once);
        }

        #endregion POST /api/values/{path}, POST /api/values

        #region GET /api/values/{path}, GET /api/values

        [Test]
        public void Read_value_from_a_subnode()
        {
            // ARRANGE

            TreesorNodeValueBase value = new TreesorNodeValue("text");

            this.service.Setup(s => s.TryGetValue(HierarchyPath.Create("a", "b"), out value)).Returns(true);

            // ACT

            var result = this.controller.Get("a/b") as OkNegotiatedContentResult<HierarchyValueBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("a/b", result.Content.path);
            Assert.AreSame(value, result.Content.value);

            this.service.Verify(s => s.TryGetValue(HierarchyPath.Create("a", "b"), out value), Times.Once);
        }

        [Test]
        public void Read_missing_value_from_a_subnode_fails_with_404()
        {
            // ARRANGE

            TreesorNodeValueBase value = new TreesorNodeValue(null);

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

            this.service.Verify(s => s.SetValue(It.IsAny<HierarchyPath<string>>(), It.IsAny<TreesorNodeValueBase>()), Times.Never);
        }

        [Test]
        public void Read_value_from_root_node_fails_with_InvalidOperationException()
        {
            // ACT

            var result = this.controller.Get() as ExceptionResult;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Exception.Message.Contains("Root may not have a value"));

            TreesorNodeValueBase value = null;
            this.service.Verify(s => s.TryGetValue(It.IsAny<HierarchyPath<string>>(), out value), Times.Never);
            this.service.VerifyAll();
        }

        #endregion GET /api/values/{path}, GET /api/values

        #region DELETE /api/v1/values/{path}?$expand, DELETE /api/v1/values?$expand

        [Test]
        public void Delete_value_from_root_node_fails_with_InvalidOperationException()
        {
            // ACT

            var result = this.controller.Delete((int?)null) as ExceptionResult;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Exception.Message.Contains("Root may not have a value"));

            this.service.Verify(s => s.RemoveValue(HierarchyPath.Create<string>(), 1), Times.Never);
            this.service.VerifyAll();
        }

        [Test]
        public void Delete_value_from_a_sub_node()
        {
            // ACT

            var result = this.controller.Delete("a/b") as OkResult;

            // ASSERT

            Assert.IsNotNull(result);

            this.service.Verify(s => s.RemoveValue(HierarchyPath.Create("a", "b"), 1), Times.Once);
        }

        [Test]
        public void Delete_value_from_root_node_and_child_node()
        {
            // ACT

            var result = this.controller.Delete(2) as OkResult;

            // ASSERT

            Assert.IsNotNull(result);

            this.service.Verify(s => s.RemoveValue(HierarchyPath.Create<string>(), 2), Times.Once);
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

        #endregion DELETE /api/v1/values/{path}?$expand, DELETE /api/v1/values?$expand

        #region PUT /api/v1/values/{*path}, PUT /api/v1/values

        [Test]
        public void Update_value_of_a_sub_node()
        {
            // ARRANGE

            object value = new { test = "text" };

            // ACT

            var result = this.controller.Put("a/b", new HierarchyValueRequestBody { value = value }) as OkNegotiatedContentResult<HierarchyValueBody>;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreSame(value, result.Content.value);
            Assert.AreEqual("a/b", result.Content.path);

            this.service.Verify(s => s.SetValue(HierarchyPath.Create("a", "b"),new TreesorNodeValue(value)), Times.Once);
            this.service.VerifyAll();
        }

        [Test]
        public void Update_value_with_empty_path_fails_with_500()
        {
            // ARRANGE

            object value = "text";

            // ACT

            var result = this.controller.Put("", new HierarchyValueRequestBody { value = value }) as ExceptionResult;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Exception.Message.Contains("may not be null or empty"));

            this.service.Verify(s => s.SetValue(It.IsAny<HierarchyPath<string>>(), It.IsAny<TreesorNodeValueBase>()), Times.Never);
            this.service.VerifyAll();
        }

        [Test]
        public void Update_value_of_root_fails_with_InvalidOperationException()
        {
            // ARRANGE

            object value = "text";

            // ACT

            var result = this.controller.Put(new HierarchyValueRequestBody { value = value }) as ExceptionResult;

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Exception.Message.Contains("Root may not have a value"));

            this.service.Verify(s => s.SetValue(HierarchyPath.Create<string>(), new TreesorNodeValue(value)), Times.Never);
            this.service.VerifyAll();
        }

        #endregion PUT /api/v1/values/{*path}, PUT /api/v1/values
    }
}