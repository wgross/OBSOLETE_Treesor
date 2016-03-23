using Elementary.Hierarchy;

namespace Treesor.Application
{
    public interface ITreesorService
    {
        void SetValue(HierarchyPath<string> path, object value);

        bool TryGetValue(HierarchyPath<string> hierarchyPath, out object value);

        void RemoveValue(HierarchyPath<string> hierarchyPath);
    }
}