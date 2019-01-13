using System;
using System.Linq;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public abstract partial class FluentCRM
    {
        private QueryExpression _queryExpression;

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

        public IEntitySet Equals<T>(T value)
        {
            Trace("Equals...");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.Equal, value));
            return this;
        }

        public IEntitySet NotEqual<T>(T value)
        {
            Trace("Not Equal...");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.NotEqual, value));
            return this;
        }

        public IEntitySet BeginsWith(string s)
        {
            Trace($"Begins with...{s}");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.BeginsWith, s));
            return this;
        }

        public IEntitySet Condition<T>(ConditionOperator op, T value)
        {
            Trace($"Operator {op.ToString()}");
            AddCriteria(new ConditionExpression(_whereAttribute, op, value));
            return this;
        }

        public IEntitySet IsNotNull
        {
            get
            {
                Trace("IsNotNull...");
                AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.NotNull));
                return this;
            }
        }

        public IEntitySet IsNull
        {
            get
            {
                Trace("IsNull...");
                AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.Null));
                return this;
            }
        }

        public IEntitySet In<T>(params T[] inVals)
        {
            Trace("IsNull...");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.In, inVals));
            return this;
        }

        public IEntitySet GreaterThan<T>(T value)
        {
            Trace("IsNull...");
            AddCriteria(new ConditionExpression(_whereAttribute, ConditionOperator.GreaterThan, value));
            return this;
        }

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

        protected QueryExpression Query
        {
            get
            {
                if (_queryExpression == null)
                {
                    _queryExpression = new QueryExpression
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

                return _queryExpression;
            }
        }

        #endregion
    }
}