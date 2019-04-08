using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FakeXrmEasy.Extensions;
using FluentCRM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using TestFluentCRM;
using Guid = System.Guid;


namespace TestFluentCRMLive
{
    /// <summary>
    /// A study of some test cases with FluentCRM used to facilitate the decommissioning of CRM infrastructure.
    /// - Create Decomissioning Teams programatically.
    /// - Programatically assign users to teams.
    /// - Programatically grant and revoke security roles to users.
    /// - Validate results of decommissioning operation.
    /// </summary>
    [TestClass]
    [Ignore]
    public class DecommissioningTests
    {
        private string _connectionString;

        [TestInitialize]
        public void Setup()
        {
            // Obtain connection string from app.config and expand any environment variables e.g. for Passwords, usernames)
            _connectionString = ConfigurationManager.ConnectionStrings["CrmOnline"].ConnectionString;
            _connectionString = Environment.ExpandEnvironmentVariables(_connectionString);
        }

        /// <summary>
        /// Keep the Connection string information in one place
        /// </summary>
        private string ConnectionString { get; } = Environment.ExpandEnvironmentVariables(
            "Url=https://mytrialaccount.crm.dynamics.com/;Username=%Account%;Password=%Password%;authtype=Office365;");
        private string ConnectionStringTemplate { get; } = Environment.ExpandEnvironmentVariables(
            "Url=https://trialaccount**.crm.dynamics.com/;Username=%Account%;Password=%Password%;authtype=Office365;RequireNewInstance=True");

        /// <summary>
        /// Create teams for decommissioning users.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        [DataTestMethod()]
        [DataRow("Decommissioning", "User account required during decommissioning")]
        [DataRow("Not Decommissioning", "User accounts not required during decommissioning")]
        [TestCategory("Decommissioning")]
        public void T1_CreateDecommissioningTeams(string name, string description)
        {
            using (var crmSvc = new CrmServiceClient(ConnectionString))
            {
                FluentCRM.FluentCRM.StaticService = crmSvc.OrganizationServiceProxy;
                EntityReference currentUser = null;
                EntityReference businessUnit = null;
                FluentSystemUser.CurrentUser()
                    .UseAttribute((Guid id) => currentUser = new EntityReference("systemuser", id), "systemuserid")
                    .UseAttribute((EntityReference id) => businessUnit = id, "businessunitid")
                    .Execute();

                Assert.IsNotNull(currentUser.Id);
                Assert.IsNotNull(businessUnit.Id);

                FluentTeam.Team().Where("Name").Equals(name)
                    .Exists(() => { Debug.WriteLine("Team already exists"); },
                        () =>
                            FluentTeam.Team().Create(new Dictionary<string, object>
                            {
                                ["name"] = name,
                                ["administratorid"] = currentUser,
                                ["teamtype"] = new OptionSetValue(0), // Owner
                                ["description"] = "User account required during decommissioning",
                                ["businessunitid"] = businessUnit,
                            }).Execute()
                    )
                    .Execute();

            }
        }

        private List<string> DecommissioningUsers = new List<string>
        {
            "Roger Hill",
            "Demo User",
            "Alan Steiner (Sample Data)",
            "CRM Service",
            "Web Service account",
        };

        /// <summary>
        ///  Add list of users to decommissioning team.
        /// </summary>
        [TestMethod()]
        [TestCategory("Decommissioning")]
        public void T2_AddUsersToDecommissioningTeam()
        {
            using (var crmSvc = new CrmServiceClient(ConnectionString))
            {
                FluentCRM.FluentCRM.StaticService = crmSvc.OrganizationServiceProxy;
                var decommissioningTeam = Guid.Empty;
                FluentTeam.Team().Where("name").Equals("Decommissioning")
                    .UseAttribute((Guid id) => decommissioningTeam = id, "teamid").Execute();
                Assert.IsFalse(Guid.Empty.Equals(decommissioningTeam));

                var decommissioningUserIds = new HashSet<Guid>();
                FluentSystemUser.SystemUser()
                    .Where("fullname").In(DecommissioningUsers.ToArray())
                    .And
                    .Where("isdisabled").Equals("False")
                    .UseAttribute((Guid id) => decommissioningUserIds.Add(id), "systemuserid")
                    // Uncomment and use if you are told that any of the user ids are invalid.,
                    //.UseEntity((e) =>
                    //{
                    //    Debug.WriteLine($"Adding {e["fullname"]} id {e.Id} to team");
                    //    FluentTeam.Team(decommissioningTeam).AddMembersToTeam(new[] {e.Id} ).Execute();                   
                    //}, "systemuserid","fullname")
                    .Exists((e) => Assert.IsTrue(e))
                    .Execute();
                Assert.IsTrue(decommissioningUserIds.Count > 0);

                // Can do it this way - but if any of the users are invalid, it fails for all of them - and without saying which ones failed.
                FluentTeam.Team(decommissioningTeam).AddMembersToTeam(decommissioningUserIds.ToArray()).Execute();
            }
        }


