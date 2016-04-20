using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Flurl;
using Flurl.Http;
using Treesor.Service.Endpoints;

namespace Treesor.Client
{
    public class RemoteHierarchy : IHierarchy<string, object>
    {
        private string remoteHierarchyAddress;

        public RemoteHierarchy(string remoteHierarchyAddress)
        {
            this.remoteHierarchyAddress = remoteHierarchyAddress;
        }

        public object this[HierarchyPath<string> hierarchyPath]
        {
            set
            {
                this.remoteHierarchyAddress
                    .AppendPathSegments(hierarchyPath.Items)
                    .PutJsonAsync(new HierarchyValueRequestBody
                    {
                        value = value
                    }).Wait();
            }
        }

        public void Add(HierarchyPath<string> hierarchyPath, object value)
        {
            this.remoteHierarchyAddress
                .AppendPathSegments(hierarchyPath.Items)
                .PostJsonAsync(new HierarchyValueRequestBody
                {
                    value = value
                }).Wait();
        }

        public bool Remove(HierarchyPath<string> hierarchyPath)
        {
            this.remoteHierarchyAddress
                .AppendPathSegments(hierarchyPath.Items)
                .DeleteAsync().Wait();
            return true;
        }

        public bool TryGetValue(HierarchyPath<string> hierarchyPath, out object value)
        {
            var responseData = this.remoteHierarchyAddress
                .AppendPathSegments(hierarchyPath.Items)
                .GetJsonAsync<HierarchyValueBody>().Result;
            value = responseData.value;
            return true;
        }

        #region Hierarchy traversal

        public IHierarchyNode<string, object> Traverse(HierarchyPath<string> path)
        {
            var nodesCollection = this.remoteHierarchyAddress
                .AppendPathSegment("nodes")
                .AppendPathSegment(path.IsRoot ? "root" : path.ToString())
                .AppendPathSegment("children")
                .GetJsonAsync<HierarchyNodeCollectionBody>()
                .Result;

            var partialHierachySnapshot = new MutableHierarchy<string, object>();

            foreach (var node in nodesCollection.nodes)
            {
                if (string.IsNullOrEmpty(node.path))
                    partialHierachySnapshot.Add(HierarchyPath.Create<string>(),null);
                else
                    partialHierachySnapshot.Add(HierarchyPath.Parse(node.path,"/"), null);
            }

            return partialHierachySnapshot.Traverse(path);
        }

        #endregion Hierarchy traversal
    }
}