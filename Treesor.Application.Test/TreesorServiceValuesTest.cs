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

        #region RemoveValue

        [Test]
        public void Delete_a_value_from_hierarchy()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), "test");

            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"));

            // ASSERT

            object value;
            Assert.IsTrue(result);
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
        }

        [Test]
        public void Delete_a_value_which_is_missing_returns_false()
        {
            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"));

            // ASSERT

            object value;
            Assert.IsFalse(result);
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
        }

        [Test]
        public void Delete_value_from_hierarchy_recursive()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), "test");
            this.service.SetValue(HierarchyPath.Create("a", "b"), "test2");
            this.service.SetValue(HierarchyPath.Create("a", "b", "c"), "test3");

            // ACT

            this.service.RemoveValue(HierarchyPath.Create("a"), 2);

            // ASSERT

            object value;
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b", "c"), out value));
            Assert.AreEqual("test3", value);
        }

        [Test]
        public void Delete_value_returns_false_if_no_value_was_removed_from_subtree()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a", "b", "c"), "test3");

            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"), 2);

            // ASSERT

            Assert.IsFalse(result);

            object value;
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b", "c"), out value));
            Assert.AreEqual("test3", value);
        }


        [Test]
        public void Delete_no_values_from_hierarchy_if_depth_is_0()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), "test");
            this.service.SetValue(HierarchyPath.Create("a", "b"), "test2");

            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"), 0);

            // ASSERT

            Assert.IsFalse(result);

            object value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.AreEqual("test", value);
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.AreEqual("test2", value);
        }

        #endregion RemoveValue

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

        [Test]
        public void Retrieve_the_descandants_of_inner_root_node_breadth_first()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), "test1");
            this.service.SetValue(HierarchyPath.Create("a", "b"), "test2");
            this.service.SetValue(HierarchyPath.Create("b"), "test3");

            // ACT

            var result = this.service.DescendantsOrSelf(HierarchyPath.Create("a"), 2).ToArray();

            // ASSERT

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(HierarchyPath.Create("a"), result.ElementAt(0).Key);
            Assert.AreEqual("test1", result.ElementAt(0).Value);
            Assert.AreEqual(HierarchyPath.Create("a", "b"), result.ElementAt(1).Key);
            Assert.AreEqual("test2", result.ElementAt(1).Value);
        }
    }
}