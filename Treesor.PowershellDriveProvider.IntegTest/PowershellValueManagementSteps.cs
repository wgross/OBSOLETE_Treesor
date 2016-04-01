using NUnit.Framework;
using RestSharp;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management.Automation;
using TechTalk.SpecFlow;

namespace Treesor.PowershellDriveProvider.IntegTest
{
    [Binding]
    public class PowershellValueManagementSteps
    {
        private PowerShell powershell;
         
        private RestClient client;

        private string path(string p) => p == "root-path" ? string.Empty : p;

        private static string GetCommonRootPath()
        {
            var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(location))));
        }

        private static Process EnsureTreesorProcess()
        {
            lock (typeof(PowershellValueManagementSteps))
            {
                var treesorProcess = Process.GetProcessesByName("Treesor").FirstOrDefault();
                if (treesorProcess == null)
                {
                    treesorProcess = Process.Start(Path.Combine(GetCommonRootPath(), "Treesor/bin/Debug/Treesor.exe"));
                }
                return treesorProcess;
            }
        }

        private static FileInfo GetTreesorDriveProvider()
        {
            return new FileInfo(Path.Combine(GetCommonRootPath(), "Treesor.PowershellDriveProvider/bin/Debug/TreesorDriveProvider.dll"));
        }

        [Given]
        public void Given_treesor_is_running_at_HOST_and_PORT(string host, int port)
        {
            this.client = new RestClient($"http://{host}:{port}/api/{{path}}");

            Assert.IsNotNull(EnsureTreesorProcess());
            Assert.IsNotNull(this.client);
        }

        [Given]
        public void Given_TreesorDriveProvider_is_imported()
        {
            this.powershell = PowerShell.Create();
            var result1 = this.powershell.AddCommand("Import-Module").AddArgument(GetTreesorDriveProvider()).Invoke();
            var result2 = this.powershell.AddCommand("Test-Path").AddArgument("treesor:/").Invoke();
        }

        [When]
        public void When_i_set_VALUE_at_hierarchy_position_PATH(string value, string path)
        {
            var result = this.powershell.AddCommand("Set-Item").AddParameter("Path",path).AddParameter("Value",value).Invoke();
        }
        
        [Then]
        public void Then_the_result_should_be_P0_on_the_screen(int p0)
        {
            ScenarioContext.Current.Pending();
        }
    }
}