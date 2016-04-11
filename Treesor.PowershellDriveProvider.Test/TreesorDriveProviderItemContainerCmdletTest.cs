using Elementary.Hierarchy.Collections;
using Moq;
using NLog;
using NLog.Config;
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

            [OneTimeSetUp]
            public void SetUpFixture()
            {
                LogManager.Configuration = new XmlLoggingConfiguration(
                  Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "NLog.Config"));
            }

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
            public void Powershell_new_item_at_root_creates_new_node_without_value()
            {
                // ARRANGE

                var childContainer = new TreesorContainerNode
                {
                    Name = "child"
                };

                this.treesorService.Setup(s => s.CreateContainer(TreesorNodePath.Create("child"))).Returns(childContainer);

                // ACT

                var result = this.powershell.AddStatement()
                    .AddCommand("New-Item")
                    .AddParameter("Path", "treesor:/child")
                    .Invoke();

                // ASSERT

                Assert.AreEqual(1, result.Count);
                Assert.IsInstanceOf<TreesorContainerNode>(result.Single().BaseObject);
                Assert.AreEqual("child", ((TreesorContainerNode)(result.Single().BaseObject)).Name);
                
                this.treesorService.Verify(s => s.CreateContainer(TreesorNodePath.Create("child")), Times.Once);
                this.treesorService.VerifyAll();
            }
        }
    }
}