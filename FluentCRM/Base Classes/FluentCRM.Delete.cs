using System;

namespace FluentCRM
{
    public abstract partial class FluentCRM
    {
        #region "Delete selected entity records"

        ICanExecute ICanExecute.Delete()
        {
            Trace($"Delete Entity/ies");

            // Add action to delete the selected set of entities.
            _actionList.Add(
                new Tuple<string[], Func<EntityWrapper, string, bool?>>(
                    new string[] { $"{LogicalName}id" },
                        (wrapper, id) =>
                        {
                            Trace($"Deleting {wrapper.Entity.LogicalName} {wrapper.Entity.Id}");
                            Service.Delete( wrapper.Entity.LogicalName, wrapper.Entity.Id);
                            _updateRequired = false;
                            return true;
                        }
                        ));

            return this;
        }

        ICanExecute IEntitySet.Delete()
        {
            return ((ICanExecute) this).Delete();
        }

        #endregion
    }
}