using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using FluentCRM.Utility;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace FluentCRM
{
    public abstract partial class FluentCRM : ICanExecute
    {
        /// <summary>
        /// Constant value used to cause fluent CRM to fetch all attributes of an entity  **DEPRECATED**
        /// </summary>
        public const string AllColumns = "All Columns";
        private ColumnSet _columnSet = new ColumnSet();
        private int _fetchedEntityCount;
        private int _processedEntityCount;
        private int _actionsCalled;
        private int _updateCount;
        private bool _updateRequired = false;
        private StringBuilder _allArgExceptions = new StringBuilder();
        private readonly List<Action<EntityWrapper>> _beforeEachRecordActions = new List<Action<EntityWrapper>>();
        private readonly List<Action<EntityWrapper>> _afterEachRecordActions = new List<Action<EntityWrapper>>();


        #region "Execute CRM operations and actions"

        /// <summary>
        /// Closure called before each entity is processed.
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="action">Called after the entity has been read with the entity value.</param>
        public ICanExecute BeforeEachRecord(Action<EntityWrapper> action)
        {
            _beforeEachRecordActions.Add(action);
            return this;
        }

        /// <summary>
        /// Closure called after each entity has been read and all closures called (i.e. from UseAttribute)
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="action">Called after the entity has been read with the entity value.</param>
        public ICanExecute AfterEachRecord(Action<EntityWrapper> action)
        {
            _afterEachRecordActions.Add(action);
            return this;
        }


        /// <summary>
        /// Carry out CRM operations described in the FluentCRM object.
        /// </summary>
        /// <param name="preExecute"></param>
        /// <param name="postExecute"></param>
        public void Execute(Action preExecute = null, Action<int, int> postExecute = null)
        {
            var stopwatch = new Stopwatch();
            var moreRecords = true;
            _allArgExceptions = new StringBuilder();

            var cols = _actionList.SelectMany(c => c.Item1).Where(c => c != AllColumns).Distinct().ToArray();
            if (_actionList.SelectMany(c => c.Item1).Any(c => c == AllColumns))
            {
                Trace($"Fetching All columns");
                cols = Array.Empty<string>();
            }
            else
            {
                Trace($"Fetching columns: [{string.Join(", ", cols)}]");
            }

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
                _updateRequired = false;

                if (_distinct && ids.Contains(entity.Id))
                {
                    Trace($"Duplicate Guid(\"{entity.Id}\") found. Ignoring");
                }
                else
                {

                    // Track processed entity Ids to avoid potential duplicates produced by paging.
                    ids.Add(entity.Id);
                    
                    var wrapper = new EntityWrapper(entity, Service,  _traceFunction);
                    // wrapper.Alias
                    _beforeEachRecordActions.ForEach(a => a(wrapper));

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

                    // If any exceptions occurred during attribute processing, raise them all in one go rather than in a piecemeal fashion.
                    if (_allArgExceptions.Length > 0)
                    {
                        throw new InvalidCastException(_allArgExceptions.ToString());
                    }

                    if (_updateRequired)
                    {
                        Trace( $"Updating entity {_update.LogicalName}/{_update.Id} - {String.Join(",", _update.Attributes.Keys)} - {EscapeBraces(String.Join(",", _update.Attributes.Values))}");
                        stopwatch.Restart();
                        if (_dryRunUpdate != null)
                        {
                            _dryRunUpdate(wrapper);
                        }
                        else
                        {
                            Service.Update(_update);
                        }

                        _updateCount++;
                        Timer($"Updated in {stopwatch.Elapsed.TotalSeconds}s");
                    }
                    _afterEachRecordActions.ForEach(a => a(wrapper));
                }
                _processedEntityCount++;
            }


            return hasMore;
        }

        /// <summary>
        /// Escape any braces included in a parameter inserted into a format string to stop it being interpreted as a format specified.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string EscapeBraces(string input)
        {
            return input.Replace("{", "{{").Replace("}", "}}");
        }

        private void PrepareFetch(params string[] fields)
        {
            _columnSet = new ColumnSet(fields);
            if (fields.Length == 0)
            {
                _columnSet = new ColumnSet(true); // Get all attributes.
            }

            if (QueryExpression != null)
            {
                QueryExpression.ColumnSet = _columnSet;
                QueryExpression.Orders.AddRange(_orders);
                QueryExpression.PageInfo.PagingCookie = null;
                QueryExpression.PageInfo.PageNumber = 1;
            }


            if (_top != null)
            {
                QueryExpression.TopCount = _top;
                if (_orders.Count == 0)
                {
                    Trace($"Warning: Top count of {_top} specified with no order criteria (at top level)");
                }
                QueryExpression.PageInfo = null;
            }

            // Look after any criteria in linked entities.
            PrepareLinkedCriteria();
        }

        /// <summary>
        /// Fetch records from CRM.
        /// Return a boolean indicating if there are more paged records still to be fetched.
        /// </summary>
        /// <returns></returns>
        private bool FetchAll()
        {
            if (Service == null)
            {
                throw new ArgumentException("CRM Organization service not specified");
            }
            _entities = new Collection<Entity> ();
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
                    HasErrors = true;
                    LastError = ex.Message;
                    if (_throwAllErrors) throw;
                }
            }
            else if (QueryExpression != null)
            {
                EntityCollection result;
                try
                {
                    // Trace($"_queryexpression = {XMLUtilities.ToString(_queryExpression)}");
                    result = Service.RetrieveMultiple(QueryExpression);
                    _entities = result.Entities;
                    if ((QueryExpression.TopCount ?? 0) == 0)
                    {
                        QueryExpression.PageInfo.PageNumber++;
                        QueryExpression.PageInfo.PagingCookie = result.PagingCookie;
                    }

                    return result.MoreRecords;
                }
                catch (Exception ex)
                {
                    Trace($"Error in RetrieveMultiple {ex.Message}");
                    Trace(ex.StackTrace);
                    HasErrors = true;
                    LastError = ex.Message;
                    if (ex.Message.Contains("entity doesn't contain attribute")) throw;
                    if (_throwAllErrors) throw;
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