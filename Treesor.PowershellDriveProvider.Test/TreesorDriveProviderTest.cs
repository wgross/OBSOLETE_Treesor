using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace Treesor.PowershellDriveProvider.Test
{
    [TestFixture]
    public class TreesorDriveProviderTest
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
        public void Powershell_throws_on_wrong_command()
        {
            // ACT

            Assert.Throws<CommandNotFoundException>(() => this.powershell.AddStatement().AddCommand("Get-Zumsel").Invoke());

        }

        [Test]
        public void Powershell_loads_Treesor_DriveProvider()
        {
            // ARRANGE

            var result = this.powershell.AddStatement().AddCommand("Get-Variable").AddArgument("PWD").Invoke();

            // ACT

            this.powershell.AddStatement().AddCommand("Import-Module").AddArgument("./TreesorDriveProvider.dll").Invoke();
            
            // ASSERT


            var result2 = this.powershell.AddStatement().AddCommand("Get-PSDrive").Invoke();
            
            Assert.IsNotNull(result.Select(o => o.BaseObject as PSDriveInfo).SingleOrDefault(ps => ps.Name == "Treesor"));
        }
    }
}
