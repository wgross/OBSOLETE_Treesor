using Elementary.Hierarchy;
using System;
using System.Web.Http;
using Treesor.Application;

namespace Treesor.Service.Endpoints
{
    public class HierarchyController : ApiController
    {
        public HierarchyController()
            : this(service:new Treesor.Application.TreesorService())
        {}

        public HierarchyController(ITreesorService service)
        {
            this.service = service;
        }

        private readonly ITreesorService service;

        [HttpGet, Route("api/{*path}")]
        public IHttpActionResult Get([FromUri] string path)
        {
            return Ok(new { result = path ?? "empty" });
        }

        [HttpPost, Route("api/{*path}")]
        public IHttpActionResult Post([FromUri] string path, [FromBody]HierarchyNodeRequestBody body)
        {
            this.service.SetValue(HierarchyPath.Parse(path,"/"), body.Value);

            var response = new HierarchyNodeBody
            {
                Path= path,
                Value = body.Value
            };

            return CreatedAtRoute("NodeAccess", new { path = path }, response);
        }
    }
}