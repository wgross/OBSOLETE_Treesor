using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using System;
using System.Web.Http;
using Treesor.Application;

namespace Treesor.Service.Endpoints
{
    public class HierarchyController : ApiController
    {
       private static readonly MutableHierarchy<string, object>  defaultHierarchy = new MutableHierarchy<string,object>();

        public HierarchyController()
            : this(defaultHierarchy)
        {}

        public HierarchyController(MutableHierarchy<string,object> hierarchy)
            : this(service:new Treesor.Application.TreesorService(hierarchy))
        {
        }

        public HierarchyController(ITreesorService service)
        {
            this.service = service;
        }

        private readonly ITreesorService service;

        [HttpGet, Route("api/{*path}")]
        public IHttpActionResult Get([FromUri] string path)
        {
            object value;
            if (this.service.TryGetValue(HierarchyPath.Parse(path,"/"), out value))
                return this.Ok(new HierarchyNodeBody
                {
                    Path = path,
                    Value = value
                });

            return this.NotFound();
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

            return this.CreatedAtRoute("NodeAccess", new { path = path }, response);
        }

        [HttpDelete, Route("api/{*path}")]
        public IHttpActionResult Delete([FromUri] string path)
        {
            this.service.RemoveValue(HierarchyPath.Parse(path, "/"));
            return this.Ok();
        }

    }
}