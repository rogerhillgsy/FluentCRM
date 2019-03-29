using System;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public abstract partial class FluentCRM
    {
        private bool _distinct = false;
        private int _pageSize = 5000;

        #region "Ordering and Counting"

        ICanExecute ICanExecute.Distinct()
        {
            _distinct = true;
            return this;
        }

        IEntitySet IEntitySet.Distinct()
        {
            return (IEntitySet) ((ICanExecute) this).Distinct();
        }

        ICanExecute ICanExecute.PageSize(int i)
        {
            _pageSize = i;
            return this;
        }

        private ICanExecute Order(string attribute, OrderType orderingType)
        {
            _orders.Add(new OrderExpression
            {
                AttributeName = attribute,
                OrderType = orderingType
            });
            return this;
        }

        ICanExecute ICanExecute.OrderByDesc(string attribute)
        {
            return Order(attribute, OrderType.Descending);
        }

        /// <summary>
        /// Used to indicate that an additional selection criteria will be applied to the set of selected entities.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        public IAnotherWhere And
        {
            get
            {
                Trace("And...");
                return this;
            }
        }

        ICanExecute ICanExecute.OrderByAsc(string attribute)
        {
            return Order(attribute, OrderType.Ascending);
        }

        IEntitySet IEntitySet.OrderByAsc(string attribute)
        {
            return (IEntitySet) Order(attribute, OrderType.Ascending);
        }

        IEntitySet IEntitySet.OrderByDesc(string attribute)
        {
            return (IEntitySet) Order(attribute, OrderType.Descending);
        }

        ICanExecute ICanExecute.Exists(Action<bool> action)
        {
            _actionList.Add(new Tuple<string[], Func<EntityWrapper, string, bool?>>(
                new string[] {"createdon"},
                (entity, c) => true));
            _postExecuteActions.Add(() =>
            {
                var exists = (_entities?.Count > 0);
                Trace($"Called exists(): {exists}");

                action?.Invoke(exists);
            });
            return this;
        }

        ICanExecute ICanExecute.Exists(Action whenTrue, Action whenFalse)
        {
            return ((ICanExecute) this).Exists(c =>
            {
                if (c)
                {
                    whenTrue?.Invoke();
                }
                else
                {
                    whenFalse?.Invoke();
                }
            });
        }

        ICanExecute IEntitySet.Count(Action<int?> action)
        {
            _postExecuteActions.Add(() =>
                {
                    Trace($"Count={_processedEntityCount}");
                    action(_processedEntityCount);
                }
            );
            return this;
        }

        ICanExecute ICanExecute.Count(Action<int?> action)
        {
            return ((IEntitySet) this).Count(action);
        }

        ICanExecute IEntitySet.Exists(Action<bool> action)
        {
            return ((ICanExecute) this).Exists(action);
        }

        ICanExecute IEntitySet.Exists(Action whenTrue, Action whenFalse)
        {
            return ((ICanExecute) this).Exists(whenTrue, whenFalse);
        }

        #endregion
    }
}