using NUnit.Framework;

namespace Treesor.Application.Test
{
    [TestFixture]
    public class TreesorNodeValueTest
    {
        [Test]
        public void ContainerNode_IsContainer_returns_true()
        {
            // ARRANGE

            TreesorNodeValueBase nodeValue = new TreesorNodeValueContainer();

            // ACT

            var result = nodeValue.IsContainer;

            // ASSERT

            Assert.IsTrue(result);
        }

        [Test]
        public void ValueNode_IsContainer_returns_false()
        {
            // ARRANGE

            TreesorNodeValueBase nodeValue = new TreesorNodeValue(null);

            // ACT

            var result = nodeValue.IsContainer;

            // ASSERT

            Assert.IsFalse(result);
        }

        [Test]
        public void ValueNodes_are_equal_if_data_is_equal()
        {
            // ARRANGE

            var a = new TreesorNodeValue("test");
            var b = new TreesorNodeValue("test");

            // ACT

            var result = (a.Equals(b) && b.Equals(a));

            // ASSERT

            Assert.IsTrue(result);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ValueNodes_are_equal_if_both_data_is_null()
        {
            // ARRANGE

            var a = new TreesorNodeValue(null);
            var b = new TreesorNodeValue(null);

            // ACT

            var result = (a.Equals(b) && b.Equals(a));

            // ASSERT

            Assert.IsTrue(result);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ValueNodes_are_not_equal_if_one_data_is_null()
        {
            // ARRANGE

            var a = new TreesorNodeValue(null);
            var b = new TreesorNodeValue("test");

            // ACT

            var result = (a.Equals(b) && b.Equals(a));

            // ASSERT

            Assert.IsFalse(result);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void ValueNodes_are_not_equal_if_data_isnt_equal()
        {
            // ARRANGE

            var a = new TreesorNodeValue(17);
            var b = new TreesorNodeValue("test");

            // ACT

            var result = (a.Equals(b) && b.Equals(a));

            // ASSERT

            Assert.IsFalse(result);
            Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
        }
    }
}