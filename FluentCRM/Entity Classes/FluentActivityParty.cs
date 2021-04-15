using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    /// <summary>
    /// FluentCRM class used to encapsulate access to the ActivityParty Entity
    /// </summary>
    public class FluentActivityParty : FluentCRM
    {

        private const string _logicalName = "activityparty";

        #region "Constructors etc required by Language"
        private FluentActivityParty(Guid id, IOrganizationService service) : base(_logicalName, id, service) { }

        protected FluentActivityParty(IOrganizationService service) : base(_logicalName, service) { }

        private FluentActivityParty(Guid id) : base(_logicalName, id) { }

        /// <summary>
        /// Select specific entity with given id value using specified IOrganizationService
        /// </summary>
        /// <param name="id">Guid of entity to select</param>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass - returns even if ID does not exist.</returns>
        public static IEntitySet ActivityParty(Guid id, IOrganizationService service)
        {
            return new FluentActivityParty(id, service);
        }

        /// <summary>
        /// Select a (sub)set of the specified entity using specified IOrganizationService
        /// </summary>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public static IUnknownEntity ActivityParty(IOrganizationService service)
        {
            return new FluentActivityParty(service);
        }

        /// <summary>
        /// Select specific entity with given id value using the static organization service specified by FluentCRM.StaticService
        /// </summary>
        /// <param name="id">Guid of entity to operator on</param>
        /// <returns>FluentCRM subclass - returns even if ID does not exist.</returns>
        public static IEntitySet ActivityParty(Guid id)
        {
            return new FluentActivityParty(id);
        }

        /// <summary>
        /// Select a (sub)set of the specified entity using the static organization service specified by FluentCRM.StaticService
        /// </summary>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public static IUnknownEntity ActivityParty()
        {
            return new FluentActivityParty();
        }

        /// <summary>
        /// Parameterless constructor required by the language, but not necessarily used.
        /// </summary>
        public FluentActivityParty() : base(_logicalName) { }

        /// <summary>
        /// Factory method to return an instance of the FluentCRM entity class with the given CRM connection.
        /// </summary>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentActivityParty(service);
        }
        #endregion

        /// <summary>
        /// Use to specify where join parameters to other entities are non-standard.
        /// </summary>
        private readonly Dictionary<string, string> _joinOn = new Dictionary<string, string>
        {
            // if the join to another entity is through the primary id field, (1:N join) nothing is needed here.
            // If it is through another field (N:1 join) then the details of the foreign entity and lookup field need to be given here.
            //
            // { "foreign entity logical name", "logical name of lookup field in this entity" }
            //   { "account", "parentcustomerid" } 
            {"activitypointer", "activityid"},
            {"appointment", "activityid"},
            {"bulkoperation", "activityid"},
            {"campaignactivity", "activityid"},
            {"campaignresponse", "activityid"},
            {"email", "activityid"},
            {"fax", "activityid"},
            {"incidentresolution", "activityid"},
            {"letter", "activityid"},
            {"opportunityclose", "activityid"},
            {"orderclose", "activityid"},
            {"phonecall", "activityid"},
            {"quoteclose", "activityid"},
            {"recurringappointmentmaster", "activityid"},
            {"serviceappointment", "activityid"},
            {"socialactivity", "activityid"},
            {"task", "activityid"},
            {"untrackedemail", "activityid"},
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
                // Most entities join via the partyid.
                return "partyid";
            }
        }
    }
}
