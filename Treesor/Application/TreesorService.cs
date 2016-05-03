using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Elementary.Hierarchy.Generic;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Treesor.Application
{
    public class TreesorService : ITreesorService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        public TreesorService(MutableHierarchy<string, TreesorNodePayload> hierarchy)
        {
            this.hierarchy = hierarchy;
        }

        private readonly MutableHierarchy<string, TreesorNodePayload> hierarchy;

        public void SetValue(HierarchyPath<string> path, TreesorNodePayload newValue)
        {
            log.Debug().Message("Setting value at '{0}' to '{1}'", path, newValue).Write();

            if (newValue == null)
                throw new ArgumentNullException(nameof(newValue));

            // root value cona contain a TReesorContainer item as a marker
            // bot no value payload

            if (path.IsRoot && !newValue.IsContainer)
                throw new InvalidOperationException("Root may not have a value");

            // first check if the node stored at position path is a value node and not container.
            // Setting values for containers fails

            IHierarchyNode<string, TreesorNodePayload> node;

            var found = this.hierarchy
                .Traverse(HierarchyPath.Create<string>())
                .TryGetDescendantAt(this.SetValue_TryGetChildNode, path, out node);

            if (found)
            {
                SetValueAtExistingNode(path, newValue, node);
            }
            else
            {
                SetValueAtNewNode(path, newValue);
            }
        }

        private void SetValueAtNewNode(HierarchyPath<string> path, TreesorNodePayload newValue)
        {
            log.Debug().Message($"Node at '{path}' doesn't exist and is created").Write();

            this.hierarchy[path] = newValue;

            log.Info().Message($"Set value at path '{path}' to '{newValue}'").Write();
        }

        private void SetValueAtExistingNode(HierarchyPath<string> path, TreesorNodePayload newValue, IHierarchyNode<string, TreesorNodePayload> node)
        {
            log.Debug().Message($"Node at '{path}' exists and changed").Write();

            if (node.Value == null || !node.Value.IsContainer)
            {
                this.hierarchy[path] = newValue;
            }
            else if (node.Value != null && node.Value.IsContainer)
            {
                if (this.hierarchy.Traverse(path).HasChildNodes)
                    throw new InvalidOperationException($"Node at '{path}' is a container and may not have a value");

                this.hierarchy[path] = newValue;
            }
            else throw new InvalidOperationException($"Node at '{path}' is a container and may not have a value");

            log.Info().Message($"Set value at path '{path}' to '{newValue}'").Write();
        }

        private bool SetValue_TryGetChildNode(IHierarchyNode<string, TreesorNodePayload> parent, string key, out IHierarchyNode<string, TreesorNodePayload> child)
        {
            child = null;

            // descending in the hierachy isn't allowed if parent node is a vaue node and not a container node.
            if (parent.HasValue && !parent.Value.IsContainer)
                throw new InvalidOperationException($"Node at '{parent.Path}' is a value node and may not have child nodes");

            // parent is a container
            // retrieve the first child having the right leaf name.
            child = parent.Children().FirstOrDefault(c => c.Path.Items.Last().Equals(key));

            return child != null;
        }

        public bool TryGetValue(HierarchyPath<string> hierarchyPath, out TreesorNodePayload value)
        {
            return this.hierarchy.TryGetValue(hierarchyPath, out value);
        }

        #region RemoveValue

        /// <summary>
        /// Removes values form hierarchy starting at <paramref name="hierarchyPath"/> until depth <paramref name="depth"/> is reached.
        /// </summary>
        /// <param name="hierarchyPath">path where values is removed</param>
        /// <param name="depth">depth to decendend in the hierarchy for removal, default is 1.</param>
        /// <returns>true, if at least on value was removed.</returns>
        public bool RemoveValue(HierarchyPath<string> hierarchyPath, int? depth = null)
        {
            log.Debug().Message($"Removing value at path '{hierarchyPath}', level={depth.GetValueOrDefault(1)}").Write();

            var depthCoalesced = depth.GetValueOrDefault(1);

            if (depthCoalesced == 1)
            {
                return this.RemoveValueAt(hierarchyPath);
            }
            else if (depthCoalesced > 1)
            {
                return this.RemoveValuesUnder(hierarchyPath, depthCoalesced);
            }
            else
            {
                log.Info().Message($"Didn't remove value at path '{hierarchyPath}', level was <=0").Write();
                return false;
            }
        }

        private bool RemoveValuesUnder(HierarchyPath<string> hierarchyPath, int depthCoalesced)
        {
            var removed = false;

            // remove all nodes from the hierarchy starting at 'hierarchyPath'
            // if the nodes is a container

            foreach (var node in this.hierarchy.Traverse(hierarchyPath).DescendantsOrSelf(depthFirst: false, maxDepth: depthCoalesced).ToArray())
                if (!node.Value.IsContainer)
                    removed = this.hierarchy.Remove(node.Path) || removed;

            return removed;
        }

        private bool RemoveValueAt(HierarchyPath<string> hierarchyPath)
        {
            var removed = false;

            TreesorNodePayload value;
            if (this.hierarchy.TryGetValue(hierarchyPath, out value))
                if (!value.IsContainer)
                    removed = this.hierarchy.Remove(hierarchyPath);

            if (removed)
                log.Info().Message("Removed value at '{0}'", hierarchyPath).Write();
            else
                log.Info().Message($"Removing value at '{hierarchyPath}' failed").Write();

            return removed;
        }

        #endregion RemoveValue

        public IEnumerable<KeyValuePair<HierarchyPath<string>, TreesorNodePayload>> DescendantsOrSelf(HierarchyPath<string> path, int maxDepth)
        {
            return this.hierarchy.Traverse(path)
                .DescendantsOrSelf(depthFirst: false, maxDepth: maxDepth)
                .Select(n =>
                {
                    if (n.HasValue)
                        return new KeyValuePair<HierarchyPath<string>, TreesorNodePayload>(n.Path, n.Value);
                    else
                        return new KeyValuePair<HierarchyPath<string>, TreesorNodePayload>(n.Path, null);
                });
        }
    }
}