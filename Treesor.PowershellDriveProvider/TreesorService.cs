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

namespace Treesor.PowershellDriveProvider
{
    public class TreesorService
    {
        private string name;
        private readonly IHierarchy<string, TreesorContainerNode> hierarchyModel;
        
        public TreesorService(IHierarchy<string, TreesorContainerNode> model, string name)
        {
            this.hierarchyModel = model;
            this.name = name;
        }

        public TreesorService(string name)
        {
            this.name = name;
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
            containerNode = this.GetOrCreateContainerNode(path);
            return true;
        }

        private TreesorContainerNode GetOrCreateContainerNode(TreesorNodePath path)
        {
            TreesorContainerNode node;
            if (this.hierarchyModel.TryGetValue(path.HierarchyPath, out node))
                return node;

            node = new TreesorContainerNode();
            this.hierarchyModel.Add(path.HierarchyPath, node);
            return node;
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

        public Task<TreesorNode> SetValue(TreesorNodePath treesorNodePath, object newItemValue)
        {
            return "http://localhost:9002/api".PostJsonAsync(new
            {
                value = newItemValue
            })
            .ReceiveJson<HierarchyNodeBody>()
            .ContinueWith<TreesorNode>(t =>
            {
                return (TreesorNode)(new TreesorContainerNode
                {
                    Name = t.Result.Path
                });
            });
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