        /// <summary>
        /// Test case to showcase joins and Team manipulation.
        /// Add all enabled users who are not in the team "Decommissioning" to the team "Not Decomissioning
        /// </summary>
        [TestMethod]
        [TestCategory("Decommissioning")]
        public void T3_addUsersToNonDecommissioningTeam()
        {
            // var connectionString = Environment.ExpandEnvironmentVariables("Url=%CRMTESTINSTANCE%;Username=%Account%;Password=%Password%;authtype=Office365;");
            using (var crmSvc = new CrmServiceClient(ConnectionString))
            {
                FluentCRM.FluentCRM.StaticService = crmSvc.OrganizationServiceProxy;

                var decommissioningTeam = new HashSet<Guid>();
                var notDecommissioningTeam = new HashSet<Guid>();

                // Read back all of the users in the "Decommissioning team"
                FluentSystemUser.SystemUser()
                    .Where("accessmode").NotEqual(3)
                    .And
                    .Where("accessmode").NotEqual(5)
                    .And
                    .Where("isdisabled").Equals("False")
                    .Join<FluentTeamMembership>(tm =>
                        tm.Join<FluentTeam>(t =>
                            t.Where("name").Equals("Decommissioning")
                        ))
                    .UseAttribute((Guid id) => decommissioningTeam.Add(id), "systemuserid")
                    .Execute();

                var notDecommTeamId = Guid.Empty;
                FluentTeam.Team().Where("name").Equals("Not Decommissioning")
                    .UseAttribute((Guid id) => notDecommTeamId = id, "teamid").Execute();
                Assert.IsFalse(Guid.Empty.Equals(notDecommTeamId), "Decommissioning team is not defined");

                FluentSystemUser.SystemUser()
                    .Where("accessmode").NotEqual(3)
                    .And
                    .Where("accessmode").NotEqual(5)
                    .And
                    .Where("isdisabled").Equals("False")
                    .UseAttribute((Guid id) =>
                    {
                        if (!decommissioningTeam.Contains(id))
                        {
                            notDecommissioningTeam.Add(id);
                        }
                    }, "systemuserid")
                    .Execute();

                Assert.IsTrue(notDecommissioningTeam.Count > 0);

                // Add users who are not a member of the decommissioning team to the "non decomissioning team"
                FluentTeam.Team(notDecommTeamId)
                    .AddMembersToTeam(notDecommissioningTeam)
                    .Trace(s => Debug.WriteLine(s))
                    .Count((int? c) =>
                    {
                        Assert.IsTrue(c > 0);
                        Debug.WriteLine($"Added {notDecommissioningTeam.Count} users to decommissioning team");
                    })
                    .Execute();
            }
        }

        [TestMethod]
        [TestCategory("Decommissioning")]
        public void T4_RemoveRoleMembership()
        {
            using (var crmSvc = new CrmServiceClient(ConnectionString))
            {
                FluentCRM.FluentCRM.StaticService = crmSvc.OrganizationServiceProxy;

                var notDecommissioningTeam = new HashSet<Guid>();

                // Find current business Unit.
                EntityReference businessUnit = null;
                FluentSystemUser.CurrentUser().UseAttribute((EntityReference bu) => businessUnit=bu,"businessunitid").Execute();

                // Get the id of the "Null" role.
                var nullRoles = new Dictionary<Guid,Guid>(); 
                FluentSecurityRole.SecurityRole()
                    .Where("name").Equals("Null Role")
                    .UseEntity((e) =>
                    {
                        nullRoles.Add(((EntityReference)e["businessunitid"]).Id, (Guid) e["roleid"]);
                        Debug.WriteLine(
                                $"Role: {e["name"]} Roleid: {e["roleid"]} businessunitid: {e["businessunitid"]}");
                    },"roleid", "name","businessunitid" )
                    .Count(c => Debug.WriteLine($"Found {c} null security roles")).Execute();
                Assert.IsTrue(nullRoles.Count > 0 , "Null roles not defined in CRM");

                var userRolesToRemove = new Dictionary<Guid, HashSet<Guid>>();
                // Read back all of the users in the "Not Decommissioning" team
                FluentSystemUser.SystemUser()
                    .Where("accessmode").NotEqual(3)
                    .And
                    .Where("accessmode").NotEqual(5)
                    .And
                    .Where("isdisabled").Equals("False")
                    .Join<FluentTeamMembership>(tm =>
                        tm.Join<FluentTeam>(t =>
                            t.Where("name").Equals("Not Decommissioning")
                        ))
                    .Join<FluentSystemUserRoles>(ur =>
                        ur.Join<FluentSecurityRole>(
                            r => r.UseEntity((e, s) =>
                            {
                                var userid = e.GetAttributeValue<Guid>("systemuserid");
                                var roleid = (Guid) e[e.Alias + "roleid"];
                                if (!userRolesToRemove.ContainsKey(userid))
                                {
                                    userRolesToRemove.Add(userid, new HashSet<Guid>());
                                }

                                var bu = e.GetAttributeValue<EntityReference>("businessunitid");
                                if (!nullRoles[bu.Id].Equals(roleid))
                                {
                                    userRolesToRemove[userid].Add(roleid);
                                }
                                Debug.WriteLine($"Remove role {e[ e.Alias + "name"]} from user {e.GetAttributeValue<string>("fullname")}");
                            }, "roleid","name")))
                    .UseEntity((e) => { }, "systemuserid","fullname", "businessunitid")
                    .Execute();

                // Remove listed roles from users in the non-decommissioning team
                foreach (var userRoles in userRolesToRemove)
                {
                    Debug.WriteLine("Re");
                    var user = (FluentSystemUser) FluentSystemUser.SystemUser(userRoles.Key);
                    var username = string.Empty;
                    EntityReference bu = null;
                    user.RemoveSecurityRole(userRoles.Value.ToArray())
                        .UseAttribute((EntityReference er) => bu = er, "businessunitid" )
                        .UseAttribute<string>(u => username = u ,"fullname")
                        .Execute();

                    ((FluentSystemUser) FluentSystemUser.SystemUser(userRoles.Key)
                            .WeakUpdate("accessmode", new OptionSetValue(1)))
                        .AddSecurityRole(nullRoles[bu.Id])
                        .Execute();
                    Debug.WriteLine($"Removed roles from {username} and added null role");
                }
            }
        }

