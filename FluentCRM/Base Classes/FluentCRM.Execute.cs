using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public abstract partial class FluentCRM : ICanExecute
    {
        public const string AllColumns = "All Columns";
        private ColumnSet _columnSet = new ColumnSet();
        private int _fetchedEntityCount;
        private int _processedEntityCount;
        private int _actionsCalled;
        private int _updateCount;
        private bool _updateRequired = false;
        private readonly List<Action<EntityWrapper>> _beforeEachEntityActions = new List<Action<EntityWrapper>>();
        private readonly List<Action<EntityWrapper>> _afterEachEntityActions = new List<Action<EntityWrapper>>();


        #region "Execute CRM operations and actions"

        public ICanExecute BeforeEachEntity(Action<EntityWrapper> action)
        {
            _beforeEachEntityActions.Add(action);
            return this;
        }

        public ICanExecute AfterEachEntity(Action<EntityWrapper> action)
        {
            _afterEachEntityActions.Add(action);
            return this;
        }


        /// <summary>
        /// Carry out CRM operations described in the FLuentCRM object.
        /// </summary>
        /// <param name="preExecute"></param>
        /// <param name="postExecute"></param>
        public void Execute(Action preExecute = null, Action<int, int> postExecute = null)
        {
            var stopwatch = new Stopwatch();
            var moreRecords = true;

            var cols = _actionList.SelectMany(c => c.Item1).Where(c => c != AllColumns).Distinct().ToArray();
            Trace( $"Fetching columns: [{string.Join(", ", cols)}]");

            preExecute?.Invoke();

            PrepareFetch(cols);
            var ids = new HashSet<Guid>();

            while (moreRecords)
            {
                moreRecords = FetchPage(stopwatch, ids);
            } 

            _postExecuteActions.ForEach(a => a.Invoke());
            postExecute?.Invoke(_actionsCalled, _updateCount );
            Trace($"Fetched {_fetchedEntityCount} Entities, Processed {_processedEntityCount} entities, Action Callbacks {_actionsCalled}, Updates {_updateCount}");
        }

        /// <summary>
        /// Process data a page at a time, returning a boolean to indicate if there is more data to process.
        /// </summary>
        /// <returns></returns>
        private bool FetchPage(Stopwatch stopwatch, ICollection<Guid> ids)
        {
            var hasMore = FetchAll();

            Trace($"Fetched {_entities.Count} entities");
            Timer($"Fetched {_entities.Count} records in {stopwatch.Elapsed.TotalSeconds}s");
            _fetchedEntityCount += _entities.Count;

            foreach (var entity in _entities.ToList())
            {
                _update = new Entity(entity.LogicalName, entity.Id);

                if (_distinct && ids.Contains(entity.Id))
                {
                    Trace($"Duplicate Guid(\"{entity.Id}\") found. Ignoring");
                }
                else
                {

                    // Track processed entity Ids to avoid potential duplicates produced by paging.
                    ids.Add(entity.Id);
                    
                    var wrapper = new EntityWrapper(entity, Service, _traceFunction);
                    _beforeEachEntityActions.ForEach(a => a(wrapper));

                    _actionList.ForEach(a =>
                        {
                            foreach (var attribute in a.Item1)
                            {
                                var result = a.Item2?.Invoke(wrapper, attribute);
                                if (result.HasValue && result.Value)
                                {
                                    _actionsCalled++;
                                    return;
                                }
                            }
                        }
                        );

                    if (_updateRequired)
                    {
                        Trace( $"Updating entity {_update.LogicalName}/{_update.Id} - {String.Join(",", _update.Attributes.Keys)} - {String.Join(",", _update.Attributes.Values)}");
                        stopwatch.Restart();
                        Service.Update(_update);
                        _updateCount++;
                        Timer($"Updated in {stopwatch.Elapsed.TotalSeconds}s");
                    }
                    _afterEachEntityActions.ForEach(a => a(wrapper));
                }
                _processedEntityCount++;
            }


            return hasMore;
        }

        private void PrepareFetch(params string[] fields)
        {
            _columnSet = new ColumnSet(fields);
            if (fields.Length == 0)
            {
                _columnSet = new ColumnSet(true); // Get all attributes.
            }

            if (_queryExpression != null)
            {
                _queryExpression.ColumnSet = _columnSet;
                _queryExpression.Orders.AddRange(_orders);
                _queryExpression.PageInfo.PagingCookie = null;
                _queryExpression.PageInfo.PageNumber = 1;
            }

            // Look after any criteria in linked entities.
            foreach (var linkedEntity in LinkedEntities)
            {
                linkedEntity.PrepareLinkedCriteria();
            }
        }

        /// <summary>
        /// Fetch records from CRM.
        /// Return a boolean indicating if there are more paged records still to be fetched.
        /// </summary>
        /// <returns></returns>
        private bool FetchAll()
        {
            if (!Guid.Empty.Equals(_id))
            {
                try
                {
                    var entity = Service.Retrieve(LogicalName, _id, _columnSet);
                    _entities = new Collection<Entity> {entity};
                }
                catch (Exception ex)
                {
                    Trace($"Error fetching {LogicalName} id {_id} message: {ex.Message}");
                    _entities = new Collection<Entity>();
                }
            }
            else if (_queryExpression != null)
            {
                EntityCollection result;
                try
                {
                    result = Service.RetrieveMultiple(_queryExpression);
                    _entities = result.Entities;
                    _queryExpression.PageInfo.PageNumber++;
                    _queryExpression.PageInfo.PagingCookie = result.PagingCookie;
                    return result.MoreRecords;
                }
                catch (Exception ex)
                {
                    Trace($"Error in RetrieveMultiple {ex.Message}");
                } 
            }
            else
            {
                Trace( "No id and no query specified");
            }

            return false;
        }


        #endregion
    }
}