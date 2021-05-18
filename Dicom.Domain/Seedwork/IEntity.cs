namespace Elekta.Capability.Dicom.Domain.Seedwork
{
    /// <summary>
    /// interface representing an ordinary (non-aggregate) entity
    /// </summary>
    /// <typeparam name="KeyType"></typeparam>
    public interface IEntity<KeyType>
    {
        /// <summary>
        /// alias for the key property
        /// </summary>
        KeyType EntityKey { get; }
    }
}
