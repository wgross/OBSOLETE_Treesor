using Elementary.Hierarchy.Collections;
using Moq;
using NLog;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Treesor.PowershellDriveProvider.Test
{
    public class TreesorDriveProviderItemContainerCmdletTest
    {
        [TestFixture]
        public class TreesorDriveProviderItemCmdletTest
        {
            private static readonly Logger log = LogManager.GetCurrentClassLogger();

            private PowerShell powershell;
            private Mock<TreesorService> treesorService;
            private Mock<IHierarchy<string, object>> remoteHierachy;

            [SetUp]
            public void ArrangeAllTests()
            {
                this.remoteHierachy = new Mock<IHierarchy<string, object>>();
                this.treesorService = new Mock<TreesorService>(this.remoteHierachy.Object);

                TreesorService.Factory = h => this.treesorService.Object;

                this.powershell = PowerShell.Create();

                this.powershell
                    .AddCommand("Set-Location")
                    .AddArgument(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
                    .Invoke();

                this.powershell
                    .AddStatement()
                    .AddCommand("Import-Module")
                    .AddArgument("./TreesorDriveProvider.dll")
                    .Invoke();
            }

            [Test]
            public void Powershell_NewItem_root_creates_new_node_without_value()
            {
                // ARRANGE

                var childContainer = new TreesorContainerNode(TreesorNodePath.Create("child"));

                this.treesorService.Setup(s => s.CreateContainer(TreesorNodePath.Create("child"), null)).Returns(childContainer);

                // ACT

                var result = this.powershell.AddStatement()
                    .AddCommand("New-Item")
                    .AddParameter("Path", "treesor:/child")
                    .Invoke();

                // ASSERT

                Assert.AreEqual(1, result.Count);
                Assert.IsInstanceOf<TreesorContainerNode>(result.Single().BaseObject);
                Assert.AreEqual("child", ((TreesorContainerNode)(result.Single().BaseObject)).Name);

                this.treesorService.Verify(s => s.CreateContainer(TreesorNodePath.Create("child"), null), Times.Once);
                this.treesorService.VerifyAll();
            }

            [Test]
            public void Powershell_NewItem_root_creates_new_node_with_value()
            {
                // ARRANGE

                var childContainer = new TreesorContainerNode(TreesorNodePath.Create("child"));

                this.treesorService.Setup(s => s.CreateContainer(TreesorNodePath.Create("child"), "value")).Returns(childContainer);

                // ACT

                var result = this.powershell.AddStatement()
                    .AddCommand("New-Item")
                    .AddParameter("Path", "treesor:/child")
                    .AddParameter("Value", "value")
                    .Invoke();

                // ASSERT

                Assert.AreEqual(1, result.Count);
                Assert.IsInstanceOf<TreesorContainerNode>(result.Single().BaseObject);
                Assert.AreEqual("child", ((TreesorContainerNode)(result.Single().BaseObject)).Name);
                //?//Assert.AreEqual("value", ((TreesorContainerNode)(result.Single().BaseObject)).Value));

                this.treesorService.Verify(s => s.CreateContainer(TreesorNodePath.Create("child"), "value"), Times.Once);
                this.treesorService.VerifyAll();
            }

            [Test]
            public void Powershell_NewItem_at_inner_node_creates_new_node_without_value()
            {
                // ARRANGE
                // the node is created with the deppath. There is no chacking of the ancestor 
                // part of the hierarchy during the creation of a grandchild.

                var childContainer = new TreesorContainerNode(TreesorNodePath.Create("a","b"));

                this.treesorService
                    .Setup(s => s.CreateContainer(TreesorNodePath.Create("a","b"), null))
                    .Returns(childContainer);

                // ACT

                var result = this.powershell.AddStatement()
                    .AddCommand("New-Item")
                    .AddParameter("Path", "treesor:/a/b")
                    .Invoke();

                // ASSERT

                Assert.AreEqual(1, result.Count);
                Assert.AreSame(childContainer, result[0].BaseObject);
                
                this.treesorService.Verify(s => s.CreateContainer(TreesorNodePath.Create("a","b"), null), Times.Once);
                this.treesorService.VerifyAll();
            }

            [Test]
            public void Powershell_GetChildItem_at_root_node_returns_Children()
            {
                // ARRANGE

                TreesorContainerNode rootContainer = new TreesorContainerNode();
                this.treesorService
                    .Setup(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer))
                    .Returns(true);

                this.treesorService
                    .Setup(s => s.GetContainer(TreesorNodePath.Create()))
                    .Returns(rootContainer);

                var nodes = new[]
                {
                    new TreesorContainerNode(TreesorNodePath.Create("a")),
                    new TreesorContainerNode(TreesorNodePath.Create("b")),
                };

                this.treesorService
                    .Setup(s => s.GetContainerChildren(TreesorNodePath.Create()))
                    .Returns(nodes);

                // ACT

                var result = this.powershell.AddStatement()
                    .AddCommand("Get-ChildItem")
                    .AddParameter("Path", "treesor:/")
                    .Invoke();

                // ASSERT

                this.treesorService.VerifyAll();
                this.treesorService.Verify(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer), Times.Once);
                this.treesorService.Verify(s => s.GetContainer(TreesorNodePath.Create()), Times.Once);
                this.treesorService.Verify(s => s.GetContainerChildren(TreesorNodePath.Create()), Times.Once);

                Assert.AreEqual(2, result.Count);
                Assert.AreSame(nodes[0], result[0].BaseObject);
                Assert.AreSame(nodes[1], result[1].BaseObject);
            }

            [Test]
            public void Powershell_GetChildItem_Recurse_at_root_node_returns_Descandants()
            {
                // ARRANGE

                TreesorContainerNode rootContainer = new TreesorContainerNode();
                this.treesorService
                    .Setup(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer))
                    .Returns(true);

                this.treesorService
                    .Setup(s => s.GetContainer(TreesorNodePath.Create()))
                    .Returns(rootContainer);

                var nodes = new[]
                {
                    new TreesorContainerNode(TreesorNodePath.Create("a")),
                    new TreesorContainerNode(TreesorNodePath.Create("b")),
                    new TreesorContainerNode(TreesorNodePath.Create("a","b")),
                };

                this.treesorService
                    .Setup(s => s.GetContainerDescendants(TreesorNodePath.Create()))
                    .Returns(nodes);

                // ACT

                var result = this.powershell.AddStatement()
                    .AddCommand("Get-ChildItem")
                    .AddParameter("Path", "treesor:/")
                    .AddParameter("Recurse")
                    .Invoke();

                // ASSERT

                this.treesorService.Verify(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer), Times.Once);
                this.treesorService.Verify(s => s.GetContainer(TreesorNodePath.Create()), Times.Exactly(2)); // from IsItemContainer
                this.treesorService.Verify(s => s.GetContainerDescendants(TreesorNodePath.Create()), Times.Once);
                this.treesorService.VerifyAll();

                Assert.AreEqual(3, result.Count);
                Assert.AreSame(nodes[0], result[0].BaseObject);
                Assert.AreSame(nodes[1], result[1].BaseObject);
                Assert.AreSame(nodes[2], result[2].BaseObject);
            }

            [Test]
            public void Powershell_GetChildItem_at_inner_node_returns_Children()
            {
                // ARRANGE

                TreesorContainerNode innerNodeContainer = new TreesorContainerNode();
                this.treesorService
                    .Setup(s => s.TryGetContainer(TreesorNodePath.Create("a"), out innerNodeContainer))
                    .Returns(true);

                this.treesorService
                    .Setup(s => s.GetContainer(TreesorNodePath.Create("a")))
                    .Returns(innerNodeContainer);

                var nodes = new[]
                {
                    new TreesorContainerNode(TreesorNodePath.Create("a","a")),
                    new TreesorContainerNode(TreesorNodePath.Create("a","b")),
                };

                this.treesorService
                    .Setup(s => s.GetContainerChildren(TreesorNodePath.Create("a")))
                    .Returns(nodes);

                // ACT

                var result = this.powershell.AddStatement()
                    .AddCommand("Get-ChildItem")
                    .AddParameter("Path", "treesor:/a")
                    .Invoke();

                // ASSERT

                this.treesorService.VerifyAll();
                this.treesorService.Verify(s => s.TryGetContainer(TreesorNodePath.Create("a"), out innerNodeContainer), Times.Once);
                this.treesorService.Verify(s => s.GetContainer(TreesorNodePath.Create("a")), Times.Once);
                this.treesorService.Verify(s => s.GetContainerChildren(TreesorNodePath.Create("a")), Times.Once);

                Assert.AreEqual(2, result.Count);
                Assert.AreSame(nodes[0], result[0].BaseObject);
                Assert.AreSame(nodes[1], result[1].BaseObject);
            }

            [Test]
            public void Powershell_GetChildItem_Recurse_at_inner_node_returns_Descandants()
            {
                // ARRANGE

                TreesorContainerNode innerNodeContainer = new TreesorContainerNode();
                this.treesorService
                    .Setup(s => s.TryGetContainer(TreesorNodePath.Create("a"), out innerNodeContainer))
                    .Returns(true);

                this.treesorService
                    .Setup(s => s.GetContainer(TreesorNodePath.Create("a")))
                    .Returns(innerNodeContainer);

                var nodes = new[]
                {
                    new TreesorContainerNode(TreesorNodePath.Create("a","a")),
                    new TreesorContainerNode(TreesorNodePath.Create("a","b")),
                    new TreesorContainerNode(TreesorNodePath.Create("a", "b", "a")),
                };

                this.treesorService
                    .Setup(s => s.GetContainerDescendants(TreesorNodePath.Create("a")))
                    .Returns(nodes);

                // ACT

                var result = this.powershell.AddStatement()
                    .AddCommand("Get-ChildItem")
                    .AddParameter("Path", "treesor:/a")
                    .AddParameter("Recurse")
                    .Invoke();

                // ASSERT

                this.treesorService.VerifyAll();
                this.treesorService.Verify(s => s.TryGetContainer(TreesorNodePath.Create("a"), out innerNodeContainer), Times.Once);
                this.treesorService.Verify(s => s.GetContainer(TreesorNodePath.Create("a")), Times.Exactly(2));
                this.treesorService.Verify(s => s.GetContainerDescendants(TreesorNodePath.Create("a")), Times.Once);

                Assert.AreEqual(3, result.Count);
                Assert.AreSame(nodes[0], result[0].BaseObject);
                Assert.AreSame(nodes[1], result[1].BaseObject);
                Assert.AreSame(nodes[2], result[2].BaseObject);
            }
        }
    }
}