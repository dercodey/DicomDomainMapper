module MappingProfiles

open AutoMapper

type DomainMappingProfile() as this =
    inherit Profile()

    do this.CreateMap<DomainModel.DicomSeries, EFModel.DicomSeries>() |> ignore

type AbstractionMappingProfile() as this =
    inherit Profile()

    // do this.CreateMap<DomainModel.DicomInstance, Abstractions.DicomInstance>() |> ignore
    do this.CreateMap<DomainModel.DicomSeries, Abstractions.DicomSeries>() |> ignore
