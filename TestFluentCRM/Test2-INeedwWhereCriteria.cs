using System;
using FakeXrmEasy;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TestFluentCRM
{
    [TestClass]
    public class Test2_INeedwWhereCriteria
    {

        private XrmFakedContext _context;
        private IOrganizationService _orgService;

        [TestInitialize]
        public void SetUp()
        {
            _context = TestUtilities.TestContext1();
            _orgService = _context.GetOrganizationService();
            FluentCRM.FluentCRM.StaticService = _orgService;
        }

        [DataRow("name", "Account1", 1)]
        [DataRow("address1_country", "UK", 2)]
        [DataRow("address1_country", "US", 2)]
        [DataRow("address1_country", "FR", 0)]
        [DataRow("statecode", 0, 4)]
        [DataRow("statecode", 1, 0)]
        [DataTestMethod]
        public void TestEqual1(string attributeName, object attributeValue, int expectedCount)
        {
            int? count = 0;
            FluentAccount.Account().Where(attributeName).Equals(attributeValue).Count(c => count = c).Execute();
            Assert.AreEqual(expectedCount, count);
        }

        [DataRow("name", "Account1", 3)]
        [DataRow("address1_country", "UK", 2)]
        [DataRow("address1_country", "US", 2)]
        [DataRow("address1_country", "FR", 4)]
        [DataRow("statecode", 0, 0)]
        [DataRow("statecode", 1, 4)]
        [DataTestMethod]
        public void TestNotEqual1(string attributeName, object attributeValue, int expectedCount)
        {
            int? count = 0;
            FluentAccount.Account().Where(attributeName).NotEqual(attributeValue).Count(c => count = c).Execute();
            Assert.AreEqual(expectedCount, count);
        }

        [DataRow("name", 4)]
        [DataRow("name3", 1)]
        [DataRow("address1_country", 4)]
        [DataRow("statecode", 4)]
        [DataTestMethod]
        public void TestNotNull1(string attributeName, int expectedCount)
        {
            int? count = 0;
            FluentAccount.Account().Where(attributeName).IsNotNull.Count(c => count = c).Execute();
            Assert.AreEqual(expectedCount, count);
        }

        [DataRow("name", 0)]
        [DataRow("name3", 3)]
        [DataRow("address1_country", 0)]
        [DataRow("statecode", 0)]
        [DataTestMethod]
        public void TestIsNull1(string attributeName, int expectedCount)
        {
            int? count = 0;
            FluentAccount.Account().Where(attributeName).IsNull.Count(c => count = c).Execute();
            Assert.AreEqual(expectedCount, count);
        }

        [DataRow("name", new String[] {"Account1", "Account2"}, 2)]
        [DataRow("address1_country", new string[] {"UK"}, 2)]
        [DataRow("address1_country", new string[] {"UK", "FR"}, 2)]
        [DataRow("address1_country", new string[] {"FR", "IT"}, 0)]
        [DataRow( "statecode", new int[] {0,1}, 4)]
        [DataTestMethod]
        public void TestIn1( string attributeName, object attributeValues, int expectedCount)
        {
            int? count = 0;
            FluentAccount.Account().Where(attributeName).In(attributeValues).Count( c => count=c).Execute();
            Assert.AreEqual(expectedCount, count);
        }

        [DataRow("name", "Account1", 3)]
        [DataRow("address1_country", "UK", 2)]
        [DataRow("address1_country", "US", 0)]
        [DataRow("address1_country", "AA", 4)]
        [DataRow("statecode", 0, 0)]
        [DataRow("statecode", -1, 4)]
        [DataTestMethod]
        public void TestGreaterThan1(string attributeName, object attributeValue, int expectedCount)
        {
            int? count = 0;
            FluentAccount.Account().Where(attributeName).GreaterThan(attributeValue).Count(c => count = c).Execute();
            Assert.AreEqual(expectedCount, count);
        }

        [DataRow("name", "Account2", 1)]
        [DataRow("address1_country", "UK", 0)]
        [DataRow("address1_country", "US", 2)]
        [DataRow("address1_country", "ZZ", 4)]
        [DataRow("statecode", 0, 0)]
        [DataRow("statecode", 1, 4)]
        [DataTestMethod]
        public void TestLessThan1(string attributeName, object attributeValue, int expectedCount)
        {
            int? count = 0;
            FluentAccount.Account().Where(attributeName).LessThan(attributeValue).Count(c => count = c).Execute();
            Assert.AreEqual(expectedCount, count);
        }

        [DataRow("name",  ConditionOperator.LessThan, "Account2", 1)]
        [DataRow("address1_country",ConditionOperator.LessThan, "UK", 0)]
        [DataRow("address1_country", ConditionOperator.LessThan, "US", 2)]
        [DataRow("address1_country", ConditionOperator.LessThan, "ZZ", 4)]
        [DataRow("statecode", ConditionOperator.LessThan, 0, 0)]
        [DataRow("statecode", ConditionOperator.LessThan, 1, 4)]
        [DataTestMethod]
        public void TestCondition1(string attributeName, ConditionOperator op, object attributeValue, int expectedCount)
        {
            int? count = 0;
            FluentAccount.Account().Where(attributeName).Condition(op, attributeValue).Count(c => count = c).Execute();
            Assert.AreEqual(expectedCount, count);
        }
    }
}
