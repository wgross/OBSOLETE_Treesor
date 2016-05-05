﻿using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using Treesor.Client;

namespace Treesor.PowershellDriveProvider
{
    public class TreesorNodeService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        #region Construction and Initialization of this instance

        static TreesorNodeService()
        {
            Factory = url => new TreesorNodeService(new RemoteHierarchy(url));
        }

        /// <summary>
        /// Treesoreservice factory is a public property to provide an entry point for tests of teh
        /// powershell drive provider.
        /// </summary>
        public static Func<string, TreesorNodeService> Factory { get; set; }

        public TreesorNodeService(IHierarchy<string, object> model)
        {
            this.remoteHierarchy = model;
        }

        private readonly IHierarchy<string, object> remoteHierarchy;

        #endregion Construction and Initialization of this instance

        /// <summary>
        /// Creates a container node in the hierachymodel.
        /// Containers ar always created on demand
        /// </summary>
        /// <param name="path"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        public virtual bool TryGetNode(TreesorNodePath path, out TreesorNode node)
        {
            log.Debug().Message($"Retrieving container '{path}'").Write();

            // retrieving a container means to retrieve a value from the
            // hierarchical dictionary

            object remoteValue;
            if (this.remoteHierarchy.TryGetValue(path.HierarchyPath, out remoteValue))
            {
                log.Info().Message($"Retrieved container {path}").Write();
                node = new TreesorContainerItem(path);
                return true;
            }
            else
            {
                log.Warn().Message($"Couldn't retrieve container {path}").Write();
                node = null;
                return false;
            }
        }
        
        public virtual TreesorNode GetNode(TreesorNodePath path)
        {
            object remoteValue;

            this.remoteHierarchy.TryGetValue(path.HierarchyPath, out remoteValue);

            return new TreesorValueItem(path);
        }

        public virtual IEnumerable<TreesorNode> GetContainerDescendants(TreesorNodePath path)
        {
            throw new NotImplementedException();
            //return this.remoteHierarchy
            //    .Traverse(path.HierarchyPath)
            //    .Descendants()
            //    .Select(n => new TreesorContainerNode
            //    {
            //        Name = n.Path.Leaf().ToString()
            //    });
        }

        public virtual bool HasChildNodes(TreesorNodePath path)
        {
            return this.GetContainerChildren(path).Any();
        }

        public virtual IEnumerable<TreesorNode> GetContainerChildren(TreesorNodePath path)
        {
            return this.remoteHierarchy
                .Traverse(path.HierarchyPath)
                .Children()
                .Select(n => new TreesorContainerItem(TreesorNodePath.Create(n.Path)));
        }

        public virtual TreesorValueItem CreateValue(TreesorNodePath path, object value)
        {
            throw new NotImplementedException();
        }

        public virtual TreesorContainerItem CreateContainer(TreesorNodePath path)
        {
            this.remoteHierarchy[path.HierarchyPath] = null;

            return new TreesorContainerItem(path);
        }

        public virtual TreesorNode SetValue(TreesorNodePath path, object newItemValue)
        {
            if (path.IsDrive)
                throw new InvalidOperationException("Root may not have a value");

            object container;
            if (this.remoteHierarchy.TryGetValue(path.HierarchyPath, out container))
                throw new InvalidOperationException("Container may not have a value");
            else
                return null;

            //this.remoteHierarchy[path.HierarchyPath] = newItemValue;
            //return new TreesorContainerNode(path);
        }

        public virtual void RemoveValue(TreesorNodePath path)
        {
            if (path.IsDrive)
                throw new InvalidOperationException("Root may not have a value");

            object container;
            if (this.remoteHierarchy.TryGetValue(path.HierarchyPath, out container))
                throw new InvalidOperationException("Container may not have a value");
        }

        public virtual bool RemoveContainer(TreesorNodePath path, bool recursive)
        {
            return this.remoteHierarchy.Remove(path.HierarchyPath, recursive ? int.MaxValue : 1);
        }

        public TreesorContainerItem RenameContainer(TreesorNodePath path, string newName)
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

        public void CopyPropertyValue(TreesorNode fromNode, TreesorNodeProperty fromProperty, TreesorNode toNode, TreesorNodeProperty toProperty)
        {
            throw new NotImplementedException();
        }

        public void MovePropertyValue(TreesorNode fromNode, TreesorNodeProperty fromProperty, TreesorNode toNode, TreesorNodeProperty toProperty)
        {
            throw new NotImplementedException();
        }
    }
}