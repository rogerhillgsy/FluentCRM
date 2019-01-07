using System;

namespace FluentCRM
{
    public abstract partial class FluentCRM
    {
        #region "Delete selected entity records"

        ICanExecute ICanExecute.Delete()
        {
            throw new NotImplementedException();
        }

        ICanExecute IEntitySet.Delete()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}