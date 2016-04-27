using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using NLog;
using NLog.Fluent;
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

        public bool RemoveValue(HierarchyPath<string> hierarchyPath, int? depth = null)
        {
            log.Debug().Message($"Removing value at path '{hierarchyPath}', level={depth.GetValueOrDefault(1)}").Write();

            var depthCoalesced = depth.GetValueOrDefault(1);

            if (depthCoalesced == 1)
            {
                var removed = this.hierarchy.Remove(hierarchyPath);

                if (removed)
                    log.Info().Message("Removed value at '{0}'", hierarchyPath).Write();
                else
                    log.Info().Message($"Removing value at '{hierarchyPath}' failed, level={depth.GetValueOrDefault(1)}", hierarchyPath).Write();

                return removed;
            }
            else if (depthCoalesced > 1)
            {
                var removed = false;

                // remove all nmodes from the hierarchy starting at 'hierarchyPath' 
                foreach (var nodes in this.hierarchy.Traverse(hierarchyPath).DescendantsOrSelf(depthFirst: false, maxDepth: depthCoalesced).ToArray())
                    removed = this.hierarchy.Remove(nodes.Path) || removed;

                return removed;
            }
            else
            {
                log.Info().Message($"Didn't remove value at path '{hierarchyPath}', level was 0").Write();
                return false;
            }
        }

        public IEnumerable<KeyValuePair<HierarchyPath<string>, object>> DescendantsOrSelf(HierarchyPath<string> path, int maxDepth)
        {
            return this.hierarchy.Traverse(path)
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