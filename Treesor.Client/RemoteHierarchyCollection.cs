using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using Flurl;
using Flurl.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
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
                    .AppendPathSegment("values")
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
                .AppendPathSegment("values")
                .AppendPathSegments(hierarchyPath.Items)
                .PostJsonAsync(new HierarchyValueRequestBody
                {
                    value = value
                }).Wait();
        }

        public bool Remove(HierarchyPath<string> hierarchyPath, int? maxDepth)
        {
            var query = this.remoteHierarchyAddress
                .AppendPathSegment("values")
                .AppendPathSegments(hierarchyPath.Items);

            if (maxDepth != null)
            {
                query = query.SetQueryParam("$expand", $"{int.MaxValue}");
            }

            return query
                .AllowHttpStatus(HttpStatusCode.OK, HttpStatusCode.NotModified)
                .DeleteAsync().Result.StatusCode == HttpStatusCode.OK;
        }

        public bool TryGetValue(HierarchyPath<string> hierarchyPath, out object value)
        {
            var responseData = this.remoteHierarchyAddress
                .AppendPathSegment("values")
                .AppendPathSegments(hierarchyPath.Items)
                .GetJsonAsync<HierarchyValueBody>().Result;
            value = responseData.value;
            return true;
        }

        #region Hierarchy traversal

        private sealed class RemoteTraverser : IHierarchyNode<string, object>
        {
            private readonly Lazy<IEnumerable<IHierarchyNode<string, object>>> childNodes;
            private readonly HierarchyPath<string> path;
            private readonly RemoteHierarchy remoteHierarchy;

            public RemoteTraverser(RemoteHierarchy remoteHierarchy, HierarchyPath<string> path)
            {
                this.remoteHierarchy = remoteHierarchy;
                this.path = path;
                this.childNodes = new Lazy<IEnumerable<IHierarchyNode<string, object>>>(() => this.MapNodeToTraverser(this.remoteHierarchy.GetChildNodes(path).nodes), mode: LazyThreadSafetyMode.None);
            }

            private IEnumerable<RemoteTraverser> MapNodeToTraverser(IEnumerable<HierarchyNodeBody> nodes)
            {
                return nodes.Select(n => new RemoteTraverser(this.remoteHierarchy, HierarchyPath.Parse(n.path, "/")));
            }

            public IEnumerable<IHierarchyNode<string, object>> ChildNodes => this.childNodes.Value;

            public bool HasChildNodes => this.childNodes.Value.Any();

            public bool HasParentNode
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool HasValue
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IHierarchyNode<string, object> ParentNode
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public HierarchyPath<string> Path => this.path;

            public object Value
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }

        public IHierarchyNode<string, object> Traverse(HierarchyPath<string> path)
        {
            return new RemoteTraverser(this, path);

            //HierarchyNodeCollectionBody nodesCollection = this.GetChildNodes(path);
            //return new Traverser
            //var partialHierachySnapshot = new MutableHierarchy<string, object>();

            //foreach (var node in nodesCollection.nodes)
            //{
            //    if (string.IsNullOrEmpty(node.path))
            //        partialHierachySnapshot.Add(HierarchyPath.Create<string>(), null);
            //    else
            //        partialHierachySnapshot.Add(HierarchyPath.Parse(node.path, "/"), null);
            //}

            //return partialHierachySnapshot.Traverse(path);
        }

        private HierarchyNodeCollectionBody GetChildNodes(HierarchyPath<string> path)
        {
            return this.remoteHierarchyAddress
                            .AppendPathSegment("nodes")
                            .AppendPathSegment(path.IsRoot ? "root" : path.ToString())
                            .AppendPathSegment("children")
                            .GetJsonAsync<HierarchyNodeCollectionBody>()
                            .Result;
        }

        #endregion Hierarchy traversal
    }
}