using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;

namespace Treesor.Application
{
    public class TreesorService : ITreesorService
    {
        public TreesorService(MutableHierarchy<string,object> hierarchy)
        {
            this.hierarchy = hierarchy;
        }

        private readonly MutableHierarchy<string, object> hierarchy;

        public void SetValue(HierarchyPath<string> path, object value)
        {
            this.hierarchy.Add(path, value);
        }

        public bool TryGetValue(HierarchyPath<string> hierarchyPath, out object value)
        {
            return this.hierarchy.TryGetValue(hierarchyPath, out value);
        }
    }
}
