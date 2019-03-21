namespace FluentCRM
{
    /// <summary>
    ///  Interface to use after we have introduced a follow on Where() clause with an "And"
    /// </summary>
    public interface IAnotherWhere
    {
        /// <summary>
        /// Select entity records where the given attribute meets some condition
        /// </summary>
        /// <returns>FluentCRM object</returns>
        /// <param name="attributeName">Logical name that is the subject of the condition</param>
        INeedsWhereCriteria Where(string attributeName);
    }
}
