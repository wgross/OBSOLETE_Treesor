using Elementary.Hierarchy;

namespace Treesor.Application
{
    public interface ITreesorService
    {
        void SetValue(HierarchyPath<string> path, object value);
    }
}