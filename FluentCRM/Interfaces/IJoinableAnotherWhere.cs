namespace FluentCRM
{
    public interface IJoinableAnotherWhere
    {
        /// <summary>
        /// Select entity records where the given attribute meets some condition
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeName">Logical name that is the subject of the condition</param>
        IJoinableNeedsWhereCriteria Where(string attributeName);
    }
}