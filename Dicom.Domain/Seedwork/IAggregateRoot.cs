namespace Elektrum.Capability.Dicom.Domain.Seedwork
{
    /// <summary>
    /// interface representing an aggregate root with a particular key type
    /// </summary>
    /// <typeparam name="KeyType"></typeparam>
    public interface IAggregateRoot<KeyType>
    {
        /// <summary>
        /// the root id is an alias for whatever property represents the root key
        /// </summary>
        KeyType RootKey { get; }
    }
}
