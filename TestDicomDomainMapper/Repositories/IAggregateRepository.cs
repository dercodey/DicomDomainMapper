using System.Threading.Tasks;
using Seedwork = Dicom.Domain.Seedwork;

namespace TestDicomDomainMapper.Repositories
{
    /// <summary>
    /// interface that represents an aggregate repository
    /// </summary>
    /// <typeparam name="TEntity">aggregate entity type</typeparam>
    /// <typeparam name="TKey">aggregate key type</typeparam>
    public interface IAggregateRepository<TEntity, TKey> 
        where TEntity : Seedwork.IAggregateRoot<TKey>
    {
        /// <summary>
        /// gets the aggregate with the given key
        /// </summary>
        /// <param name="forKey">key to match</param>
        /// <returns>the matching aggregate</returns>
        TEntity GetAggregateForKey(TKey forKey);

        /// <summary>
        /// updates an aggregate (or creates if it is new)
        /// </summary>
        /// <param name="updatedAggregate">aggregate to be updated</param>
        /// <returns>Task representing the updation operation</returns>
        Task UpdateAsync(TEntity updatedAggregate);
    }
}
