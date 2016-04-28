using Elementary.Hierarchy;
using Elementary.Hierarchy.Collections;
using System;
using System.Web.Http;
using Treesor.Application;

namespace Treesor.Service.Endpoints
{
    public class HierarchyValueController : ApiController
    {
        #region Construction and Initialization of this instance

        private static readonly MutableHierarchy<string, TreesorNodeValueBase> defaultHierarchy = new MutableHierarchy<string, TreesorNodeValueBase>();

        public HierarchyValueController()
            : this(defaultHierarchy)
        { }

        public HierarchyValueController(MutableHierarchy<string, TreesorNodeValueBase> hierarchy)
            : this(service: new Treesor.Application.TreesorService(hierarchy))
        {
        }

        public HierarchyValueController(ITreesorService service)
        {
            this.service = service;
        }

        private readonly ITreesorService service;

        #endregion Construction and Initialization of this instance
        
        /// <summary>
        /// Retrieves a Hierachy node value
        /// </summary>
        /// <returns>Http 'Ok' with the value of the node or 'NotFound'</returns>
        [HttpGet, Route("api/v1/values/")]
        public IHttpActionResult Get()
        {
            return this.InternalServerError(new InvalidOperationException("Root may not have a value"));

            //object value;
            //if (this.service.TryGetValue(HierarchyPath.Create<string>(), out value))
            //    return this.Ok(new HierarchyValueBody
            //    {
            //        path = string.Empty,
            //        value = value
            //    });

            //return this.NotFound();
        }

        /// <summary>
        /// Retrieves a Hierachy node value
        /// </summary>
        /// <returns>Http 'Ok' with the value of the node or 'NotFound'</returns>
        [HttpGet, Route("api/v1/values/{*path}")]
        public IHttpActionResult Get([FromUri] string path)
        {
            if (string.IsNullOrEmpty(path))
                return this.InternalServerError(new ArgumentException("Path may not be null or empty"));

            TreesorNodeValueBase value;
            if (this.service.TryGetValue(HierarchyPath.Parse(path, "/"), out value))
                return this.Ok(new HierarchyValueBody
                {
                    path = path,
                    value = value
                });

            return this.NotFound();
        }

        [HttpPost, Route("api/v1/values", Name = "SetValueAtRoot")]
        public IHttpActionResult Post([FromBody]HierarchyValueRequestBody body)
        {
            if (body.value != null)
                return this.InternalServerError(new InvalidOperationException("Root may not have a value"));

            this.service.SetValue(HierarchyPath.Create<string>(), new TreesorNodeValue(body.value));

            var response = new HierarchyValueBody
            {
                path = string.Empty,
                value = body.value
            };

            return this.CreatedAtRoute("SetValueAtRoot", new { path = string.Empty }, response);
        }

        [HttpPost, Route("api/v1/values/{*path}", Name = "SetValue")]
        public IHttpActionResult Post([FromUri] string path, [FromBody]HierarchyValueRequestBody body)
        {
            if (string.IsNullOrEmpty(path))
                return this.InternalServerError(new ArgumentException("Path may not be null or empty"));

            this.service.SetValue(HierarchyPath.Parse(path, "/"), new TreesorNodeValue(body.value));

            var response = new HierarchyValueBody
            {
                path = path,
                value = body.value
            };

            return this.CreatedAtRoute("SetValue", new { path = path }, response);
        }

        [HttpDelete, Route("api/v1/values")]
        public IHttpActionResult Delete([FromUri(Name="$expand")] int? expand=null)
        {
            if (expand == null || expand == 1)
                return this.InternalServerError(new InvalidOperationException("Root may not have a value"));

            this.service.RemoveValue(HierarchyPath.Create<string>(), expand.GetValueOrDefault(1));
            return this.Ok();
        }

        [HttpDelete, Route("api/v1/values/{*path}")]
        public IHttpActionResult Delete([FromUri] string path, [FromUri(Name = "$expand")] int? expand = null)
        {
            if (string.IsNullOrEmpty(path))
                return this.InternalServerError(new ArgumentException("Path may not be null or empty"));

            this.service.RemoveValue(HierarchyPath.Parse(path, "/"),expand.GetValueOrDefault(1));
            return this.Ok();
        }

        [HttpPut, Route("api/v1/values")]
        public IHttpActionResult Put([FromBody] HierarchyValueRequestBody value)
        {
            return this.InternalServerError(new InvalidOperationException("Root may not have a value"));

            //this.service.SetValue(HierarchyPath.Create<string>(), value.value);

            //return this.Ok(new HierarchyValueBody { value = value.value, path = string.Empty });
        }

        [HttpPut, Route("api/v1/values/{*path}")]
        public IHttpActionResult Put([FromUri] string path, [FromBody] HierarchyValueRequestBody value)
        {
            if (string.IsNullOrEmpty(path))
                return this.InternalServerError(new ArgumentException("Path may not be null or empty"));

            this.service.SetValue(HierarchyPath.Parse(path, "/"), new TreesorNodeValue(value.value));

            return this.Ok(new HierarchyValueBody { value = value.value, path = path });
        }
    }
}