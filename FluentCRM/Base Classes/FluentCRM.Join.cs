using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public abstract partial class FluentCRM : IJoinable, IJoinableNeedsWhereCriteria, IJoinableAnotherWhere, IJoinableEntitySet
    {
        private ICollection<FluentCRM> LinkedEntities { get; } = new List<FluentCRM>();

        #region "Join functions"

        IJoinableEntitySet IJoinableEntitySet.Delete()
        {
            throw new NotImplementedException();
        }

        IJoinableEntitySet IJoinableEntitySet.Clear(string attributeToClear, params string[] additionalAttributesToClear)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Join the current entity to the specified FluentCRM entity.
        /// </summary>
        /// <returns>FluentCRM entity</returns>
        /// <param name="target">Closure used to configure the joined entity.</param>
        /// <typeparam name="T">The type of entity joined to.</typeparam>
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
            t.ParentEntity = this;

            var link = t.LinkEntity;

            Trace(
                $"Link from {link.LinkFromEntityName}/{link.LinkFromAttributeName} to {link.LinkToEntityName}/{link.LinkToAttributeName}");
            Query.LinkEntities.Add(link);
            // Add links for case where there is more than one level of join.
            LinkEntity.LinkEntities.Add(link);

            target(t); // Call to set up the callbacks on linked entity.
            var cols = t._actionList.SelectMany(c => c.Item1).Where(c => c != AllColumns).Distinct().ToArray();
            if (t._actionList.SelectMany(c => c.Item1).Any(c => c == AllColumns))
            {
                Trace($"Fetching All columns");
                cols = new string[] {} ;
            }
            else
            {
                t.LinkEntity.Columns = new ColumnSet(cols);
            }

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

        private FluentCRM TopEntity => ParentEntity?.TopEntity ?? this;

        private FluentCRM ParentEntity { get; set; }

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

        IJoinableEntitySet IJoinable.Join<T>(Action<IJoinable> target)
        {
            return (IJoinableEntitySet) Join<T>(target);
        }

        IJoinableEntitySet IJoinableEntitySet.Join<T>(Action<IJoinable> target)
        {
            return (IJoinableEntitySet) Join<T>(target);
        }

        IJoinableEntitySet IJoinableEntitySet.UseAttribute<T>(Action<T> action, string attribute1, params string[] attributes)
        {
            return (IJoinableEntitySet) this.UseAttribute(default(T), false, (string c, T val) => action(val), attribute1, attributes);
        }

        IJoinableEntitySet IJoinableEntitySet.UseAttribute<T>(Action<string, T> action, string attribute1,
            params string[] attributes)
        {
            return (IJoinableEntitySet) this.UseAttribute(default(T), false, action, attribute1, attributes);
        }

        IJoinableEntitySet IJoinableEntitySet.UseAttribute<T>(T defaultValue, Action<T> action, string attribute1, params string[] attributes)
        {
            return (IJoinableEntitySet) this.UseAttribute(defaultValue, true, (string c, T val) => action(val), attribute1, attributes);
        }

        IJoinableEntitySet IJoinableEntitySet.UseAttribute<T>(T defaultValue, Action<string, T> action, string attribute1,
            params string[] attributes)
        {
            return (IJoinableEntitySet) this.UseAttribute(defaultValue, true, action, attribute1, attributes);
        }



        IJoinableEntitySet IJoinableEntitySet.UseEntity(Action<string, EntityWrapper, string> action, string attribute1,
            params string[] attributes)
        {
            return (IJoinableEntitySet) this.UseEntity(action, attribute1, attributes);
        }

        IJoinableEntitySet IJoinableEntitySet.UseEntity(Action<EntityWrapper, string> action, string attribute1,
            params string[] attributes)
        {
            return (IJoinableEntitySet) this.UseEntity(action, attribute1, attributes);
        }

        /// <summary>
        /// Update a given attribute in the current entity. Do so in a "weak" fashion.
        /// If the value is unchanged, no update will occur. (Including setting a null value to null)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToUpdate">Attribute to be updated.</param>
        /// <param name="updateValue">Value to be used to update the attribute</param>
        /// <typeparam name="T">The type of the attribute that will be updated</typeparam>
        public IJoinableEntitySet WeakUpdate<T>(string attributeToUpdate, T updateValue)
        {
            return (IJoinableEntitySet) ((IEntitySet) this).WeakUpdate(attributeToUpdate, updateValue);
        }

        /// <summary>
        /// Update a given attribute in the current entity. Do so in a "weak" fashion.
        /// If the value is unchanged, no update will occur. (Including setting a null value to null)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeToUpdate">Attribute to be updated.</param>
        /// <param name="getUpdateValue">Closure that returns the value to be used to update the attribute.</param>
        /// <typeparam name="T">The type of the attribute that will be updated</typeparam>
        public IJoinableEntitySet WeakUpdate<T>(string attributeToUpdate, Func<T, T> getUpdateValue)
        {
            return (IJoinableEntitySet) ((IEntitySet)this).WeakUpdate(attributeToUpdate, getUpdateValue);
            throw new NotImplementedException();
        }

        IJoinableEntitySet IJoinableEntitySet.Exists(Action<bool> action)
        {
            TopEntity.Exists(action);
            return this;
        }

        IJoinableEntitySet IJoinableEntitySet.Exists(Action whenTrue, Action whenFalse)
        {
            TopEntity.Exists(whenTrue, whenFalse);
            return this;
        }

        IJoinableAnotherWhere IJoinableEntitySet.And
        {
            get { return (IJoinableAnotherWhere) And; }
        } 

        /// <summary>
        /// Read an attribute from the current entity and call the action closure with the attribute value as argument.
        /// If the attribute has a null value, the closure will not be called.
        /// If multiple attributes are specified, then the first non-null value will be used to call the action closure.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="action">Closure to be called with the name of the attribute and the value of the attribute (if not null)</param>
        /// <param name="attribute">The logical name of the attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional attributes that we will try to use if the first attribute is null</param>
        /// <typeparam name="T">The expected type of the attribute that will be returned.</typeparam>
        public IJoinableEntitySet UseAttribute<T>(Action<string, T> action, string attribute, params string[] optionalAttributes)
        {
            return (IJoinableEntitySet) UseAttribute(default(T), false, action, attribute, optionalAttributes);
        }

        public IJoinableEntitySet UseAttribute<T>(T defaultValue,  Action<string, T> action, string attribute, params string[] optionalAttributes)
        {
            return (IJoinableEntitySet) UseAttribute(defaultValue, true, action, attribute, optionalAttributes);
        }


        IJoinableEntitySet IJoinable.UseEntity(Action<EntityWrapper,string> action, string attribute1, params string[] attributes)
        {
            return (IJoinableEntitySet) UseEntity(action, attribute1, attributes);
        }

        IJoinableEntitySet IJoinable.UseEntity(Action<string, EntityWrapper,string> action, string attribute1, params string[] attributes)
        {
            return (IJoinableEntitySet) UseEntity(action, attribute1, attributes);
        }

        /// <summary>
        /// Read an attribute from the current entity and call the action closure with the attribute value as argument.
        /// If the attribute has a null value, the closure will not be called.
        /// If multiple attributes are specified, then the first non-null value will be used to call the action closure.
        /// </summary>
        /// <returns>FluentCRM Object</returns>
        /// <param name="action">Closure to be called with the name of the attribute and the value of the attribute (if not null)</param>
        /// <param name="attribute">The logical name of the attribute that we will try to extract</param>
        /// <param name="optionalAttributes">Optional attributes that we will try to use if the first attribute is null</param>
        /// <typeparam name="T">The expected type of the attribute that will be returned.</typeparam>
        public IJoinableEntitySet UseAttribute<T>(Action<T> action, string attribute, params string[] optionalAttributes)
        {
            return (IJoinableEntitySet) UseAttribute(default(T), false, action, attribute, optionalAttributes);
        }

        public IJoinableEntitySet UseAttribute<T>(T defaultValue, Action<T> action, string attribute, params string[] optionalAttributes)
        {
            return (IJoinableEntitySet) UseAttribute(defaultValue, true, action, attribute, optionalAttributes);
        }


        /// <summary>
        /// Returns the name of the lookup attribute to be used to join to the given entity.
        /// </summary>
        /// <returns>The attribute.</returns>
        /// <param name="joinEntity">Join entity.</param>
        public virtual string JoinAttribute(string joinEntity)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Internal-use function used to get the name of the "this entity" attribute to be used to join to the specified "foreign" entity .
        /// </summary>
        /// <param name="foreignEntityName"></param>
        /// <returns>Name of "this entity" attribute to be used to join to the given "foreign" entity.</returns>
        public virtual string JoinFromAttribute(string foreignEntityName)
        {
            return null;
        }

        /// <summary>
        /// Used to spectify that an outer join is to be used.
        /// </summary>
        /// <returns>The outer.</returns>
        public IJoinable Outer()
        {
            LinkEntity.JoinOperator = JoinOperator.LeftOuter;
            return this;
        }
        #endregion

        /// <summary>
        /// Constructor required for Join operation.
        /// </summary>
        protected FluentCRM()
        {
            GetAlias = () => $"a.{AliasCount++}";
        }

        /// <summary>
        /// Used to ensure that constructed alias names are unique.
        /// </summary>
        protected int AliasCount = 1;

        /// <summary>
        /// Function used to get alias values for joined entities.
        /// </summary>
        protected Func<string> _getAlias;

        /// <summary>
        /// Base function used to get unique alias values.
        /// </summary>
        protected Func<string> GetAlias
        {
            get { return _getAlias ?? (_getAlias = () => $"a{AliasCount++}"); }
            set { _getAlias = value; }
        }

        /// <summary>
        /// Abstract factory function used to construct an instance of the target entity.
        /// </summary>
        /// <param name="service"></param>
        /// <returns></returns>
        public abstract IJoinable Factory(IOrganizationService service);

        /// <summary>
        /// Select joined entity records where the given attribute meets some condition
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeName">Logical name that is the subject of the condition</param>
        public IJoinableNeedsWhereCriteria Where(string attributeName)
        {
            return (IJoinableNeedsWhereCriteria) ((IUnknownEntity) this).Where(attributeName);
        }

        private LinkEntity _linkEntity;

        /// <summary>
        /// Link value used in joins to related entities.
        /// </summary>
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

        /// <summary>
        /// Used to return the Primary key of the current entity.
        /// </summary>
        public virtual string PrimaryKey => $"{LogicalName}id";

        IJoinableEntitySet IJoinableNeedsWhereCriteria.Equals<T>(T value)
        {
            return (IJoinableEntitySet) Equals(value);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.NotEqual<T>(T value)
        {
            return (IJoinableEntitySet) NotEqual(value);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.IsNotNull => (IJoinableEntitySet) (IsNotNull);
    

        IJoinableEntitySet IJoinableNeedsWhereCriteria.IsNull =>  (IJoinableEntitySet) (IsNull);

        IJoinableEntitySet IJoinableNeedsWhereCriteria.In<T>(params T[] inVals)
        {
            return (IJoinableEntitySet) In(inVals);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.GreaterThan<T>(T value)
        {
            return (IJoinableEntitySet) GreaterThan(value);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.LessThan<T>(T value)
        {
            return (IJoinableEntitySet) LessThan(value);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.BeginsWith(string s)
        {
            return (IJoinableEntitySet) BeginsWith(s);
        }

        IJoinableEntitySet IJoinableNeedsWhereCriteria.Condition(ConditionOperator op)
        {
            return (IJoinableEntitySet) Condition(op);
        }


        IJoinableEntitySet IJoinableNeedsWhereCriteria.Condition<T>(ConditionOperator op, T value)
        {
            return (IJoinableEntitySet) Condition(op,value);
        }

        IJoinableEntitySet IJoinableEntitySet.AfterEachRecord(Action<EntityWrapper> action)
        {
            TopEntity._afterEachRecordActions.Add((e) =>
            {
                var oldAlias = e.Alias;
                e.Alias = LinkEntity.EntityAlias + ".";
                action.Invoke(e);
                e.Alias = oldAlias;
            });
            return this;
        }

        private void PrepareLinkedCriteria()
        {
            foreach (var linkedEntity in LinkedEntities)
            {
                linkedEntity.PrepareLinkedCriteria();
            }

            if (LinkEntity != null)
            {
                LinkEntity.LinkCriteria = Query?.Criteria;
            }
        }
    }
}