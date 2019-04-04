using System;
using System.Configuration;
using System.Linq;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Tooling.Connector;

namespace TestFluentCRM
{
    /// <summary>
    ///  Validate the operation of the clearing of a field in FakeXrmEasy vs real CRM
    /// </summary>
    [TestClass]
    public class TestClear
    {
        [TestMethod]
        [Ignore] // Doesn't work with Fakes (ATM)
        public void TestClearFake()
        {
            var context = TestUtilities.TestContext2();
            var contact1 = context.Data["contact"].First().Value;
            Assert.IsTrue(contact1.Attributes.ContainsKey("telephone1"));
            var tele1 = contact1.Attributes["telephone1"];

            FluentContact.Contact(contact1.Id, context.GetOrganizationService())
                .Clear("telephone1")
                .Execute();

            Assert.IsFalse(contact1.Attributes.ContainsKey("telephone1"));
            var tele2 = contact1.GetAttributeValue<string>("telephone1");
            Assert.IsNull(tele2);
        }

        [TestMethod]
        [Ignore]
        public void TestClearSimple()
        {
            var cnString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;
            using (var crmSvc = new CrmServiceClient(cnString))
            {
                var context = TestUtilities.TestContext2();

                #region "Real CRM"

                //var orgService = crmSvc.OrganizationServiceProxy;
                //var contactId = Guid.Parse("273D526C-1B23-E911-A977-0022480149C2");

                #endregion

                #region "Fake CRM"

                // Faked CRM - fails
                var orgService = context.GetOrganizationService();
                var contactId = context.Data["contact"].First().Key;

                #endregion

                var result = orgService.Retrieve("contact", contactId, new ColumnSet("telephone1", "mobilephone"));
                Assert.IsNotNull(result);
                Assert.IsTrue(result.Attributes.ContainsKey("telephone1"));
                Assert.IsTrue(result.Attributes.ContainsKey("mobilephone"));

                Entity contact = new Entity("contact", contactId);

                contact.Attributes["telephone1"] = null;

                orgService.Update(contact);

                result = orgService.Retrieve("contact", contactId, new ColumnSet("telephone1", "mobilephone"));
                Assert.IsNotNull(result);
                Assert.IsFalse(result.Attributes.ContainsKey("telephone1"));
                Assert.IsTrue(result.Attributes.ContainsKey("mobilephone"));

            }
        }

        [TestMethod]
        public void TestDelete()
        {

        }

        [TestMethod]
        public void TestJoin()
        {

        }

        [TestMethod]
        public void TestBeforeEachRecord()
        {

        }

        [TestMethod]
        public void TestAfterEachRecord()
        {

        }

        [TestMethod]
        public void TestExecute()
        {

        }

    }
}
