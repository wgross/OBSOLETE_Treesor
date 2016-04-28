using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using NUnit.Framework;
using System;
using System.Linq;

namespace Treesor.Application.Test
{
    [TestFixture]
    public class TreesorServiceValuesTest
    {
        private MutableHierarchy<string, TreesorNodeValueBase> hierarchy;
        private TreesorService service;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.hierarchy = new MutableHierarchy<string, TreesorNodeValueBase>();
            this.service = new TreesorService(this.hierarchy);
        }

        [Test]
        public void SetValue_at_hierarchy_node()
        {
            // ACT

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorNodeValue("test"));
        }

        [Test]
        public void SetValue_at_hierarchy_root_fails_with_InvalidOperationException()
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.service.SetValue(HierarchyPath.Create<string>(), new TreesorNodeValue("test")));

            // ASSERT

            Assert.That(result.Message.Contains("Root may not have a value"));
        }

        [Test]
        public void SetValue_of_null_fails_with_ArgumentNullException()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.service.SetValue(HierarchyPath.Create("a"), null));

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("value", result.ParamName);
        }

        [Test]
        public void SetValue_converts_a_container_if_it_has_no_children()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorNodeValueContainer());

            // ACT

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorNodeValue("test2"));

            // ASSERT

            TreesorNodeValueBase value;
            this.service.TryGetValue(HierarchyPath.Create("a"), out value);
            Assert.IsInstanceOf<TreesorNodeValue>(value);
            Assert.AreEqual("test2", ((TreesorNodeValue)value).Value);
        }

        [Test]
        public void Read_a_value_from_hierarchy()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorNodeValue("test"));

            // ACT

            TreesorNodeValueBase value;
            bool result = this.service.TryGetValue(HierarchyPath.Create("a"), out value);

            // ASSERT

            Assert.IsTrue(result);
            Assert.AreEqual("test", ((TreesorNodeValue)value).Value);
        }

        #region RemoveValue

        [Test]
        public void Delete_a_value_from_hierarchy()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorNodeValue("test"));

            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"));

            // ASSERT

            TreesorNodeValueBase value;
            Assert.IsTrue(result);
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
        }

        [Test]
        public void Delete_a_value_which_is_missing_returns_false()
        {
            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"));

            // ASSERT

            TreesorNodeValueBase value;
            Assert.IsFalse(result);
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
        }

        [Test]
        public void Delete_value_from_hierarchy_recursive()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorNodeValue("test"));
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorNodeValue("test2"));
            this.service.SetValue(HierarchyPath.Create("a", "b", "c"), new TreesorNodeValue("test3"));

            // ACT

            this.service.RemoveValue(HierarchyPath.Create("a"), 2);

            // ASSERT

            TreesorNodeValueBase value;
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b", "c"), out value));
            Assert.AreEqual("test3", ((TreesorNodeValue)value).Value);
        }

        [Test]
        public void Delete_value_returns_false_if_no_value_was_removed_from_subtree()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a", "b", "c"), new TreesorNodeValue("test3"));

            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"), 2);

            // ASSERT

            Assert.IsFalse(result);

            TreesorNodeValueBase value;
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b", "c"), out value));
            Assert.AreEqual("test3", ((TreesorNodeValue)value).Value);
        }

        [Test]
        public void Delete_no_values_from_hierarchy_if_depth_is_0()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorNodeValue("test"));
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorNodeValue("test2"));

            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"), 0);

            // ASSERT

            Assert.IsFalse(result);

            TreesorNodeValueBase value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.AreEqual("test", ((TreesorNodeValue)value).Value);
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.AreEqual("test2", ((TreesorNodeValue)value).Value);
        }

        #endregion RemoveValue

        [Test]
        public void Retrieve_the_descandants_of_the_root_node_breadth_first()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorNodeValue("test1"));
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorNodeValue("test2"));
            this.service.SetValue(HierarchyPath.Create("b"), new TreesorNodeValue("test3"));

            // ACT

            var result = this.service.DescendantsOrSelf(HierarchyPath.Create<string>(), 2).ToArray();

            // ASSERT

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(HierarchyPath.Create<string>(), result.ElementAt(0).Key);
            Assert.IsNull(result.ElementAt(0).Value);
            Assert.AreEqual(HierarchyPath.Create("a"), result.ElementAt(1).Key);
            Assert.AreEqual("test1", ((TreesorNodeValue)result.ElementAt(1).Value).Value);
            Assert.AreEqual(HierarchyPath.Create("b"), result.ElementAt(2).Key);
            Assert.AreEqual("test3", ((TreesorNodeValue)result.ElementAt(2).Value).Value);
            //Assert.AreEqual(HierarchyPath.Create("a", "b"), result.ElementAt(3).Key);
            //Assert.AreEqual("test2", result.ElementAt(3).Value);
        }

        [Test]
        public void Retrieve_the_descandants_of_inner_root_node_breadth_first()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorNodeValue("test1"));
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorNodeValue("test2"));
            this.service.SetValue(HierarchyPath.Create("b"), new TreesorNodeValue("test3"));

            // ACT

            var result = this.service.DescendantsOrSelf(HierarchyPath.Create("a"), 2).ToArray();

            // ASSERT

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(HierarchyPath.Create("a"), result.ElementAt(0).Key);
            Assert.AreEqual("test1", ((TreesorNodeValue)result.ElementAt(0).Value).Value);
            Assert.AreEqual(HierarchyPath.Create("a", "b"), result.ElementAt(1).Key);
            Assert.AreEqual("test2", ((TreesorNodeValue)result.ElementAt(1).Value).Value);
        }
    }
}