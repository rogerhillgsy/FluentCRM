using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public abstract partial class FluentCRM : IJoinable, IJoinableNeedsWhereCriteria, IJoinableEntitySet
    {
        private ICollection<FluentCRM> LinkedEntities { get; } = new List<FluentCRM>();

        #region "Join functions"

        public ICanExecute Delete()
        {
            throw new NotImplementedException();
        }

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
            t.LinkEntity.LinkFromAttributeName = t.JoinFromAttribute(LogicalName) ?? JoinAttribute(t.LogicalName);
            t.LinkEntity.LinkToAttributeName = t.JoinAttribute(LogicalName);
            t.GetAlias = GetAlias; // Make sure that any linked entitites use the same sequence of unique aliases

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
                new string[] {"versionnumber"},
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

        ICanExecute IJoinableEntitySet.UseAttribute<T>(Action<T> action, string attribute1, params string[] attributes)
        {
            throw new NotImplementedException();
        }

        ICanExecute IJoinableEntitySet.UseAttribute<T>(Action<string, T> action, string attribute1,
            params string[] attributes)
        {
            throw new NotImplementedException();
        }

        ICanExecute IJoinableEntitySet.UseEntity(Action<EntityWrapper> action, string attribute1,
            params string[] attributes)
        {
            throw new NotImplementedException();
        }

        public ICanExecute WeakUpdate<T>(string attributeToUpdate, T updateValue)
        {
            throw new NotImplementedException();
        }

        public ICanExecute WeakUpdate<T>(string attributeToUpdate, Func<T, T> getUpdateValue)
        {
            throw new NotImplementedException();
        }

        IJoinableAnotherWhere IJoinableEntitySet.And => _and;

        public IJoinable UseAttribute<T>(Action<string, T> action, string attribute1, params string[] attributes)
        {
            return (IJoinable) ((IEntitySet) this).UseAttribute(action, attribute1, attributes);
        }

        public IJoinable UseEntity(Action<EntityWrapper> action, string attribute1, params string[] attributes)
        {
            return (IJoinable) ((IEntitySet) this).UseEntity(action, attribute1, attributes);
        }

        public IJoinable UseAttribute<T>(Action<T> action, string attribute1, params string[] attributes)
        {
            return (IJoinable) ((IEntitySet) this).UseAttribute(action, attribute1, attributes);
        }


        public virtual string JoinAttribute(string JoinEntity)
        {
            throw new NotImplementedException();
        }

        // Use in non-standard join scenarios, such as Account to primary contact.
        public virtual string JoinFromAttribute(string leftEntityName)
        {
            return null;
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

        public IJoinableNeedsWhereCriteria Where(string attributeName)
        {
            return (IJoinableNeedsWhereCriteria) ((IUnknownEntity) this).Where(attributeName);
        }

        private LinkEntity _linkEntity;
        private IJoinableEntitySet _isNotNull;
        private IJoinableEntitySet _isNull;
        private IJoinableAnotherWhere _and;

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

        IJoinableEntitySet IJoinableNeedsWhereCriteria.Equals<T>(T value)
        {
            return (IJoinableEntitySet) ((INeedsWhereCriteria) this).Equals(value);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.NotEqual<T>(T value)
        {
            return (IJoinableEntitySet) ((INeedsWhereCriteria) this).NotEqual(value);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.IsNotNull => (IJoinableEntitySet) (IsNotNull);
    

        IJoinableEntitySet IJoinableNeedsWhereCriteria.IsNull =>  (IJoinableEntitySet) (IsNull);

        IJoinableEntitySet IJoinableNeedsWhereCriteria.In<T>(params T[] inVals)
        {
            return (IJoinableEntitySet) ((INeedsWhereCriteria) this).In(inVals);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.GreaterThan<T>(T value)
        {
            return (IJoinableEntitySet) ((INeedsWhereCriteria) this).GreaterThan(value);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.LessThan<T>(T value)
        {
            return (IJoinableEntitySet) ((INeedsWhereCriteria) this).LessThan(value);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.BeginsWith(string s)
        {
            return (IJoinableEntitySet) ((INeedsWhereCriteria) this).BeginsWith(s);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.Condition<T>(ConditionOperator op, T value)
        {
            return (IJoinableEntitySet) ((INeedsWhereCriteria) this).Condition(op,value);
        }

        private void PrepareLinkedCriteria()
        {
            LinkEntity.LinkCriteria = Query?.Criteria;
        }
    }
}