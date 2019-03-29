using System;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public abstract partial class FluentCRM
    {
        /// <summary>
        /// Return the query expression that will be executed by FluentCRM.
        /// </summary>
        public QueryExpression QueryExpression { get; private set; }

        #region "Entity Selection functions"

        /// <summary>
        /// Id of record to fetch.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IEntitySet Id(Guid id)
        {
            _id = id;
            return this;
        }

        INeedsWhereCriteria IUnknownEntity.Where(string attributeName)
        {
            _whereAttribute = attributeName;
            return this;
        }

        INeedsWhereCriteria IAnotherWhere.Where(string attributeName)
        {
            return (INeedsWhereCriteria) ((IUnknownEntity) this).Where(attributeName);
        }

        /// <summary>
        /// Add criteria that the Where-attribute equals the given value
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="value">Select entity records where the given attribute equals this value.</param>
        /// <typeparam name="T">The type of the value being compared to.</typeparam>
        public IEntitySet Equals<T>(T value)
        {
            Trace("Equals...");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.Equal, value));
            return this;
        }

        /// <summary>
        /// Add criteria that the Where-attribute does not equal the given value
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="value">Select entity records where the given attribute does not equal this value.</param>
        /// <typeparam name="T">The type of the value being compared to.</typeparam>
        public IEntitySet NotEqual<T>(T value)
        {
            Trace("Not Equal...");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.NotEqual, value));
            return this;
        }

        /// <summary>
        /// Add criteria that the Where-attribute begins with the given value
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="s">Select entity records where the given attribute starts with this string value.</param>
        public IEntitySet BeginsWith(string s)
        {
            Trace($"Begins with...{s}");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.BeginsWith, s));
            return this;
        }

        /// <summary>
        /// Add criteria that the Where-attribute matches the given condition and value
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="op">ConditionalOperator from the CRM SDK.</param>
        public IEntitySet Condition(ConditionOperator op )
        {
            Trace($"Operator {op.ToString()}");
            AddCriteria(new ConditionExpression(_whereAttribute, op));
            return this;
        }

        /// <summary>
        /// Add criteria that the Where-attribute matches the given condition and value
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="op">ConditionalOperator from the CRM SDK.</param>
        /// <param name="value">Select entity records where the given attribute and conition matches this value.</param>
        /// <typeparam name="T">The type of the value being compared to.</typeparam>
        public IEntitySet Condition<T>(ConditionOperator op, T value)
        {
            Trace($"Operator {op.ToString()}");
            AddCriteria(new ConditionExpression(_whereAttribute, op, value));
            return this;
        }

        /// <summary>
        /// Add criteria that the Where-attribute is not null
        /// </summary>
        /// <returns>FluentCRM object</returns>
        public IEntitySet IsNotNull
        {
            get
            {
                Trace("IsNotNull...");
                AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.NotNull));
                return this;
            }
        }

        /// <summary>
        /// Add criteria that the Where-attribute is null
        /// </summary>
        /// <returns>FluentCRM object</returns>
        public IEntitySet IsNull
        {
            get
            {
                Trace("IsNull...");
                AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.Null));
                return this;
            }
        }

        /// <summary>
        /// Add criteria that the Where-attribute is in the given set of values
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="inVals">Select entity records where the given attribute value is in the given set of values</param>
        public IEntitySet In<T>(params T[] inVals)
        {
            Trace("IsNull...");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.In, inVals));
            return this;
        }

        /// <summary>
        /// Add criteria that the Where-attribute is greater than the given value
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="value">Select entity records where the given attribute is greater than this value.</param>
        /// <typeparam name="T">The type of the value being compared to.</typeparam>
        public IEntitySet GreaterThan<T>(T value)
        {
            Trace("IsNull...");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.GreaterThan, value));
            return this;
        }

        /// <summary>
        /// Add criteria that the Where-attribute is less than the given value
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="value">Select entity records where the given attribute is less than this value.</param>
        /// <typeparam name="T">The type of the value being compared to.</typeparam>
        public IEntitySet LessThan<T>(T value)
        {
            Trace("IsNull...");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.LessThan, value));
            return this;
        }

        private void AddCriteria(ConditionExpression conditionExpression)
        {
            if (conditionExpression != null)
            {
                Query.Criteria.Filters.First().AddCondition(conditionExpression);
            }
        }

        /// <summary>
        /// Query that will be used to select Entity records on which to operate.
        /// </summary>
        protected QueryExpression Query
        {
            get
            {
                if (QueryExpression == null)
                {
                    QueryExpression = new QueryExpression
                    {
                        EntityName = LogicalName,
                        ColumnSet = new ColumnSet(true),
                        Criteria =
                        {
                            Filters =
                            {
                                new FilterExpression
                                {
                                    FilterOperator = LogicalOperator.And
                                }

                            }
                        },
                        // Note that paging is unreliable, and can return the same record on two different pages
                        PageInfo = new PagingInfo {Count = _pageSize, PageNumber = 1, PagingCookie = null},
                    };
                }

                return QueryExpression;
            }
        }

        #endregion
    }
}