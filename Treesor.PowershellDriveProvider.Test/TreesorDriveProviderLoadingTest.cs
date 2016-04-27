using Elementary.Hierarchy.Collections;
using Moq;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Management.Automation;

namespace Treesor.PowershellDriveProvider.Test
{
    [TestFixture]
    public class TreesorDriveProviderLoadingTest
    {
        private PowerShell powershell;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.powershell = PowerShell.Create();
            var result = this.powershell
                .AddCommand("Set-Location")
                .AddArgument(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location))
                .Invoke();
        }

        [Test]
        public void Powershell_returns_list_or_processes()
        {
            // ACT

            var result = this.powershell.AddStatement().AddCommand("Get-Process").Invoke();

            // ASSERT

            Assert.IsNotNull(result);
            Assert.That(result.Count > 0);
        }

        [Test]
        public void Powershell_loads_Treesor_DriveProvider_automatically()
        {
            // ACT

            this.powershell.AddStatement().AddCommand("Import-Module").AddArgument("./TreesorDriveProvider.dll").Invoke();

            // ASSERT

            var result = this.powershell.AddStatement().AddCommand("Get-PSDrive").Invoke();

            Assert.IsNotNull(result.Select(o => o.BaseObject as PSDriveInfo).SingleOrDefault(ps => ps.Name == "treesor"));
        }

        [Test]
        public void Powershell_creates_drive_with_specific_url()
        {
            // ARRANGE

            var remoteHierachy = new Mock<IHierarchy<string, object>>();
            var treesorService = new Mock<TreesorService>(remoteHierachy.Object);

            string  givenUri = null;
            TreesorService.Factory = uri =>
            {
                givenUri = uri;
                return treesorService.Object;
            };

            this.powershell.AddStatement().AddCommand("Import-Module").AddArgument("./TreesorDriveProvider.dll").Invoke();

            // ACT

            var result = this.powershell.AddStatement()
                .AddCommand("New-PsDrive")
                .AddParameter("Name", "custTree")
                .AddParameter("PsProvider", "Treesor")
                .AddParameter("Root", "http://zumsel:9999")
                .Invoke();

            // ASSERT

            Assert.AreEqual("http://zumsel:9999", givenUri);

            var drives = this.powershell.AddStatement().AddCommand("Get-PSDrive").Invoke();
            
            Assert.IsNotNull(drives.Select(o => o.BaseObject as PSDriveInfo).SingleOrDefault(ps => ps.Name == "treesor"));
            Assert.IsNotNull(drives.Select(o => o.BaseObject as PSDriveInfo).SingleOrDefault(ps => ps.Name == "custTree"));
        }
    }
}