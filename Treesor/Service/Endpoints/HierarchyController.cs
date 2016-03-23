using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using System;
using System.Web.Http;
using Treesor.Application;

namespace Treesor.Service.Endpoints
{
    public class HierarchyController : ApiController
    {
        private static readonly MutableHierarchy<string, object> defaultHierarchy = new MutableHierarchy<string, object>();

        public HierarchyController()
            : this(defaultHierarchy)
        { }

        public HierarchyController(MutableHierarchy<string, object> hierarchy)
            : this(service: new Treesor.Application.TreesorService(hierarchy))
        {
        }

        public HierarchyController(ITreesorService service)
        {
            this.service = service;
        }

        private readonly ITreesorService service;

        [HttpGet, Route("api")]
        public IHttpActionResult Get()
        {
            object value;
            if (this.service.TryGetValue(HierarchyPath.Create<string>(), out value))
                return this.Ok(new HierarchyNodeBody
                {
                    Path = string.Empty,
                    Value = value
                });

            return this.NotFound();
        }

        [HttpGet, Route("api/{*path}")]
        public IHttpActionResult Get([FromUri] string path)
        {
            if (string.IsNullOrEmpty(path))
                return this.InternalServerError(new ArgumentException("Path may not be null or empty"));

            object value;
            if (this.service.TryGetValue(HierarchyPath.Parse(path, "/"), out value))
                return this.Ok(new HierarchyNodeBody
                {
                    Path = path,
                    Value = value
                });

            return this.NotFound();
        }

        [HttpPost, Route("api", Name = "SetValueAtRoot")]
        public IHttpActionResult Post([FromBody]HierarchyNodeRequestBody body)
        {
            this.service.SetValue(HierarchyPath.Create<string>(), body.Value);

            var response = new HierarchyNodeBody
            {
                Path = string.Empty,
                Value = body.Value
            };

            return this.CreatedAtRoute("SetValueAtRoot", new { path = string.Empty }, response);
        }

        [HttpPost, Route("api/{*path}", Name = "SetValue")]
        public IHttpActionResult Post([FromUri] string path, [FromBody]HierarchyNodeRequestBody body)
        {
            if (string.IsNullOrEmpty(path))
                return this.InternalServerError(new ArgumentException("Path may not be null or empty"));

            this.service.SetValue(HierarchyPath.Parse(path, "/"), body.Value);

            var response = new HierarchyNodeBody
            {
                Path = path,
                Value = body.Value
            };

            return this.CreatedAtRoute("SetValue", new { path = path }, response);
        }

        [HttpDelete, Route("api")]
        public IHttpActionResult Delete()
        {
            this.service.RemoveValue(HierarchyPath.Create<string>());
            return this.Ok();
        }

        [HttpDelete, Route("api/{*path}")]
        public IHttpActionResult Delete([FromUri] string path)
        {
            if (string.IsNullOrEmpty(path))
                return this.InternalServerError(new ArgumentException("Path may not be null or empty"));

            this.service.RemoveValue(HierarchyPath.Parse(path, "/"));
            return this.Ok();
        }

        [HttpPut, Route("api")]
        public IHttpActionResult Put([FromBody] HierarchyNodeRequestBody value)
        {
            this.service.SetValue(HierarchyPath.Create<string>(), value.Value);

            return this.Ok(new HierarchyNodeBody { Value = value.Value, Path = string.Empty });
        }

        [HttpPut, Route("api/{*path}")]
        public IHttpActionResult Put([FromUri] string path , [FromBody] HierarchyNodeRequestBody value)
        {
            if (string.IsNullOrEmpty(path))
                return this.InternalServerError(new ArgumentException("Path may not be null or empty"));

            this.service.SetValue(HierarchyPath.Parse(path, "/"), value.Value);

            return this.Ok(new HierarchyNodeBody { Value = value.Value, Path = path });
        }
    }
}