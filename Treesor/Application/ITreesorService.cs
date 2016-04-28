using Elementary.Hierarchy;
using System.Collections.Generic;

namespace Treesor.Application
{
    public interface ITreesorService
    {
        void SetValue(HierarchyPath<string> path, TreesorNodeValueBase value);

        bool TryGetValue(HierarchyPath<string> hierarchyPath, out TreesorNodeValueBase value);

        bool RemoveValue(HierarchyPath<string> hierarchyPath, int? depth = 0);

        IEnumerable<KeyValuePair<HierarchyPath<string>, TreesorNodeValueBase>> DescendantsOrSelf(HierarchyPath<string> path, int maxDepth);
    }
}