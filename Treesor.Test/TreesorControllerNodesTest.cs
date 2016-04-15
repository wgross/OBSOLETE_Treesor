using Moq;
using NUnit.Framework;
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
            this.controller = new HierarchyNodeController();
        }

        [Test]
        public void Get_children_of_root_node()
        {
            // ACT

            var result = this.controller.Get() as OkNegotiatedContentResult<object>;

            // ASSERT

            Assert.IsNotNull(result);
        }
    }
}