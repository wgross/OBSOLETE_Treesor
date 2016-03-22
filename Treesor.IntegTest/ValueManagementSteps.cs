using NUnit.Framework;
using RestSharp;
using System;
using TechTalk.SpecFlow;
using Treesor.Service.Endpoints;

namespace Treesor.IntegTest
{
    [Binding]
    public class ValueManagementSteps
    {
        private RestClient client;
        private IRestResponse<HierarchyNodeBody> readPathResponse;
        private string path(string p) => p == "root-path" ? string.Empty : p;

        [Given]
        public void Given_treesor_is_running_at_HOST_and_PORT(string host, int port)
        {
            this.client = new RestClient($"http://{host}:{port}/api/{{path}}");
        }

        [Given]
        public void Given_I_store_VALUE_at_hierarchy_position_PATH(string value, string path)
        {
            var response = this.client.Post<HierarchyNodeBody>(new RestRequest()
                .AddUrlSegment("path", this.path(path))
                .AddJsonBody(new HierarchyNodeRequestBody
            {
                Value = value
            }));


            Assert.AreEqual(value, response.Data.Value);
            Assert.AreEqual(this.path(path), response.Data.Path);
        }
        
        [When]
        public void When_I_read_PATH(string path)
        {
            this.readPathResponse = this.client.Get<HierarchyNodeBody>(new RestRequest()
                .AddUrlSegment("path", this.path(path)));
        }
        
        [Then]
        public void Then_the_response_contains_PATH_and_VALUE(string path, string value)
        {
            Assert.AreEqual(value, this.readPathResponse.Data.Value);
            Assert.AreEqual(this.path(path), this.readPathResponse.Data.Path);
        }
    }
}
