using System;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestFluentCRM
{
    [TestClass]
    public class Test0_Extract
    {
        [TestMethod]
        public void TestUseAttribute1()
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

        /// <summary>
        /// Validate use of default values.
        /// </summary>
                [TestMethod]
        public void TestUseAttribute2()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;

            FluentAccount.Account(account1.Id, context.GetOrganizationService())
                .Trace( s => Debug.WriteLine(s))
                .UseAttribute( "default", (string s) => Assert.AreEqual("default", s), "namemissing")
                .UseAttribute( "defphone", (string s) => Assert.AreEqual("defphone", s), "phone1missing")
                .UseAttribute(5, (int s) => Assert.AreEqual(5, s), "statecodemissing")
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

        /// <summary>
        /// This does not work if run at the same time as other unit tests due to coupling via StaticService.
        /// </summary>
        [TestMethod]
        public void TestMissingOrgService()
        {
            var contactId = Guid.NewGuid();
            dynamic dto = new ExpandoObject();
            FluentCRM.FluentCRM.StaticService = null;  // Side effect from other tests which may set StaticService value.

            Assert.ThrowsException<ArgumentException>(() =>
                FluentAccount.Account(contactId)
                    .UseAttribute((string account) => dto.name = account, "name")
                    .Join<FluentContact>(
                        c => c.UseAttribute<string>(contact => dto.fullname = contact, "fullname"))
                    .Execute());

        }

        /// <summary>
        /// Test to see if we can use a generic "All" to return all records
        /// </summary>
        [TestMethod]
        public void TestGenericAll()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;

            var a = new FluentAccount();

            a.All(context.GetOrganizationService()).Trace(s => Debug.WriteLine(s) )
                .UseAttribute((string s) => Console.WriteLine($"Read account: s"), "name")
                .Count((c) => Assert.AreEqual(4, c))
                .Execute();
        }

    }
}
