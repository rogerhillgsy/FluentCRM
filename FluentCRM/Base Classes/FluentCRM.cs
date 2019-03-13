using System;
using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public abstract partial class FluentCRM : IUnknownEntity, IEntitySet, INeedsWhereCriteria, IAnotherWhere
    {
        private Guid _id = Guid.Empty;

        public IOrganizationService Service { get; }

        public virtual string LogicalName { get; }

        private Entity _update;
        private string _whereAttribute = string.Empty;

        private ICollection<Entity> _entities;
        private readonly List<OrderExpression> _orders = new List<OrderExpression>();
        private readonly List<Action> _postExecuteActions = new List<Action>();

        /// <summary>
        /// List of actions to be carried out on the Entity collection once it has been retrieved from the sever.
        /// </summary>
        private readonly List<Tuple<string[], Func<EntityWrapper, string, bool?>>> _actionList = new List<Tuple<string[], Func<EntityWrapper, string, bool?>>>();


        #region "Constructors and Factory functions"

        protected FluentCRM(string logicalName, IOrganizationService service)
        {
            LogicalName = logicalName;
            Service = service;
        }

        protected FluentCRM(string logicalName, Guid id, IOrganizationService service) : this(logicalName, service)
        {
            _id = id;
        }

      
        public static IOrganizationService StaticService { get; set; } = null;

        protected FluentCRM(string logicalName )
        {
            LogicalName = logicalName;
            if (StaticService == null)
            {
                Trace("Warning: FluentCRM static service is null");
            }

            Service = StaticService;
        }

        protected FluentCRM(string logicalName, Guid id) : this(logicalName)
        {
            _id = id;
        }

        #endregion
    }
}
