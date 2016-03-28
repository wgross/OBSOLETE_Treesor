using System;
using System.Collections.Generic;

namespace Treesor.PowershellDriveProvider
{
    internal class TreesorNodeService
    {
        private string name;

        public TreesorNodeService(string name)
        {
            this.name = name;
        }

        internal bool TryGetContainer(TreesorNodePath path, out TreesorContainerNode jObjectAtPath)
        {
            throw new NotImplementedException();
        }

        internal TreesorNode GetContainer(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<TreesorNode> GetContainerDescendants(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        internal IEnumerable<TreesorNode> GetContainerChildren(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        internal TreesorNode CreateContainer(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        internal TreesorNode CreateDataNode(TreesorNodePath treesorNodePath, ODataRepository oDataRepository, object newItemValue)
        {
            throw new NotImplementedException();
        }

        internal void RemoveContainer(TreesorNodePath path)
        {
            throw new NotImplementedException();
        }

        internal TreesorContainerNode RenameContainer(TreesorNodePath path, string newName)
        {
            throw new NotImplementedException();
        }

        internal void CopyContainer(TreesorNodePath sourcePath, TreesorNodePath destinationPath)
        {
            throw new NotImplementedException();
        }

        internal void MoveContainer(TreesorNodePath path, TreesorNodePath destination)
        {
            throw new NotImplementedException();
        }

        internal bool TryGetNodeProperty(string propertyName, out TreesorNodeProperty propertyDefinition)
        {
            throw new NotImplementedException();
        }

        internal void CreateNodeProperty(TreesorNodeProperty treesorNodeProperty)
        {
            throw new NotImplementedException();
        }

        internal void CopyPropertyValue(TreesorContainerNode fromNode, TreesorNodeProperty fromProperty, TreesorContainerNode toNode, TreesorNodeProperty toProperty)
        {
            throw new NotImplementedException();
        }

        internal void MovePropertyValue(TreesorContainerNode fromNode, TreesorNodeProperty fromProperty, TreesorContainerNode toNode, TreesorNodeProperty toProperty)
        {
            throw new NotImplementedException();
        }
    }
}