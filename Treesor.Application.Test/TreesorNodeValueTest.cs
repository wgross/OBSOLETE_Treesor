using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            TreesorNodeValueBase nodeValue = new TreesorNodeValue();

            // ACT

            var result = nodeValue.IsContainer;

            // ASSERT

            Assert.IsFalse(result);
        }
    }
}
