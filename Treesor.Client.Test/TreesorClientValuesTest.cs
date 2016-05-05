using Elementary.Hierarchy;
using Flurl.Http;
using Flurl.Http.Testing;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using Treesor.Service.Endpoints;

namespace Treesor.Client.Test
{
    [TestFixture]
    public class TreesorClientValuesTest
    {
        private HttpTest httpTest;
        private RemoteHierarchy remoteHierarchy;

        [SetUp]
        public void ArrangeAllTests()
        {
            this.httpTest = new HttpTest();
            this.remoteHierarchy = new RemoteHierarchy("http://localhost:9002/api/v1");
        }

        [TearDown]
        public void CleanupAllTests()
        {
            this.httpTest.Dispose();
        }

        #region Add/Set/TryGetValue(/)

        [Test]
        public void Add_value_at_remote_hierarchy_root_fails_with_InvalidOperationException()
        {
            // ARRANGE

            this.httpTest
                .RespondWith(500, string.Empty);

            // ACT

            var result = Assert.Throws<AggregateException>(() => this.remoteHierarchy.Add(HierarchyPath.Create<string>(), "value"));

            // ASSERT

            Assert.IsInstanceOf<FlurlHttpException>(result.InnerException);
            Assert.AreEqual(HttpStatusCode.InternalServerError, ((FlurlHttpException)result.InnerException).Call.HttpStatus);

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api")
                .WithVerb(HttpMethod.Post)
                .WithContentType("application/json")
                .With(c => c.RequestBody.Contains("\"value\":\"value\""));
        }

        public void Set_value_at_remote_hierarchy_root()
        {
            // ARRANGE

            this.httpTest
                .RespondWith(500, string.Empty);

            // ACT

            var result = Assert.Throws<AggregateException>(() => this.remoteHierarchy[HierarchyPath.Create<string>()] = "value");

            // ASSERT

            Assert.IsInstanceOf<FlurlHttpException>(result.InnerException);
            Assert.AreEqual(HttpStatusCode.InternalServerError, ((FlurlHttpException)result.InnerException).Call.HttpStatus);

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api")
                .WithVerb(HttpMethod.Put)
                .WithContentType("application/json")
                .With(c => c.RequestBody.Contains("\"value\":\"value\""));
        }

        [Test]
        public void Get_value_from_remote_hierarchy_root()
        {
            // ARRANGE

            this.httpTest
                .RespondWith(500, string.Empty);

            // ACT

            object value;
            var result = Assert.Throws<AggregateException>(() => this.remoteHierarchy.TryGetValue(HierarchyPath.Create<string>(), out value));

            // ASSERT

            Assert.IsInstanceOf<FlurlHttpException>(result.InnerException);
            Assert.AreEqual(HttpStatusCode.InternalServerError, ((FlurlHttpException)result.InnerException).Call.HttpStatus);

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api")
                .WithVerb(HttpMethod.Get);
        }

        [Test]
        public void Remove_value_from_remote_hierarchy_root()
        {
            // ARRANGE

            this.httpTest
                .RespondWith(500, string.Empty);

            // ACT

            var result = Assert.Throws<AggregateException>(() => this.remoteHierarchy.Remove(HierarchyPath.Create<string>(), null));

            // ASSERT

            Assert.IsInstanceOf<FlurlHttpException>(result.InnerException);
            Assert.AreEqual(HttpStatusCode.InternalServerError, ((FlurlHttpException)result.InnerException).Call.HttpStatus);

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/v1/values")
                .WithVerb(HttpMethod.Delete);
        }

        [Test]
        public void Remove_value_recursive_from_remote_hierarchy_root_returns_true()
        {
            // ARRANGE

            this.httpTest
                .RespondWith((int)HttpStatusCode.OK, string.Empty);

            // ACT

            var result = this.remoteHierarchy.Remove(HierarchyPath.Create<string>(), int.MaxValue);

            // ASSERT

            Assert.IsTrue(result);

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/v1/values?$expand=2147483647")
                .WithVerb(HttpMethod.Delete);
        }

        [Test]
        public void Remove_value_recursive_from_remote_hierarchy_root_returns_false()
        {
            // ARRANGE

            this.httpTest
                .RespondWith((int)HttpStatusCode.NotModified, string.Empty);

            // ACT

            var result = this.remoteHierarchy.Remove(HierarchyPath.Create<string>(), int.MaxValue);

            // ASSERT

            Assert.IsFalse(result);

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/v1/values?$expand=2147483647")
                .WithVerb(HttpMethod.Delete);
        }

        #endregion Add/Set/TryGetValue(/)

        [Test]
        public void Add_value_at_remote_hierarchy_node()
        {
            // ARRANGE

            // ACT

            this.remoteHierarchy.Add(HierarchyPath.Create("a"), "value");

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/v1/values/a")
                .WithVerb(HttpMethod.Post)
                .WithContentType("application/json")
                .With(c => c.RequestBody.Contains("\"value\":\"value\""));
        }

        [Test]
        public void Set_value_at_remote_hierarchy_node()
        {
            // ARRANGE

            // ACT

            this.remoteHierarchy[HierarchyPath.Create("a")] = "value";

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/v1/values/a")
                .WithVerb(HttpMethod.Put)
                .WithContentType("application/json")
                .With(c => c.RequestBody.Contains("\"value\":\"value\""));
        }

        [Test]
        public void Set_value_null_at_remote_hierarchy_node()
        {
            // ARRANGE

            // ACT

            this.remoteHierarchy[HierarchyPath.Create("a")] = null;

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/v1/values/a")
                .WithVerb(HttpMethod.Put)
                .WithContentType("application/json")
                .With(c => c.RequestBody.Contains("\"value\":null"));
        }

        [Test]
        public void Get_value_from_remote_hierarchy_node()
        {
            // ARRANGE

            this.httpTest.RespondWithJson(new HierarchyValueBody
            {
                path = null,
                value = "value"
            });

            // ACT

            object value;
            var result = this.remoteHierarchy.TryGetValue(HierarchyPath.Create("a"), out value);

            // ASSERT

            Assert.IsTrue(result);
            Assert.AreEqual("value", value);

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/v1/values/a")
                .WithVerb(HttpMethod.Get);
        }

        [Test]
        public void Remove_value_from_remote_hierarchy_node()
        {
            // ACT

            var result = this.remoteHierarchy.Remove(HierarchyPath.Create("a"), null);

            // ASSERT

            Assert.IsTrue(result);

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api/v1/values/a")
                .WithVerb(HttpMethod.Delete);
        }
    }
}