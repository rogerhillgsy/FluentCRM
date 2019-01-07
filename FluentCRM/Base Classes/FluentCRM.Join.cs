using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public abstract partial class FluentCRM : IJoinable
    {
        private ICollection<FluentCRM> LinkedEntities { get; } = new List<FluentCRM>();

        #region "Join functions"

        public ICanExecute Join<T>(Action<IJoinable> target) where T : FluentCRM, new()
        {
            // Force use of RetrieveMultiple() rather than just Retrieve.
            if (!Guid.Empty.Equals(_id))
            {
                AddCriteria(new ConditionExpression(PrimaryKey, ConditionOperator.Equal, _id)); 
                _id = Guid.Empty;
            }

            var t = new T();
            t = (T) t.Factory(Service);
            LinkedEntities.Add(t);
            t._traceFunction = s => { this.Trace(s); };

            t.LinkEntity.EntityAlias = GetAlias();
            t.LinkEntity.LinkFromEntityName = LogicalName;
            t.LinkEntity.LinkToEntityName = t.LogicalName;
            t.LinkEntity.LinkFromAttributeName = JoinAttribute(t.LogicalName);
            t.LinkEntity.LinkToAttributeName = t.JoinAttribute(LogicalName);
            t.GetAlias = GetAlias; // Make sure that any use the same sequence of unique aliases

            var link = t.LinkEntity;

            Trace(
                $"Link from {link.LinkFromEntityName}/{link.LinkFromAttributeName} to {link.LinkToEntityName}/{link.LinkToAttributeName}");
            Query.LinkEntities.Add(link);
            // Add links for case where there is more than one level of join.
            LinkEntity.LinkEntities.Add(link);

            target(t); // Call to set up the callbacks on linked entity.
            var cols = t._actionList.SelectMany(c => c.Item1).Where(c => c != AllColumns).Distinct().ToArray();
            t.LinkEntity.Columns = new ColumnSet(cols);

            var resultCols = cols.Select(s => $"{link.EntityAlias}.{s}");

            _actionList.Add(new Tuple<string[], Func<EntityWrapper, string, bool?>>(
                new string[] { "versionnumber" },
                (entity, c) =>
                {
                    t._actionList.ForEach(a =>
                    {
                        foreach (var column in resultCols)
                        {
                            var result = a.Item2?.Invoke(entity, column);
                            if (result.HasValue && result.Value)
                            {
                                return;
                            }
                        }
                    });
                    return true;
                }));

            return this;
        }

        ICanExecute IUnknownEntity.Join<T>(Action<IJoinable> target)
        {
            return Join<T>(target);
        }

        ICanExecute IEntitySet.Join<T>(Action<IJoinable> target)
        {
            return Join<T>(target);
        }

        ICanExecute ICanExecute.Join<T>(Action<IJoinable> target)
        {
            return Join<T>(target);
        }

        public IJoinable UseAttribute<T>(Action<string, T> action, string attribute1, params string[] attributes)
        {
            return (IJoinable)((IEntitySet) this).UseAttribute(action, attribute1, attributes);
        }

        public IJoinable UseEntity(Action<EntityWrapper> action, string attribute1, params string[] attributes)
        {
            return (IJoinable)((IEntitySet) this).UseEntity(action, attribute1, attributes);
        }

        public IJoinable UseAttribute<T>(Action<T> action, string attribute1, params string[] attributes)
        {
            return (IJoinable)((IEntitySet) this).UseAttribute(action, attribute1, attributes);
        }


        public virtual string JoinAttribute(string JoinEntity)
        {
            throw new NotImplementedException();
        }

        public IJoinable Outer { get; }

        #endregion

        protected FluentCRM()
        {
            GetAlias = () => $"a.{AliasCount++}";
        }

        protected int AliasCount = 1;

        protected Func<string> _getAlias;
        protected Func<string> GetAlias
        {
            get { return _getAlias ?? (_getAlias = () => $"a{AliasCount++}"); }
            set { _getAlias = value; }
        }

        public abstract IJoinable Factory(IOrganizationService service);

        private LinkEntity _linkEntity;
        protected LinkEntity LinkEntity
        {
            get
            {
                return _linkEntity ?? (_linkEntity = new LinkEntity
                {
                    Columns = new ColumnSet(true),
                    JoinOperator = JoinOperator.Inner,
                    LinkFromEntityName = LogicalName
                });
            }
        }

        public virtual string PrimaryKey => $"{LogicalName}id";
    }
}