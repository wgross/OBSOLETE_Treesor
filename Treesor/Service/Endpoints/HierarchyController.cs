using System.Web.Http;

namespace Treesor.Service.Endpoints
{
    public class HierarchyController : ApiController
    {
        [HttpGet, Route("api/{*path}")]
        public IHttpActionResult Get([FromUri] string path)
        {
            return Ok(new { result = path ?? "empty" });
        }
    }
}