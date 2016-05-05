using System;

namespace Treesor.PowershellDriveProvider
{
    public class TreesorNode
    {
        protected TreesorNode(TreesorNodePath path)
        {
            this.Path = path;
            this.Name = path.HierarchyPath.Leaf().ToString();
        }

        public TreesorNodePath Path { get; private set; }

        public string Name { get; private set; }

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