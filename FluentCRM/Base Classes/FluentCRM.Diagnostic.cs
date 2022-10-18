using System;
using Microsoft.Xrm.Sdk;

namespace FluentCRM
{
    public abstract partial class FluentCRM
    {
        private Action<string> _traceFunction;
        private Action<string> _timerFunction;
        private bool _throwAllErrors = false;
        private Action<EntityWrapper> _dryRunCreate;
        private Action<EntityWrapper> _dryRunUpdate;
        private Action<EntityReference> _dryRunDelete;

        #region "Diagnostic methods"

        /// <summary>
        /// Allow injection of Tracing function to diagnose issues with the library.
        /// </summary>
        /// <param name="action">Closure to be called with any trace messages.</param>
        /// <returns></returns>
        IUnknownEntity IUnknownEntity.Trace(Action<string> action)
        {
            _traceFunction = action;
            return this;
        }

        ICanExecute ICanExecute.Trace(Action<string> action)
        {
            return (ICanExecute) ((IUnknownEntity) this).Trace(action);
        }

        IEntitySet IEntitySet.Trace(Action<string> action)
        {
            return (IEntitySet) ((IUnknownEntity) this).Trace(action);
        }

        /// <summary>
        /// Function to be called internally to emit any tracing.
        /// Uses String.Format() style args.
        /// </summary>
        /// <returns></returns>
        protected void Trace(string format, params object[] args)
        {
            _traceFunction?.Invoke(SafeFormat(format, args));
        }

        IEntitySet IEntitySet.ThrowAllErrors(bool throwAll )
        {
            _throwAllErrors = throwAll;
            return this;
        }

       /// <summary>
        /// Output messages regarding the excution time of create, read and update operations.
        /// </summary>
        /// <param name="timerAction">Action function that can be used to log timer messasges</param>
        /// <returns>FluentCRM Object</returns>
        public IUnknownEntity Timer(Action<string> timerAction)
        {
            _timerFunction = timerAction;

            return this;
        }

        /// <summary>
        ///  Called internally to trace the timing of various parts of the processing.
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        private void Timer(string format, params object[] args)
        {
            _timerFunction?.Invoke(string.Format(format, args));
        }

        ICanExecute ICanExecute.Timer(Action<string> timerAction)
        {
            return (ICanExecute) ((IUnknownEntity) this).Timer(timerAction);
        }

        IEntitySet IEntitySet.Timer(Action<string> timerAction)
        {
            return (IEntitySet) ((IUnknownEntity) this).Timer(timerAction);
        }

        /// <summary>
        /// TODO: 
        /// Read data, but make no actual changes to CRM.
        /// By default output log messages to Trace and do no updates, optionally call action functions with to-be-updated data.
        /// </summary>
        /// <param name="onCreate">Function called instead of a create action.</param>
        /// <param name="onUpdate">Function called instead of an update action.</param>
        /// <param name="onDelete">Function called instead of a delete action.</param>
        /// <returns></returns>
        ICanExecute ICanExecute.DryRun(Action<EntityWrapper> onCreate =null, Action<EntityWrapper> onUpdate =null,
            Action<EntityReference> onDelete =null)
        {
            _dryRunCreate = onCreate;
            _dryRunUpdate = onUpdate;
            _dryRunDelete = onDelete;
            return this;
        }

        /// <summary>
        /// Safely format a string avoiding issues where the format string may contain {} that would otherwise break this.
        /// </summary>
        /// <param name=""></param>
        /// <param name="args"></param>
        /// <returns></returns>
        private string SafeFormat(string format, object[] args)
        {
            var rv = format;
            try
            {
                rv = string.Format(format, args);
            }
            catch (FormatException ex) {
            }

            return rv;
        }
        #endregion
    }
}