using System;

namespace Treesor.PowershellDriveProvider
{
    internal class TreesorNodeProperty
    {
        private string propertyName;
        private Type type;

        public TreesorNodeProperty(string propertyName, Type type)
        {
            this.propertyName = propertyName;
            this.type = type;
        }
    }
}