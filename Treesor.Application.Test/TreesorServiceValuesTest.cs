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
            this.hierarchy = new MutableHierarchy<string, TreesorNodePayload>();
            this.service = new TreesorService(this.hierarchy);
        }

        #region SetValue

        [Test]
        public void SetValue_at_hierarchy_node()
        {
            // ACT

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test"));
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
        public void SetValue_of_null_fails_with_ArgumentNullException()
        {
            // ACT

            var result = Assert.Throws<ArgumentNullException>(() => this.service.SetValue(HierarchyPath.Create("a"), null));

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreEqual("value", result.ParamName);
        }

        [Test]
        public void SetValue_fails_for_container_nodes()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorContainer());

            // ACT

            var result = Assert.Throws<InvalidOperationException>(() => this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test2")));

            // ASSERT

            Assert.That(result.Message.Contains("'a'"));
            Assert.That(result.Message.Contains("is a container and may not have a value"));
        }

        #endregion SetValue


        [Test]
        public void Read_a_value_from_hierarchy()
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

        #region RemoveValue

        [Test]
        public void Delete_a_value_from_hierarchy()
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
        public void Delete_a_value_which_is_missing_returns_false()
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

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test"));
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2"));
            this.service.SetValue(HierarchyPath.Create("a", "b", "c"), new TreesorValue("test3"));

            // ACT

            this.service.RemoveValue(HierarchyPath.Create("a"), 2);

            // ASSERT

            TreesorNodePayload value;
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
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
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.IsFalse(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b", "c"), out value));
            Assert.AreEqual("test3", ((TreesorValue)value).Value);
        }

        [Test]
        public void Delete_no_values_from_hierarchy_if_depth_is_0()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test"));
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2"));

            // ACT

            var result = this.service.RemoveValue(HierarchyPath.Create("a"), 0);

            // ASSERT

            Assert.IsFalse(result);

            TreesorNodePayload value;
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a"), out value));
            Assert.AreEqual("test", ((TreesorValue)value).Value);
            Assert.IsTrue(this.service.TryGetValue(HierarchyPath.Create("a", "b"), out value));
            Assert.AreEqual("test2", ((TreesorValue)value).Value);
        }

        #endregion RemoveValue

        [Test]
        public void Retrieve_the_descandants_of_the_root_node_breadth_first()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test1"));
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2"));
            this.service.SetValue(HierarchyPath.Create("b"), new TreesorValue("test3"));

            // ACT

            var result = this.service.DescendantsOrSelf(HierarchyPath.Create<string>(), 2).ToArray();

            // ASSERT

            Assert.AreEqual(3, result.Count());
            Assert.AreEqual(HierarchyPath.Create<string>(), result.ElementAt(0).Key);
            Assert.IsNull(result.ElementAt(0).Value);
            Assert.AreEqual(HierarchyPath.Create("a"), result.ElementAt(1).Key);
            Assert.AreEqual("test1", ((TreesorValue)result.ElementAt(1).Value).Value);
            Assert.AreEqual(HierarchyPath.Create("b"), result.ElementAt(2).Key);
            Assert.AreEqual("test3", ((TreesorValue)result.ElementAt(2).Value).Value);
            //Assert.AreEqual(HierarchyPath.Create("a", "b"), result.ElementAt(3).Key);
            //Assert.AreEqual("test2", result.ElementAt(3).Value);
        }

        [Test]
        public void Retrieve_the_descandants_of_inner_root_node_breadth_first()
        {
            // ARRANGE

            this.service.SetValue(HierarchyPath.Create("a"), new TreesorValue("test1"));
            this.service.SetValue(HierarchyPath.Create("a", "b"), new TreesorValue("test2"));
            this.service.SetValue(HierarchyPath.Create("b"), new TreesorValue("test3"));

            // ACT

            var result = this.service.DescendantsOrSelf(HierarchyPath.Create("a"), 2).ToArray();

            // ASSERT

            Assert.AreEqual(2, result.Count());
            Assert.AreEqual(HierarchyPath.Create("a"), result.ElementAt(0).Key);
            Assert.AreEqual("test1", ((TreesorValue)result.ElementAt(0).Value).Value);
            Assert.AreEqual(HierarchyPath.Create("a", "b"), result.ElementAt(1).Key);
            Assert.AreEqual("test2", ((TreesorValue)result.ElementAt(1).Value).Value);
        }
    }
}