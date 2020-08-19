
using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    /// <summary>
    /// FluentCRM class used to encapsulate access to the Activity Entity
    /// </summary>
    public class FluentActivity : FluentCRM
    {

        private static string _logicalName = "activity";

        #region "Constructors etc required by Language"

        private FluentActivity(Guid id, IOrganizationService service, string logicalName) : base(logicalName, id,
            service)
        {
            _logicalName = logicalName;
        }

        protected FluentActivity(IOrganizationService service, string logicalName) : base(logicalName, service)
        {
            _logicalName = logicalName;
        }

        private FluentActivity(Guid id, string logicalName) : base(logicalName, id)
        {
            _logicalName = logicalName;
        }

        /// <summary>
        /// Select specific entity with given id value using specified IOrganizationService
        /// </summary>
        /// <param name="id">Guid of entity to select</param>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass - returns even if ID does not exist.</returns>
        public static IEntitySet Activity(Guid id, IOrganizationService service, string logicalName)
        {
            return new FluentActivity(id, service, logicalName);
        }

        /// <summary>
        /// Select a (sub)set of the specified entity using specified IOrganizationService
        /// </summary>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public static IUnknownEntity Activity(IOrganizationService service, string logicalName)
        {
            return new FluentActivity(service, logicalName);
        }

        /// <summary>
        /// Select specific entity with given id value using the static organization service specified by FluentCRM.StaticService
        /// </summary>
        /// <param name="id">Guid of entity to operator on</param>
        /// <param name="logicalName">Logical name of specific activity</param>
        /// <returns>FluentCRM subclass - returns even if ID does not exist.</returns>
        public static IEntitySet Activity(Guid id,string logicalName)
        {
            return new FluentActivity(id, logicalName);
        }

        /// <summary>
        /// Select a (sub)set of the specified entity using the static organization service specified by FluentCRM.StaticService
        /// </summary>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public static IUnknownEntity Activity()
        {
            return new FluentActivity();
        }

        /// <summary>
        /// Parameterless constructor required by the language, but not necessarily used.
        /// </summary>
        public FluentActivity() : base(_logicalName) { }

        /// <summary>
        /// Factory method to return an instance of the FluentCRM entity class with the given CRM connection.
        /// </summary>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentActivity(service, _logicalName);
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
                return "activityid";
            }
        }
    }
}
