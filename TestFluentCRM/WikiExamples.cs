using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace TestFluentCRM
{
    [TestClass]
    public class WikiExamples
    {
        [TestMethod]
        public void TestLateBindingtFetch()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;
            var telno = string.Empty;

            IOrganizationService orgService = context.GetOrganizationService();
            var cols = new ColumnSet("telephone1", "telephone2", "mobilephone");

            var result = orgService.Retrieve("account", account1.Id, cols);
            if (result != null)
            {
                if (result.Contains("telephone1"))
                {
                    telno = result.GetAttributeValue<string>("telephone1");
                }
                else if (result.Contains("telephone2"))
                {
                    telno = result.GetAttributeValue<string>("telephone");
                }
                else if (result.Contains("mobilephone"))
                {
                    telno = result.GetAttributeValue<string>("mobilephone");
                }
            }

            Debug.WriteLine($"Telephone no is {telno}");
        }

        //[TestMethod]
        //public void TestEarlyBindingtFetch()
        //{
        //    var context = TestUtilities.TestContext1();
        //    var account1 = context.Data["account"].First().Value;
        //    var telno = string.Empty;
        //    var name = string.Empty;

        //    using (var ctx = new DynamicsLinq.OrgServiceContext(context.GetOrganizationService()))
        //    {
        //        var result = from a in ctx.AccountSet
        //            where account1.Id == a.Id
        //            select new {a.Name, a.Telephone1, a.Telephone2, a.Address1_Telephone1};

        //        var account = result.FirstOrDefault();
        //        if (account != null)
        //        {
        //            name = account.Name;
        //            telno = account.Telephone1 ?? account.Telephone2 ?? account.Address1_Telephone1;
        //        }

        //    }

        //    Debug.WriteLine($"Telephone no is {telno}");
        //}

        [TestMethod]
        public void TestFluentCRM()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;
            var telno = string.Empty;

            FluentAccount.Account(account1.Id, context.GetOrganizationService())                   // 1)
                .UseAttribute((string t) => telno = t, "telephone1", "telephone2","mobilephone")   // 2)
                .Execute();                                                                        // 3)

            Debug.WriteLine($"Telephone no is {telno}");
        }

        [TestMethod]
        public void TestFluentCRM2()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;
            var telno = string.Empty;

            FluentAccount.Account()
                .Where("name").Equals("Acme Co")                                                   // 1)
                .UseAttribute((string t) => telno = t, "telephone1", "telephone2","mobilephone")   // 2)
                .Execute();                                                                        // 3)

            Debug.WriteLine($"Telephone no is {telno}");

            var telnos = new List<string>();
            FluentAccount.Account()
                .Where("name").Equals("Acme Co")                                                   // 1)
                .UseAttribute((string t) => telnos.Add(t), "telephone1", "telephone2","mobilephone")   // 2)
                .Execute();                                                                        // 3)


            FluentAccount.Account()
                .Where("name").Equals("Acme Co")
                .UseAttribute((string t) => telno= t, "telephone1", "telephone2","mobilephone")
                .WeakUpdate<string>("telephone1", (s) => telno)
                .Execute();

        }

        [TestMethod]
            public void TestFluentCRM3()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;
            var telno = string.Empty;

            FluentAccount.Account(context.GetOrganizationService())
                .Where("name").Equals("Account1")
                .UseAttribute((string t) => telno= t, "telephone1", "telephone2","mobilephone","phone1")
                .WeakUpdate<string>("telephone1", (s) => telno)
                .Execute();

            Debugger.Break();
            telno = string.Empty;

            FluentAccount.Account(context.GetOrganizationService())
                .Where("name").Equals("Account1")
                .UseAttribute((string t) => telno= t, "telephone1", "telephone2","mobilephone","phone1")
                .WeakUpdate<string>("telephone1",  telno)
                .Execute();

            Debugger.Break();

            FluentAccount.Account(context.GetOrganizationService())
                .Where("createdon").IsNotNull
                .UseAttribute((string t) => telno= t, "telephone1", "telephone2","mobilephone","phone1")
                .WeakUpdate<string>("telephone1",  telno)
                .Execute();

        }

        [TestMethod]
        public void TestFluentCRM4()
        {
            var context = TestUtilities.TestContext1();
            var contact1 = context.Data["contact"].First().Value;
            FluentCRM.FluentCRM.StaticService = context.GetOrganizationService();

            MyStruct dto = new MyStruct
            {
                phoneNumber = "123456",
                emailAddress = "smith@gmail.com",
                name = "John Smith"
            };


            FluentContact.Contact(contact1.Id)
                .WeakUpdate("lastname", (string n) => dto.name )
                .Execute();

            var smithSequence = 0;
            FluentContact.Contact()
                .Where("lastname").Equals("Smith")
                .WeakUpdate("new_smithsequence", (int i) => smithSequence++)
                .Execute();

            FluentContact.Contact(Guid.NewGuid())
                .UseAttribute((string s) => Debug.WriteLine(s), "name")
                .Count(n => Assert.AreEqual(0,n))
                .Execute();
        }

        [TestMethod]
        public void JoinDemo()
        {
            var context = TestUtilities.TestContext1();
            var account1 = context.Data["account"].First().Value;
            var telno = string.Empty;
            var incidentId = Guid.NewGuid();
            var shippingLabelId = Guid.Empty;
            var shippingUrl = string.Empty;

            FluentCase.Case(incidentId, context.GetOrganizationService())
                .Join<FluentAnnotation>(a => a.UseEntity(
                    (EntityWrapper e, string alias) =>
                    {
                        var filename = (string) e["filename"];
                        if (filename?.Contains("Shipping.") ?? false)
                        {
                            shippingLabelId = (Guid) e["annotationid"];
                        }
                    }, "annotationid", "filename"))
                .UseAttribute((string u) => shippingUrl = u, "new_shippinglabelurl")
                .Execute();

            var permittedImageIds = new List<Guid>();

            //// Add Application attachments
            //FluentPortalUser.PortalUser(userid, CRMObjectSingleton.ztCRM.Service)
            //    .Trace(s => Debug.WriteLine(s))
            //    .Join<FluentAccount>(c =>
            //        c.Join<FluentApplication>(a =>
            //            a.Join<FluentAnnotation>(
            //                an => an.UseAttribute((Guid noteId) => permittedImageIds.Add(noteId), "annotationid"))))
            //    .Execute();

            dynamic dto = new ExpandoObject();

            FluentAccount.Account(account1.Id, context.GetOrganizationService())
                .UseAttribute((string account) => dto.name = account, "name")
                .Join<FluentContact>(
                    c => c.UseAttribute<string>(contact => dto.fullname = contact, "firstname"))
                .Execute();
            Assert.IsTrue( ((IDictionary<string, object>)dto).ContainsKey("fullname"));
            dto.fullname = null;

            FluentAccount.Account(account1.Id, context.GetOrganizationService())
                .Join<FluentContact>(
                    c => c.UseEntity( (contact, alias) => 
                        dto.fullname = contact.GetAttributeValue<string>( contact.Alias + "firstname"), "firstname"))
                .Execute();

            Assert.IsNotNull(dto.fullname);
        }

        [TestMethod]
        public void TestDeepJoin()
        {
            var roleId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var roleEntities = new List<Entity>
            {
                new Entity("systemuser")
                {
                    Id = userId,
                },
                new Entity("systemuserroles")
                {
                    Id = Guid.NewGuid(),
                    ["systemuserid"] = userId,
                    ["roleid"] = roleId
                },
                new Entity("role")
                {
                    Id = roleId,
                    ["name"] = "Test Role"
                }
            };
            var ctx = TestUtilities.TestContext3(roleEntities);

            var hasRole = false;
            FluentSecurityRole.SecurityRole(ctx.GetOrganizationService())
                .Trace(s => Debug.WriteLine(s))
                .Where("name").Equals("Test Role")
                .Join<FluentSystemUserRoles>(
                    sur => sur.Join<FluentSystemUser>(
                        su => su.Where("systemuserid").Equals(userId)
                            .UseAttribute((Guid hr) => hasRole = true, "systemuserid")
                    )
                )
                .Execute();

            Assert.IsTrue(hasRole);

            hasRole = false;
            FluentSecurityRole.SecurityRole(ctx.GetOrganizationService())
                .Trace(s => Debug.WriteLine(s))
                .Where("name").Equals("Test Role")
                .Join<FluentSystemUserRoles>(
                    sur => sur.Join<FluentSystemUser>(
                        su => su.Where("systemuserid").Equals(userId)
                            .Exists(() => hasRole = true)
                    )
                )
                .Execute();
            Assert.IsTrue(hasRole);
        }

        public struct MyStruct
        {
            public string emailAddress;
            public string phoneNumber;
            public string name;

        }

        [TestMethod]
        public void TestUseEntity()
        {
            var context = TestUtilities.TestContext1();
            var contact1 = context.Data["contact"].First().Value;
            var service = context.GetOrganizationService();
            MyStruct myStruct;
        
            FluentContact.Contact(contact1.Id, service)
                .UseAttribute((string e) => myStruct.emailAddress = e, "emailaddress1")
                .UseAttribute<string>(num => myStruct.phoneNumber = num, "mobilephone", "telephone1")
                .UseEntity((EntityWrapper contact) =>
                    {
                        myStruct.name = string.Join(" ", contact["firstname"], contact["lastname"]);
                    },
                    "firstname", "lastname")
                .Execute();
        }
    }
}
