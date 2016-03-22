using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using NUnit.Framework;

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
        public void Set_a_value_to_the_hierarchy()
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
    }
}