using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Seedwork = Elekta.Capability.Dicom.Domain.Seedwork;

namespace Elekta.Capability.Dicom.Application.Repositories
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
        /// gets the aggregate with the given key
        /// </summary>
        /// <param name="selectFunc">func to match keys</param>
        /// <returns>the matching aggregate</returns>
        IEnumerable<TEntity> SelectAggregates(Func<TEntity, bool> selectFunc);

        /// <summary>
        /// updates an aggregate (or creates if it is new)
        /// </summary>
        /// <param name="updatedAggregate">aggregate to be updated</param>
        /// <returns>Task representing the updation operation</returns>
        Task UpdateAsync(TEntity updatedAggregate);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forKey"></param>
        /// <returns></returns>
        Task RemoveAsync(TKey forKey);
    }
}
