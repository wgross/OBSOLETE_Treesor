using System;

namespace Treesor.PowershellDriveProvider
{
    public class TreesorContainerItem : TreesorNode
    {
        public TreesorContainerItem()
            :this(TreesorNodePath.RootPath)
        {

        }

        public TreesorContainerItem(TreesorNodePath path) : base(path)
        {
        }

        internal void ClearPropertyValue(TreesorNodeProperty propertyDefinition)
        {  
            throw new NotImplementedException();
        }

        internal void SetPropertyValue(TreesorNodeProperty propertyDefinition, object value)
        {
            throw new NotImplementedException();
        }

        internal bool TryGetPropertyValue<T>(TreesorNodeProperty propertyDefinition, out object value)
        {
            throw new NotImplementedException();
        }
    }
}