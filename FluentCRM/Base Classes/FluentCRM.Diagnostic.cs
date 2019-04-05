using System;

namespace FluentCRM
{
    public abstract partial class FluentCRM
    {
        private Action<string> _traceFunction;
        private Action<string> _timerFunction;

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
            _traceFunction?.Invoke(String.Format(format, args));
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

        #endregion
    }
}