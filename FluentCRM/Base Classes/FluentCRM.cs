using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    /// <summary>
    /// Base class used to provide a Fluent-style interface for interacting with Microsoft Dynamics CRM
    /// </summary>
    public abstract partial class FluentCRM : IUnknownEntity, IEntitySet, INeedsWhereCriteria, IAnotherWhere
    {
        internal Guid _id = Guid.Empty;

        /// <summary>
        /// IOrganizationService that will be used by the FluentCRM instance.
        /// </summary>
        public IOrganizationService Service { get; }

        /// <summary>
        /// Logical name of the entity associated with the current FluentCRM instance.
        /// </summary>
        public virtual string LogicalName { get; }

        private Entity _update;
        private string _whereAttribute = string.Empty;

        private ICollection<Entity> _entities;
        private readonly List<OrderExpression> _orders = new List<OrderExpression>();
        private readonly List<Action> _postExecuteActions = new List<Action>();
        private int? _top = null;

        /// <summary>
        /// List of actions to be carried out on the Entity collection once it has been retrieved from the sever.
        /// </summary>
        private readonly List<Tuple<string[], Func<EntityWrapper, string, bool?>>> _actionList = new List<Tuple<string[], Func<EntityWrapper, string, bool?>>>();

        // Indicate error status
        public bool HasErrors { get; protected set; } = false;
        public string LastError { get; protected set; } = String.Empty;

        #region "Constructors and Factory functions"

        /// <summary>
        /// Construct FluentCRM instance with the given logical name and IOrganizationService for accssing CRM.
        /// </summary>
        /// <param name="logicalName">CRM logical name of the entity to be accessed</param>
        /// <param name="service">Connection to be used to access CRM.</param>
        protected FluentCRM(string logicalName, IOrganizationService service)
        {
            LogicalName = logicalName;
            Service = service;
        }

        /// <summary>
        /// Construct FluentCRM instance with the given logical name and IOrganizationService for accessing CRM.
        /// </summary>
        /// <param name="logicalName">CRM logical name of the entity to be accessed</param>
        /// <param name="id">Guid of a specific entity to be operated on.</param>
        /// <param name="service">Connection to be used to access CRM.</param>
        protected FluentCRM(string logicalName, Guid id, IOrganizationService service) : this(logicalName, service)
        {
            _id = id;
        }

        /// <summary>
        /// Used to set or return a CRM connection that can be used globally by all FluentCRM objects.
        /// </summary>
        public static IOrganizationService StaticService { get; set; } = null;

        /// <summary>
        /// Select specific entity with given id value using the static organization service specified by FluentCRM.StaticService
        /// </summary>
        /// <param name="logicalName">CRM logical name of the entity to be accessed</param>
        protected FluentCRM(string logicalName )
        {
            LogicalName = logicalName;
            if (StaticService == null)
            {
                Trace("Warning: FluentCRM static service is null");
            }

            Service = StaticService;
        }

        /// <summary>
        /// Select specific entity with given id value using the static organization service specified by FluentCRM.StaticService
        /// </summary>
        /// <param name="logicalName">CRM logical name of the entity to be accessed</param>
        /// <param name="id">Guid of a specific entity to be operated on.</param>
        protected FluentCRM(string logicalName, Guid id) : this(logicalName)
        {
            _id = id;
        }

        #endregion
    }
}
