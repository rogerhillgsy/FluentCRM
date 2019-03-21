using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FakeXrmEasy;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;

namespace TestFluentCRM
{
    [TestClass]
    public class Test1Extract1
    {
        [TestMethod]
        public void TestUseAttribute()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;

            FluentAccount.Account(account1.Id, context.GetOrganizationService())
                .Trace( s => Debug.WriteLine(s))
                .UseAttribute((string s) => Assert.AreEqual(account1["name"], s), "name")
                .UseAttribute((string s) => Assert.AreEqual(account1["phone1"], s), "phone1")
                .UseAttribute((int s) => Assert.AreEqual(account1["statecode"], s), "statecode")
                .Count( (c) => Assert.AreEqual(1, c ))
                .Execute();

        }

        // Test selection of a single entity by Id
        [TestMethod]
        public void TestSelectById()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;

            FluentAccount.Account(account1.Id, context.GetOrganizationService())
                .Trace( s => Debug.WriteLine(s))
                .UseAttribute((string s) => Assert.AreEqual(account1["name"], s), "name")
                .Count( (c) => Assert.AreEqual(1, c ))
                .Execute();
        }

        // Test selection of a single entity by Attribute
        [TestMethod]
        public void TestSelectByAttribute()
        {
            var context = TestUtilities.TestContext1();

            FluentAccount.Account(context.GetOrganizationService())
                .Trace( s => Debug.WriteLine(s))
                .Where("name").Equals("Account2")
                .UseAttribute((string s) => Assert.AreEqual("654321", s), "phone1")
                .Count( (c) => Assert.AreEqual(1, c ))
                .Execute();
        }

        // Test selection of a single entity by Attribute
        [TestMethod]
        public void TestSelectByAttributeMultiple()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;

            FluentAccount.Account(context.GetOrganizationService())
                .Trace( s => Debug.WriteLine(s))
                .Where("address1_country").Equals("UK")
                .UseAttribute((int s) => Assert.AreEqual(0, s), "statecode")
                .Count( (c) => Assert.AreEqual(2, c ))
                .Execute();

        }

        // Test use of static organization context to select by Id
        [TestMethod]
        public void TestStaticSelectById()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            FluentAccount.Account( account1.Id)
                .Trace( s => Debug.WriteLine(s))
                .UseAttribute((string s) => Assert.AreEqual(account1["name"], s), "name")
                .UseAttribute((string s) => Assert.AreEqual(account1["phone1"], s), "phone1")
                .UseAttribute((int s) => Assert.AreEqual(account1["statecode"], s), "statecode")
                .Count( (c) => Assert.AreEqual(1, c ))
                .Execute();
        }

        // Test use of Static organization context to select a single entity by Attribute
        [TestMethod]
        public void TestSelectStaticByAttribute()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            FluentAccount.Account()
                .Trace( s => Debug.WriteLine(s))
                .Where("name").Equals("Account2")
                .UseAttribute((string s) => Assert.AreEqual("654321", s), "phone1")
                .UseAttribute((int s) => Assert.AreEqual(account1["statecode"], s), "statecode")
                .Count( (c) => Assert.AreEqual(1, c ))
                .Execute();

        }


        // Test use of Static organizaion to select multiple entities by Attribute.
        [TestMethod]
        public void TestSelectStaticByAttributeMultiple()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;

            FluentAccount.Account()
                .Trace( s => Debug.WriteLine(s))
                .Where("address1_country").Equals("UK")
                .UseAttribute((int s) => Assert.AreEqual(account1["statecode"], s), "statecode")
                .Count( (c) => Assert.AreEqual(2, c ))
                .Execute();

        }


    }
}
