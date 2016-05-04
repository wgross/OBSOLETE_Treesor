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
        private MutableHierarchy<string, TreesorNodePayload> hierarchy;
        private TreesorService service;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.hierarchy = new MutableHierarchy<string, TreesorNodePayload>(getDefaultValue: p => new TreesorContainer());
            this.service = new TreesorService(this.hierarchy);
        }

        #region SetValue

        [Test]
        public void SetValue_at_new_hierarchy_node()
        {
            // ACT

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test"));

            // ASSERT

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.AreEqual("test", ((TreesorValue)value).Value);
        }

        [Test]
        public void SetValue_as_container_at_new_hierarchy_node()
        {
            // ACT

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorContainer());

            // ASSERT

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsInstanceOf<TreesorContainer>(value);
        }

        [Test]
        public void SetValue_at_new_value_node_under_existing_container_node()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorContainer());

            // ACT

            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2"));

            // ASSERT

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.AreEqual("test2", ((TreesorValue)value).Value);
        }

        [Test]
        public void SetValue_at_deep_new_value_node_creates_new_container_parent()
        {
            // ACT

            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2"));

            // ASSERT

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.AreEqual("test2", ((TreesorValue)value).Value);
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsInstanceOf<TreesorContainer>(value);
        }

        [Test]
        public void SetValue_at_deep_new_container_node_creates_new_container_parent()
        {
            // ACT

            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorContainer());

            // ASSERT

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.IsInstanceOf<TreesorContainer>(value);
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsInstanceOf<TreesorContainer>(value);
        }

        [Test]
        public void SetValue_twice_at_existing_value_node_under_container_node()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test1"));

            // ACT

            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2"));

            // ASSERT

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.AreEqual("test2", ((TreesorValue)value).Value);
        }

        [Test]
        public void SetValue_converts_container_without_children_to_value_node()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorContainer());

            // ACT

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test"));

            // ASSERT

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.AreEqual("test", ((TreesorValue)value).Value);
        }

        [Test]
        public void SetValue_of_null_value_fails_with_ArgumentNullException()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.service.SetValue(HierarchyPath.Create("a"), (TreesorValue)null));

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("newValue", result.ParamName);
        }

        [Test]
        public void SetValue_of_null_container_fails_with_ArgumentNullException()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.service.SetValue(HierarchyPath.Create("a"), (TreesorContainer)null));

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("newValue", result.ParamName);
        }

        [Test]
        public void SetValue_at_hierarchy_root_fails_with_InvalidOperationException()
        {
            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.service.SetValue(HierarchyPath.Create<string>(), new TreesorValue("test")));

            // ASSERT

            Assert.That(result.Message.Contains("Root may not have a value"));
        }

        [Test]
        public void SetValue_at_existing_container_node_with_children_fails_with_InvaidOperationException()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorContainer());
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorContainer());

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test2")));

            // ASSERT

            Assert.That(result.Message.Contains("'a'"));
            Assert.That(result.Message.Contains("is a container and may not have a value"));
        }

        [Test]
        public void SetValue_at_new_value_node_under_existing_value_node_fails_with_InvalidOperationException()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("value"));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2")));

            // ASSERT

            Assert.That(result.Message.Contains("'a'"));
            Assert.That(result.Message.Contains("is a value node and may not have child nodes"));
        }

        [Test]
        public void SetValue_at_new_container_node_under_existing_value_node_fails_with_InvalidOperationException()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("value"));

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorContainer()));

            // ASSERT

            Assert.That(result.Message.Contains("'a'"));
            Assert.That(result.Message.Contains("is a value node and may not have child nodes"));
        }

        #endregion SetValue

        #region TryGetValue

        [Test]
        public void Read_a_container_from_root_by_default()
        {
            // ACT

            TreesorNodePayload value;
            var result = this.service.TryGetValue(HierarchyPath.Create<string>(), out value);

            // ASSERT

            Assert.IsTrue(result);
            Assert.IsInstanceOf<TreesorContainer>(value);
        }

        [Test]
        public void Read_a_value_from_hierarchy_value_node()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test"));

            // ACT

            TreesorNodePayload value;
            bool result = this.service.TryGetValue(HierarchyPath.Create("a"), out value);

            // ASSERT

            Assert.IsTrue(result);
            Assert.AreEqual("test", ((TreesorValue)value).Value);
        }

        [Test]
        public void Read_a_value_from_a_hierarchy_container_node()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorContainer());

            // ACT

            TreesorNodePayload value;
            this.service.TryGetValue(HierarchyPath.Create("a"), out value);

            // ASSERT

            Assert.IsNotNull(value);
            Assert.IsTrue(value.IsContainer);
            Assert.IsInstanceOf<TreesorContainer>(value);
        }

        #endregion TryGetValue

        #region RemoveValue

        [Test]
        public void Delete_a_value_from_a_value_node()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test"));

            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"));

            // ASSERT

            TreesorNodePayload value;
            Assert.IsTrue(result);
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
        }

        [Test]
        public void Delete_a_value_from_missing_node_returns_false()
        {
            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"));

            // ASSERT

            TreesorNodePayload value;
            Assert.IsFalse(result);
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
        }

        [Test]
        public void Delete_value_from_hierarchy_recursive()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorContainer());
            this.service.SetValue(HierarchyPath.Create("a", "a"), new TreesorValue("test2"));
            this.service.SetValue(HierarchyPath.Create("a", "b", "c"), new TreesorValue("test3"));

            // ACT

            this.service.RemoveValue(HierarchyPath.Create("a"), 2);

            // ASSERT

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsTrue(value.IsContainer);
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a", "a"), out value));
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.IsTrue(value.IsContainer);
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b", "c"), out value));
            Assert.AreEqual("test3", ((TreesorValue)value).Value);
        }

        [Test]
        public void Delete_value_returns_false_if_no_value_was_removed_from_subtree()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a", "b", "c"), new TreesorValue("test3"));

            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"), 2);

            // ASSERT

            Assert.IsFalse(result);

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsInstanceOf<TreesorContainer>(value);
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.IsInstanceOf<TreesorContainer>(value);
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b", "c"), out value));
            Assert.AreEqual("test3", ((TreesorValue)value).Value);
        }

        [Test]
        public void Delete_no_values_from_hierarchy_if_depth_is_0()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorContainer());
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2"));

            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"), 0);

            // ASSERT

            Assert.IsFalse(result);

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsTrue(value.IsContainer);
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.AreEqual("test2", ((TreesorValue)value).Value);
        }

        #endregion RemoveValue

        [Test]
        public void Retrieve_the_descandants_of_the_root_node_breadth_first()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorContainer());
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2"));
            this.service.SetValue(HierarchyPath.Create("b"), new TreesorValue("test3"));

            // ACT

            var result = this.service.DescendantsOrSelf(HierarchyPath.Create<string>(), 2).ToArray();

            // ASSERT

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(HierarchyPath.Create<string>(), result.ElementAt(0).Key);
            Assert.IsNotNull(result.ElementAt(0).Value);
            Assert.IsInstanceOf<TreesorContainer>(result.ElementAt(0).Value);
            Assert.AreEqual(HierarchyPath.Create("a"), result.ElementAt(1).Key);
            Assert.IsInstanceOf<TreesorContainer>(result.ElementAt(1).Value);
            Assert.AreEqual(HierarchyPath.Create("b"), result.ElementAt(2).Key);
            Assert.AreEqual("test3", ((TreesorValue)result.ElementAt(2).Value).Value);
        }

        [Test]
        public void Retrieve_the_descandants_of_inner_root_node_breadth_first()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorContainer());
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2"));
            this.service.SetValue(HierarchyPath.Create("b"), new TreesorValue("test3"));

            // ACT

            var result = this.service.DescendantsOrSelf(HierarchyPath.Create("a"), 2).ToArray();

            // ASSERT

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(HierarchyPath.Create("a"), result.ElementAt(0).Key);
            Assert.IsInstanceOf<TreesorContainer>(result.ElementAt(0).Value);
            Assert.AreEqual(HierarchyPath.Create("a", "b"), result.ElementAt(1).Key);
            Assert.AreEqual("test2", ((TreesorValue)result.ElementAt(1).Value).Value);
        }
    }
}