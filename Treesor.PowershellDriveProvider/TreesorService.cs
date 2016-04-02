using Elementary.Hierarchy.Collections;
using System;
using System.Collections.Generic;
using Flurl;
using Flurl.Http;
using Treesor.Service.Endpoints;
using System.Net;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;

namespace Treesor.PowershellDriveProvider
{
    public class TreesorService
    {
        private readonly IHierarchy<string, object> remoteHierarchy;
        
        public TreesorService(IHierarchy<string, object> model)
        {
            this.remoteHierarchy = model;
        }
        
        /// <summary>
        /// Creates a container node in the hierachymodel.
        /// Containers ar always created on demand
        /// </summary>
        /// <param name="path"></param>
        /// <param name="containerNode"></param>
        /// <returns></returns>
        public bool TryGetContainer(TreesorNodePath path, out TreesorContainerNode containerNode)
        {
            object remoteValue;

            this.remoteHierarchy.TryGetValue(path.HierarchyPath, out remoteValue);

            containerNode = new TreesorContainerNode
            {
                Name = path.HierarchyPath.Items.LastOrDefault()
            };

            return true;
        }

        private TreesorContainerNode GetOrCreateContainerNode(TreesorNodePath path)
        {
            object remoteValue;
            if (this.remoteHierarchy.TryGetValue(path.HierarchyPath, out remoteValue))
            {
                return new TreesorContainerNode
                {
                    Name = null
                };
            }
            return null;
        }

        public TreesorNode GetContainer(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreesorNode> GetContainerDescendants(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TreesorNode> GetContainerChildren(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        public TreesorNode CreateContainer(TreesorNodePath treesorNodePath)
        {
            throw new NotImplementedException();
        }

        public TreesorNode SetValue(TreesorNodePath treesorNodePath, object newItemValue)
        {
            this.remoteHierarchy[treesorNodePath.HierarchyPath] = newItemValue;

            return new TreesorContainerNode
            {
                Name = treesorNodePath.HierarchyPath.Items.LastOrDefault()
            };
        }

        public void RemoveContainer(TreesorNodePath path)
        {
            throw new NotImplementedException();
        }

        public TreesorContainerNode RenameContainer(TreesorNodePath path, string newName)
        {
            throw new NotImplementedException();
        }

        public void CopyContainer(TreesorNodePath sourcePath, TreesorNodePath destinationPath)
        {
            throw new NotImplementedException();
        }

        public void MoveContainer(TreesorNodePath path, TreesorNodePath destination)
        {
            throw new NotImplementedException();
        }

        public bool TryGetNodeProperty(string propertyName, out TreesorNodeProperty propertyDefinition)
        {
            throw new NotImplementedException();
        }

        public void CreateNodeProperty(TreesorNodeProperty treesorNodeProperty)
        {
            throw new NotImplementedException();
        }

        public void CopyPropertyValue(TreesorContainerNode fromNode, TreesorNodeProperty fromProperty, TreesorContainerNode toNode, TreesorNodeProperty toProperty)
        {
            throw new NotImplementedException();
        }

        public void MovePropertyValue(TreesorContainerNode fromNode, TreesorNodeProperty fromProperty, TreesorContainerNode toNode, TreesorNodeProperty toProperty)
        {
            throw new NotImplementedException();
        }
    }
}