using Elementary.Hierarchy;
using System.Collections.Generic;

namespace Treesor.Application
{
    public interface ITreesorService
    {
        void SetValue(HierarchyPath<string> path, object value);

        bool TryGetValue(HierarchyPath<string> hierarchyPath, out object value);

        void RemoveValue(HierarchyPath<string> hierarchyPath);

        IEnumerable<KeyValuePair<HierarchyPath<string>, object>> DescendantsOrSelf(HierarchyPath path, int maxDepth);
    }
}