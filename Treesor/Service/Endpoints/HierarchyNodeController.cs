using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace Treesor.Service.Endpoints
{
    public class HierarchyNodeController : ApiController
    {
        [HttpGet, Route("api/node/{path*}")]
        public IHttpActionResult Get()
        {
            return Ok();
        }
    }
}
