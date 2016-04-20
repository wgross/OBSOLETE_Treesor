using Elementary.Hierarchy;
using System.Linq;
using System.Web.Http;
using Treesor.Application;

namespace Treesor.Service.Endpoints
{
    public class HierarchyNodeController : ApiController
    {
        private readonly ITreesorService treesorService;

        public HierarchyNodeController(ITreesorService treesorService)
        {
            this.treesorService = treesorService;
        }

        [HttpGet, Route("api/node/children")]
        public IHttpActionResult GetChildren()
        {
            return Ok(new HierarchyNodeCollectionBody
            {
                nodes = this.treesorService
                    .DescendantsOrSelf(HierarchyPath.Create<string>(), 2)
                    .Select(kv => new HierarchyNodeBody
                    {
                        path = kv.Key.ToString()
                    }).ToArray()
            });
        }

        [HttpGet, Route("api/node/{*path}/children")]
        public IHttpActionResult GetChildren(string path)
        {
            return Ok(new HierarchyNodeCollectionBody
            {
                nodes = this.treesorService
                    .DescendantsOrSelf(HierarchyPath.Parse(path, "/"), 2)
                    .Skip(1) // dont take the start node
                    .Select(kv => new HierarchyNodeBody
                    {
                        path = kv.Key.ToString()
                    }).ToArray()
            });
        }
    }
}