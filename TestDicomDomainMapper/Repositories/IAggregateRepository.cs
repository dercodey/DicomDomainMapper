using System.Threading.Tasks;

namespace TestDicomDomainMapper.Repositories
{
    /// <summary>
    /// interface that represents an aggregate repository
    /// </summary>
    /// <typeparam name="TEntity">aggregate entity type</typeparam>
    /// <typeparam name="TKey">aggregate key type</typeparam>
    public interface IAggregateRepository<TEntity, TKey> 
        where TEntity : Seedworks.IAggregateRoot<TKey>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="forKey"></param>
        /// <returns></returns>
        TEntity GetAggregateForKey(TKey forKey);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedAggregate"></param>
        /// <returns></returns>
        Task UpdateAsync(TEntity updatedAggregate);
    }
}
