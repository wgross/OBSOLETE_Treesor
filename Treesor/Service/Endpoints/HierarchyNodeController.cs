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

        [HttpGet, Route("api/node")]
        public IHttpActionResult Get()
        {
            return Ok(new HierarchyNodeCollectionBody
            {
                nodes = this.treesorService.Descendants().Select(kv => new HierarchyNodeBody
                {
                    path = kv.Key.ToString()
                }).ToArray()
            });
        }
    }
}