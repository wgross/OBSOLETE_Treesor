using Elementary.Hierarchy.Collections;
using Moq;
using NLog;
using NUnit.Framework;
using System;
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

        #region Get-/Set-/Clear-Item -Path treesor:/, Test-Path -Path treesor:/

        [Test]
        public void Powershell_GetItem_root_returns_DriveInfo()
        {
            // ARRANGE

            TreesorContainerItem rootContainer = new TreesorContainerItem();
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
        public void Powershell_SetItem_root_fails_with_PSNotSupportedException()
        {
            // ARRANGE

            var rootContainer = new TreesorContainerItem(TreesorNodePath.RootPath);

            this.treesorService
                .Setup(s => s.SetValue(TreesorNodePath.Create(), "value"))
                .Throws(new InvalidOperationException("Container may not have a value"));

            // ACT

            var result = this.powershell
                .AddStatement()
                .AddCommand("Set-Item")
                .AddParameter("Path", "treesor:/")
                .AddParameter("Value", "value")
                .Invoke();

            // ASSERT

            Assert.IsFalse(result.Any());
            Assert.IsTrue(this.powershell.HadErrors);
            Assert.AreEqual(PSInvocationState.Failed, this.powershell.InvocationStateInfo.State);
            Assert.IsInstanceOf<CmdletInvocationException>(this.powershell.InvocationStateInfo.Reason);
            Assert.IsTrue(this.powershell.InvocationStateInfo.Reason.Message.Contains("Set-Item"));
            Assert.IsTrue(this.powershell.InvocationStateInfo.Reason.Message.Contains("isn't supported"));

            this.treesorService.Verify(s => s.SetValue(TreesorNodePath.Create(), "value"), Times.Once);
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_ClearItem_root_fails_with_PSNotSupportedException()
        {
            // ARRANGE

            var rootContainer = new TreesorContainerItem(TreesorNodePath.RootPath);

            this.treesorService.Setup(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer)).Returns(true);
            this.treesorService
                .Setup(s => s.RemoveValue(TreesorNodePath.Create()))
                .Throws(new InvalidOperationException("Container may not have a value"));

            // ACT

            var result = this.powershell
                .AddStatement()
                .AddCommand("Clear-Item")
                .AddParameter("Path", "treesor:/")
                .Invoke();

            // ASSERT

            Assert.IsFalse(result.Any());
            Assert.IsTrue(this.powershell.HadErrors);
            Assert.AreEqual(PSInvocationState.Failed, this.powershell.InvocationStateInfo.State);
            Assert.IsInstanceOf<CmdletInvocationException>(this.powershell.InvocationStateInfo.Reason);
            Assert.IsTrue(this.powershell.InvocationStateInfo.Reason.Message.Contains("Clear-Item"));
            Assert.IsTrue(this.powershell.InvocationStateInfo.Reason.Message.Contains("isn't supported"));

            this.treesorService.Verify(s => s.TryGetContainer(TreesorNodePath.Create(), out rootContainer), Times.Once);
            this.treesorService.Verify(s => s.RemoveValue(TreesorNodePath.Create()), Times.Once);
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_TestPath_root_returns_true()
        {
            // ARRANGE

            var rootContainer = new TreesorContainerItem(TreesorNodePath.RootPath);

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

        #endregion Get-/Set-/Clear-Item -Path treesor:/, Test-Path -Path treesor:/

        #region Set-Item -Value

        [Test]
        public void Powershell_SetItem_inner_node_creates_new_value_node()
        {
            // ARRANGE

            var valueNode = new TreesorValueNode(TreesorNodePath.RootPath);

            this.treesorService
                .Setup(s => s.SetValue(TreesorNodePath.Create("child"), "value"))
                .Returns(valueNode);

            // ACT

            var result = this.powershell
                .AddStatement()
                .AddCommand("Set-Item")
                .AddParameter("Path", "treesor:/child")
                .AddParameter("Value", "value")
                .Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.SetValue(TreesorNodePath.Create("child"), "value"), Times.Once);
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_SetItem_inner_node_twice_changes_value_node()
        {
            // ARRANGE

            var valueNode = new TreesorValueNode(TreesorNodePath.RootPath);

            this.treesorService
                .Setup(s => s.SetValue(TreesorNodePath.Create("child"), "value"))
                .Returns(valueNode)
                .Callback(delegate { });

            this.treesorService
                .Setup(s => s.SetValue(TreesorNodePath.Create("child"), "value2"))
                .Returns(valueNode);

            // ACT

            var result = this.powershell
                .AddStatement()
                    .AddCommand("Set-Item")
                    .AddParameter("Path", "treesor:/child")
                    .AddParameter("Value", "value")
                .AddStatement()
                    .AddCommand("Set-Item")
                    .AddParameter("Path", "treesor:/child")
                    .AddParameter("Value", "value2")
                .Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);
            Assert.AreEqual(0, result.Count);

            this.treesorService.Verify(s => s.SetValue(TreesorNodePath.Create("child"), "value"), Times.Once);
            this.treesorService.Verify(s => s.SetValue(TreesorNodePath.Create("child"), "value2"), Times.Once);
            this.treesorService.VerifyAll();
        }

        [Test]
        public void Powershell_SetItem_inner_node_deep_creates_new_value_node()
        {
            // ARRANGE

            var valueNode = new TreesorValueNode(TreesorNodePath.RootPath);

            this.treesorService
                .Setup(s => s.SetValue(TreesorNodePath.Create("child", "grandchild"), "value"))
                .Returns(valueNode);

            // ACT

            var result = this.powershell
                .AddStatement()
                .AddCommand("Set-Item")
                .AddParameter("Path", "treesor:/child/grandchild")
                .AddParameter("Value", "value")
                .Invoke();

            // ASSERT

            Assert.IsFalse(this.powershell.HadErrors);

            this.treesorService.Verify(s => s.SetValue(TreesorNodePath.Create("child", "grandchild"), "value"), Times.Once);
            this.treesorService.VerifyAll();
        }

        #endregion Set-Item -Value
    }
}