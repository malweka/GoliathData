namespace Goliath.Security
{
    public interface IUniqueNumberGenerator
    {
        /// <summary>
        /// Gets the next id.
        /// </summary>
        /// <returns></returns>
        long GetNextId();
    }
}