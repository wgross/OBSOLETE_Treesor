using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
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

        public TreesorService(MutableHierarchy<string, object> hierarchy)
        {
            this.hierarchy = hierarchy;
        }

        private readonly MutableHierarchy<string, object> hierarchy;

        public void SetValue(HierarchyPath<string> path, object value)
        {
            log.Debug().Message("Setting value at '{0}' to '{1}'", path, value).Write();

            this.hierarchy.Add(path, value);

            log.Info().Message("Set value at path '{0}' to '{1}'", path, value).Write();
        }

        public bool TryGetValue(HierarchyPath<string> hierarchyPath, out object value)
        {
            return this.hierarchy.TryGetValue(hierarchyPath, out value);
        }

        public void RemoveValue(HierarchyPath<string> hierarchyPath)
        {
            log.Debug().Message("Removing value at path '{0}'", hierarchyPath).Write();

            if (this.hierarchy.Remove(hierarchyPath))
                log.Info().Message("Removed value at '{0}'", hierarchyPath).Write();
            else
                log.Info().Message("Removing value at '{0}' failed", hierarchyPath).Write();
        }

        public IEnumerable<KeyValuePair<HierarchyPath<string>, object>> DescendantsOrSelf(HierarchyPath path, int maxDepth)
        {
            return this.hierarchy.Traverse()
                .DescendantAt(path)
                .DescendantsOrSelf(depthFirst: false, maxDepth: maxDepth)
                .Select(n =>
                {
                    if (n.HasValue)
                        return new KeyValuePair<HierarchyPath<string>, object>(n.Path, n.Value);
                    else
                        return new KeyValuePair<HierarchyPath<string>, object>(n.Path, null);
                });
        }
    }
}