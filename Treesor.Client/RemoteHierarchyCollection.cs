using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Flurl;
using Flurl.Http;
using Treesor.Service.Endpoints;

namespace Treesor.Client
{
    public class RemoteHierarchy : IHierarchy<string, object>
    {
        private string remoteHierarchyAddress = "http://localhost:9002/api";

        public object this[HierarchyPath<string> hierarchyPath]
        {
            set
            {
                this.remoteHierarchyAddress
                    .AppendPathSegments(hierarchyPath.Items)
                    .PutJsonAsync(new HierarchyNodeRequestBody
                    {
                        value = value
                    }).Wait();
            }
        }

        public void Add(HierarchyPath<string> hierarchyPath, object value)
        {
            this.remoteHierarchyAddress
                .AppendPathSegments(hierarchyPath.Items)
                .PostJsonAsync(new HierarchyNodeRequestBody
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
                .GetJsonAsync<HierarchyNodeBody>().Result;
            value = responseData.value;
            return true;
        }
    }
}