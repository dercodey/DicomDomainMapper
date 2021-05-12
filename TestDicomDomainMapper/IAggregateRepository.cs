using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestDicomDomainMapper
{
    interface IAggregateRepository<TEntity, TKey> 
        where TEntity : DomainModel.IAggregateRoot<TKey>
    {
        TEntity GetAggregateForKey(TKey forKey);

        Task UpdateAsync(TEntity updatedAggregate);
    }
}
