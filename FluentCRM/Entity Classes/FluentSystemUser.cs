
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    /// <summary>
    /// FluentCRM class used to encapsulate access to the SystemUser Entity
    /// </summary>
    public class FluentSystemUser : FluentCRM
    {

        private const string _logicalName = "systemuser";

        #region "Constructors etc required by Language"
        private FluentSystemUser(Guid id, IOrganizationService service) : base(_logicalName, id, service) { }

        private FluentSystemUser(IOrganizationService service) : base(_logicalName, service) { }

        private FluentSystemUser(Guid id) : base(_logicalName, id) { }

        /// <summary>
        /// Select specific entity with given id value using specified IOrganizationService
        /// </summary>
        /// <param name="id">Guid of entity to select</param>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass - returns even if ID does not exist.</returns>
        public static IEntitySet SystemUser(Guid id, IOrganizationService service)
        {
            return new FluentSystemUser(id, service);
        }

        /// <summary>
        /// Select a (sub)set of the specified entity using specified IOrganizationService
        /// </summary>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public static IUnknownEntity SystemUser(IOrganizationService service)
        {
            return new FluentSystemUser(service);
        }

        /// <summary>
        /// Select specific entity with given id value using the static organization service specified by FluentCRM.StaticService
        /// </summary>
        /// <param name="id">Guid of entity to operator on</param>
        /// <returns>FluentCRM subclass - returns even if ID does not exist.</returns>
        public static IEntitySet SystemUser(Guid id)
        {
            return new FluentSystemUser(id);
        }

        /// <summary>
        /// Select a (sub)set of the specified entity using the static organization service specified by FluentCRM.StaticService
        /// </summary>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public static IUnknownEntity SystemUser()
        {
            return new FluentSystemUser();
        }

        /// <summary>
        /// Parameterless constructor required by the language, but not necessarily used.
        /// </summary>
        public FluentSystemUser() : base(_logicalName) { }

        /// <summary>
        /// Factory method to return an instance of the FluentCRM entity class with the given CRM connection.
        /// </summary>
        /// <param name="service">CRM system to fetch entity from</param>
        /// <returns>FluentCRM subclass that can be used to filter and operate on the specified entity type.</returns>
        public override IJoinable Factory(IOrganizationService service)
        {
            return new FluentSystemUser(service);
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
                return "systemuserid";
            }
        }



        /// <summary>
        /// Extract systemuser record for the current user.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static ICanExecute CurrentUser(){
            var rv = (IUnknownEntity) new FluentSystemUser();
            return  (ICanExecute) rv.Where("systemuserid").Condition(ConditionOperator.EqualUserId);
        }

        /// <summary>
        /// Extract systemuser record for the current user.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public static ICanExecute CurrentUser(IOrganizationService service)
        {
            var rv = (IUnknownEntity) new FluentSystemUser(service);
            return  (ICanExecute) rv.Where("systemuserid").Condition(ConditionOperator.EqualUserId);
        }

        public ICanExecute AddSecurityRole(Guid securityRoleId)
        {
            return AddSecurityRole(new List<Guid> {securityRoleId});
        }

        public ICanExecute AddSecurityRole(ICollection<Guid> securityRoleIds)
        {
            var roles = new EntityReferenceCollection( (from r in securityRoleIds select new EntityReference("role",r)).ToList());
            Trace("Tracing");
           return (ICanExecute)   this.UseAttribute((Guid id) =>
           {
               Service.Associate( LogicalName,
                   id,
                   new Relationship("systemuserroles_association"),
                   roles);
           }, "systemuserid");
        }

        public ICanExecute RemoveSecurityRole(Guid securityRoleId)
        {
            return RemoveSecurityRole(new List<Guid> {securityRoleId});
        }

        public ICanExecute RemoveSecurityRole(ICollection<Guid> securityRoleIds)
        {
            var roles = new EntityReferenceCollection( (from r in securityRoleIds select new EntityReference("role",r)).ToList());
            return (ICanExecute)   this.UseAttribute((Guid id) =>
            {
                Service.Disassociate( LogicalName,
                    id,
                    new Relationship("systemuserroles_association"),
                    roles);
            }, "systemuserid");
        }


    }
}
