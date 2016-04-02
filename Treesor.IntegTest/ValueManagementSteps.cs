using NUnit.Framework;
using RestSharp;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using TechTalk.SpecFlow;
using Treesor.Service.Endpoints;

namespace Treesor.IntegTest
{
    [Binding]
    public class ValueManagementSteps
    {
        private RestClient client;
        private IRestResponse<HierarchyNodeBody> readPathResponse;
        private IRestResponse deletePathResponse;
        private IRestResponse<HierarchyNodeBody> putPathResponse;

        private string path(string p) => p == "root-path" ? string.Empty : p;

        private static Process EnsureTreesorProcess()
        {
            lock (typeof(ValueManagementSteps))
            {
                var treesorProcess = Process.GetProcessesByName("Treesor").FirstOrDefault();
                if (treesorProcess == null)
                {
                    var location = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var commonRoot = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(location))));

                    treesorProcess = Process.Start(Path.Combine(commonRoot, "Treesor/bin/Debug/Treesor.exe"));
                }
                return treesorProcess;
            }
        }

        [Given]
        public void Given_treesor_is_running_at_HOST_and_PORT(string host, int port)
        {
            this.client = new RestClient($"http://{host}:{port}/api/{{path}}");

            Assert.IsNotNull(EnsureTreesorProcess());
            Assert.IsNotNull(this.client);
        }

        [Given]
        public void Given_I_create_VALUE_at_hierarchy_position_PATH(string value, string path)
        {
            var response = this.client.Post<HierarchyNodeBody>(new RestRequest()
                .AddUrlSegment("path", this.path(path))
                .AddJsonBody(new HierarchyNodeRequestBody
                {
                    value = value
                }));

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
            Assert.AreEqual(value, response.Data.value);
            Assert.AreEqual(this.path(path), response.Data.path);
        }

        [When]
        public void When_I_read_PATH(string path)
        {
            this.readPathResponse = this.client.Get<HierarchyNodeBody>(new RestRequest()
                .AddUrlSegment("path", this.path(path)));
        }

        [When]
        public void When_I_delete_at_hierarchy_position_PATH(string path)
        {
            this.deletePathResponse = this.client.Delete(new RestRequest()
                .AddUrlSegment("path", this.path(path)));
        }

        [When]
        public void When_I_update_with_NEWVALUE_at_hierarchy_position_PATH(string newValue, string path)
        {
            this.putPathResponse = this.client.Put<HierarchyNodeBody>(new RestRequest()
                .AddUrlSegment("path", path)
                .AddJsonBody(new { Value = (object)newValue }));
        }

        [Then]
        public void Then_Read_response_contains_PATH_and_VALUE(string path, string value)
        {
            Assert.AreEqual(value, this.readPathResponse.Data.value);
            Assert.AreEqual(this.path(path), this.readPathResponse.Data.path);
        }

        [Then]
        public void Then_Reading_at_a_b_fails_with_P0(int p0)
        {
            ScenarioContext.Current.Pending();
        }

        [Then]
        public void Then_read_response_is_STATUSCODE(int statusCode)
        {
            Assert.AreEqual(statusCode, (int)this.readPathResponse.StatusCode);
        }

        [Then]
        public void Then_delete_response_is_STATUSCODE(int statusCode)
        {
            Assert.AreEqual(statusCode, (int)this.deletePathResponse.StatusCode);
        }

        [Then]
        public void Then_update_response_is_STATUSCODE(int statusCode)
        {
            Assert.AreEqual(statusCode, (int)this.putPathResponse.StatusCode);
        }

        [Then]
        public void Then_update_result_contains_PATH_and_NEWVALUE(string path, string newValue)
        {
            Assert.IsNotNull(this.putPathResponse);
            Assert.AreEqual(this.path(path), this.putPathResponse.Data.path);
            Assert.AreEqual(newValue, this.putPathResponse.Data.value);
        }
    }
}