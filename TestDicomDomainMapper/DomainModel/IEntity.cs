using System;
using System.Collections.Generic;
using System.Text;

namespace TestDicomDomainMapper.DomainModel
{
    interface IEntity<KeyType>
    {
        KeyType EntityId { get; }
    }
}
