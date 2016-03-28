using System;

namespace Treesor.PowershellDriveProvider
{
    internal class TreesorNewPropertyDynamicParameterProvider
    {
        public static readonly TreesorNewPropertyDynamicParameterProvider Instance = new TreesorNewPropertyDynamicParameterProvider();

        internal TreesorNewODataDynamicParameters Get(TreesorNodePath treesorNodePath, string propertyName, string propertyTypeName, object value)
        {
            throw new NotImplementedException();
        }
    }
}