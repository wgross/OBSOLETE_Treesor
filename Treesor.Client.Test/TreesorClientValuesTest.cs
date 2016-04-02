﻿using Elementary.Hierarchy;
using Flurl.Http.Testing;
using NUnit.Framework;
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
            this.remoteHierarchy = new RemoteHierarchy();
        }

        [TearDown]
        public void CleanupAllTests()
        {
            this.httpTest.Dispose();
        }

        [Test]
        public void Add_value_at_remote_hierachy_root()
        {
            // ARRANGE

            // ACT

            this.remoteHierarchy.Add(HierarchyPath.Create<string>(), "value");

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api")
                .WithVerb(HttpMethod.Post)
                .WithContentType("application/json")
                .With(c => c.RequestBody.Contains("\"value\":\"value\""));
        }

        [Test]
        public void Set_value_at_remote_hierachy_root()
        {
            // ARRANGE

            // ACT

            this.remoteHierarchy[HierarchyPath.Create<string>()] = "value";

            // ASSERT

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api")
                .WithVerb(HttpMethod.Put)
                .WithContentType("application/json")
                .With(c => c.RequestBody.Contains("\"value\":\"value\""));
        }

        [Test]
        public void Get_value_at_remote_hierarchy_root()
        {
            // ARRANGE

            this.httpTest.RespondWithJson(new HierarchyNodeBody
            {
                path = null,
                value = "value"
            });

            // ACT

            object value;
            var result = this.remoteHierarchy.TryGetValue(HierarchyPath.Create<string>(), out value);

            // ASSERT

            Assert.IsTrue(result);
            Assert.AreEqual("value", value);

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api")
                .WithVerb(HttpMethod.Get);
        }

        [Test]
        public void Remove_value_from_remote_hierarchy_root()
        {
            // ACT

            var result = this.remoteHierarchy.Remove(HierarchyPath.Create<string>());

            // ASSERT

            Assert.IsTrue(result);

            this.httpTest
                .ShouldHaveCalled("http://localhost:9002/api")
                .WithVerb(HttpMethod.Delete);
        }
    }
}