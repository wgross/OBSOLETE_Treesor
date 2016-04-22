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
        public void TreesorSvc_is_mocked()
        {
            // ACT

            var result = TreesorService.Factory(null);

            // ASSERT

            Assert.IsNotNull(result);
            Assert.AreSame(this.treesorService.Object, result);
        }

        [Test]
        public void Powershell_GetItem_root_returns_DriveInfo()
        {
            // ARRANGE

            TreesorContainerNode rootContainer = new TreesorContainerNode();
            this.treesorService.Setup(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer)).Returns(true);

            // ACT

            PSObject result = this.powershell.AddStatement().AddCommand("Get-Item").AddParameter("Path", "treesor:/").Invoke().Single();

            // ASSERT

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.BaseObject);
            Assert.IsInstanceOf(typeof(Treesor.PowershellDriveProvider.TreesorDriveInfo), result.BaseObject);

            var driveInfo = result.BaseObject as TreesorDriveInfo;

            Assert.AreEqual("treesor", driveInfo.Name);
            Assert.AreEqual("Treesor data store provider", driveInfo.Description);
            Assert.IsInstanceOf(typeof(ProviderInfo), driveInfo.Provider);
            Assert.AreSame(typeof(TreesorDriveCmdletProvider), driveInfo.Provider.ImplementingType);

            this.treesorService.Verify(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer), Times.Once);
            this.treesorService.Verify(s => s.GetContainer(TreesorNodePath.Create()), Times.Never);
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_SetItem_root_set_value()
        {
            // ARRANGE

            var rootContainer = new TreesorContainerNode(TreesorNodePath.RootPath);

            this.treesorService.Setup(s => s.SetValue(TreesorNodePath.Create(), "value")).Returns(rootContainer);

            // ACT

            var result = this.powershell
                .AddStatement()
                .AddCommand("Set-Item")
                .AddParameter("Path", "treesor:/")
                .AddParameter("Value", "value")
                .Invoke();

            // ASSERT

            Assert.IsFalse(result.Any());

            this.treesorService.Verify(s => s.SetValue(TreesorNodePath.Create(), "value"), Times.Once);
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_ClearItem_root_deletes_value()
        {
            // ARRANGE

            var rootContainer = new TreesorContainerNode(TreesorNodePath.RootPath);

            this.treesorService.Setup(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer)).Returns(true);
            this.treesorService.Setup(s => s.RemoveValue(TreesorNodePath.Create()));

            // ACT

            var result = this.powershell
                .AddStatement()
                .AddCommand("Clear-Item")
                .AddParameter("Path", "treesor:/")
                .Invoke();

            // ASSERT

            Assert.IsFalse(result.Any());

            this.treesorService.Verify(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer), Times.Once);
            this.treesorService.Verify(s => s.RemoveValue(TreesorNodePath.Create()), Times.Once);
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_TestPath_root_returns_true()
        {
            // ARRANGE

            var rootContainer = new TreesorContainerNode(TreesorNodePath.RootPath);
            
            this.treesorService.Setup(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer)).Returns(true);

            // ACT

            var result = this.powershell
                .AddStatement()
                .AddCommand("Test-Path")
                .AddParameter("Path", "treesor:/")
                .Invoke();

            // ASSERT

            Assert.IsTrue(result.Any());
            Assert.IsInstanceOf(typeof(bool), result.Single().BaseObject);
            Assert.IsTrue((bool)result.Single().BaseObject);

            this.treesorService.Verify(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer), Times.Once);
            this.treesorService.VerifyAll();
        }
    }
}