using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using NUnit.Framework;
using System.Linq;

namespace Treesor.Application.Test
{
    [TestFixture]
    public class TreesorServiceValuesTest
    {
        private MutableHierarchy<string, object> hierarchy;
        private TreesorService service;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.hierarchy = new MutableHierarchy<string, object>();
            this.service = new TreesorService(this.hierarchy);
        }

        [Test]
        public void Set_a_value_in_the_hierarchy()
        {
            // ACT

            this.service.SetValue(HierarchyPath.Create("a"), "test");
        }

        [Test]
        public void Read_a_value_from_hierarchy()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), "test");

            // ACT

            object value;
            bool result = this.service.TryGetValue(HierarchyPath.Create("a"), out value);

            // ASSERT

            Assert.IsTrue(result);
            Assert.AreEqual("test", (string)value);
        }

        [Test]
        public void Delete_a_value_from_hierarchy()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), "test");

            // ACT

            this.service.RemoveValue(HierarchyPath.Create("a"));

            // ASSERT

            object value;
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
        }

        [Test]
        public void Retrieve_the_descandants_of_the_root_node_breadth_first()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), "test1");
            this.service.SetValue(HierarchyPath.Create("a", "b"), "test2");
            this.service.SetValue(HierarchyPath.Create("b"), "test3");

            // ACT

            var result = this.service.DescendantsOrSelf(HierarchyPath.Create<string>(), 2).ToArray();

            // ASSERT

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(HierarchyPath.Create<string>(), result.ElementAt(0).Key);
            Assert.IsNull(result.ElementAt(0).Value);
            Assert.AreEqual(HierarchyPath.Create("a"), result.ElementAt(1).Key);
            Assert.AreEqual("test1", result.ElementAt(1).Value);
            Assert.AreEqual(HierarchyPath.Create("b"), result.ElementAt(2).Key);
            Assert.AreEqual("test3", result.ElementAt(2).Value);
            //Assert.AreEqual(HierarchyPath.Create("a", "b"), result.ElementAt(3).Key);
            //Assert.AreEqual("test2", result.ElementAt(3).Value);
        }
    }
}