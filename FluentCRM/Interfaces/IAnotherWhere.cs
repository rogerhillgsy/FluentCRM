namespace FluentCRM
{
    /// <summary>
    ///  Interface to use after we have introduced a follow on Where() claus with an "And"
    /// </summary>
    public interface IAnotherWhere
    {
        INeedsWhereCriteria Where(string attributeName);
    }
}
