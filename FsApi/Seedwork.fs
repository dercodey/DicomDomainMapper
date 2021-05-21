module Seedwork

type IAggregateRoot<'key> =
    abstract RootKey : 'key

type IEntity<'key> =
    abstract Key : 'key