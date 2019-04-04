
using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    /// <summary>
    /// FluentCRM class used to encapsulate access to the TeamMembership Entity
    /// </summary>
    public class FluentTeamMembership : FluentCRM
    {

        private const string _logicalName = "teammembership";

        private FluentTeamMembership(Guid id, IOrganizationService service) : base(_logicalName, id, service) { }

        private FluentTeamMembership(IOrganizationService service) : base(_logicalName, service) { }

        private FluentTeamMembership(Guid id) : base(_logicalName, id) { }

        /// <summary>
        /// Select specific entity with given id value using specified IOrganizationService
        /// </summary>
        /// <param name="id">Guid of entity to select</param>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass - returns even if ID does not exist.</returns>
        public static IEntitySet TeamMembership(Guid id, IOrganizationService service)
        {
            return new FluentTeamMembership(id, service);
        }

        /// <summary>
        /// Select a (sub)set of the specified entity using specified IOrganizationService
        /// </summary>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public static IUnknownEntity TeamMembership(IOrganizationService service)
        {
            return new FluentTeamMembership(service);
        }

        /// <summary>
        /// Select specific entity with given id value using the static organization service specified by FluentCRM.StaticService
        /// </summary>
        /// <param name="id">Guid of entity to operator on</param>
        /// <returns>FluentCRM subclass - returns even if ID does not exist.</returns>
        public static IEntitySet TeamMembership(Guid id)
        {
            return new FluentTeamMembership(id);
        }

        /// <summary>
        /// Select a (sub)set of the specified entity using the static organization service specified by FluentCRM.StaticService
        /// </summary>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public static IUnknownEntity TeamMembership()
        {
            return new FluentTeamMembership();
        }

        /// <summary>
        /// Parameterless constructor required by the language, but not necessarily used.
        /// </summary>
        public FluentTeamMembership() : base(_logicalName) { }

        /// <summary>
        /// Factory method to return an instance of the FluentCRM entity class with the given CRM connection.
        /// </summary>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentTeamMembership(service);
        }

        /// <summary>
        /// Use to specify where join parameters to other entites are non-standard.
        /// </summary>
        private readonly Dictionary<string, string> _joinOn = new Dictionary<string, string>
        {
            {"systemuser", "systemuserid"},
            {"team", "teamid"}
        };

        /// <summary>
        /// Internal-use function used to get the name of the "this entity" attribute to be used to join to the specified "foreign" entity .
        /// </summary>
        /// <param name="foreignEntityName"></param>
        /// <returns>Name of "this entity" attribute to be used to join to the given "foreign" entity.</returns>
        public override string JoinAttribute(string foreignEntityName)
        {
            if (_joinOn.ContainsKey(foreignEntityName))
            {
                return _joinOn[foreignEntityName];
            }
            else
            {
                return "teammembershipid";
            }
        }
    }
}
