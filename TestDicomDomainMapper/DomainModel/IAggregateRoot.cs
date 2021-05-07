using System;
using System.Collections.Generic;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    interface IAggregateRoot<KeyType>
    {
        KeyType RootId { get; }
    }
}
