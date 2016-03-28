using System;
using System.Management.Automation;

namespace Treesor.PowershellDriveProvider
{
    internal class TreesorNewODataDynamicParameters
    {
        internal static TreesorNewODataDynamicParameters Get(TreesorNodePath treesorNodePath, string itemTypeName)
        {
            throw new NotImplementedException();
        }


        [Parameter(Mandatory = true)]
        [ValidateNotNullOrEmpty]
        public Uri Endpoint { get; set; }
    }
}