        /// <summary>
        /// Make sure that all accounts in the decommissioned group are really decommissioned - only the Null security role, and in Administrative access mode.
        /// </summary>
        [TestMethod]
        public void T5_verifyDecommissionedAccounts()
        {
            ValidateDecommissioningState(ConnectionString);
        }

        [TestMethod]
        public void T6_verifyAllOrgsDecommissionedAccounts()
        {
            foreach (var org in new string[] {"-dev", "-qa", "-uat", ""})
            {
                Console.WriteLine("");
                Console.WriteLine($"Validating {org}");
                Console.WriteLine("");
                var connection = ConnectionStringTemplate;
                connection = connection.Replace("**", org);
                ValidateDecommissioningState(connection);
            }
        }


        private void ValidateDecommissioningState(string connection) {
        using (var crmSvc = new CrmServiceClient( connection))
            {
                FluentCRM.FluentCRM.StaticService = crmSvc.OrganizationServiceProxy;
                Console.WriteLine($"Connected to {crmSvc.CrmConnectOrgUriActual}");

                var userName = string.Empty;
                OptionSetValue accessmode = null;
                var nullRole = Guid.Empty;

                FluentSecurityRole.SecurityRole().Where("name").Equals("Null Role").UseAttribute((Guid id) => nullRole=id,"roleid").Execute();
                if (Guid.Empty.Equals(nullRole))    
                {
                    Console.WriteLine(">>>> Null Role not found");
                    return;
                }

                // Look at decommissioning team members, and make sure that each member of this team is in admin mode and has no security roles apart form the null role.
                FluentTeam.Team().Where("name").Equals("Not Decommissioning")
                //    .Trace(s => Debug.WriteLine(s))
                    .Join<FluentTeamMembership>(tm =>
                        tm.Join<FluentSystemUser>(u => u
                            .UseAttribute((string n) => userName = n, "fullname")
                            .UseAttribute((OptionSetValue o) => accessmode = o, "accessmode")
                            .UseAttribute((Guid userId) =>
                            {
                                var roles  = new Dictionary<Guid, string>();
                                var roleId = Guid.Empty;
                                // Get roles  for the current user
                                FluentSystemUser.SystemUser(userId)
                                //    .Trace(s => Debug.WriteLine(s))
                                    .Join<FluentSystemUserRoles>(ur => ur.Join<FluentSecurityRole>(r => r
                                        .UseAttribute((Guid roleId1) => roleId = roleId1, "roleid")
                                        .UseAttribute((string n) => { roles.Add(roleId, n); },
                                            "name")));
                                if (roles.ContainsKey(nullRole)) roles.Remove(nullRole);
                                var roleMessage = string.Empty;
                                if (roles.Count > 0)
                                {
                                    roleMessage = $"Found unexpected roles: [{string.Join(",", roles.Values)}]";
                                }

                                var accessModeMessage = string.Empty;
                                if (accessmode.Value != 1)
                                {
                                    accessModeMessage = $"Accessmode was not administrative: found {accessmode.Value}";
                                }

                                if ( !(string.IsNullOrEmpty(roleMessage) && string.IsNullOrEmpty(accessModeMessage)))
                                {
                                    Console.WriteLine($">>> Incorrect configuration for user: {userName} - {roleMessage}/{accessModeMessage}");
                                } else
                                {
                                    Console.WriteLine($"User OK: {userName}");
                                }
                            }, "systemuserid")
                        ))
                    .Exists(() => { }, ()=>{Console.WriteLine(">>>>> Decommissioning team not found, or did not have any members");})
                    .Count((n) => Console.WriteLine($"Verified {n} Accounts"))
                    .Execute();

                crmSvc.Dispose();              
            }
        }
    }
}
