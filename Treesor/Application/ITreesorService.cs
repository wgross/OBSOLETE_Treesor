using Elementary.Hierarchy;
using System.Collections.Generic;

namespace Treesor.Application
{
    public interface ITreesorService
    {
        void SetValue(HierarchyPath<string> path, TreesorValue value);

        void SetValue(HierarchyPath<string> path, TreesorContainer value);

        bool TryGetValue(HierarchyPath<string> hierarchyPath, out TreesorNodePayload value);

        bool RemoveValue(HierarchyPath<string> hierarchyPath, int? depth = 0);

        IEnumerable<KeyValuePair<HierarchyPath<string>, TreesorNodePayload>> DescendantsOrSelf(HierarchyPath<string> path, int maxDepth);
    }
}