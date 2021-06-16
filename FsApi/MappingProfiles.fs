module MappingProfiles

open AutoMapper

type ToStringFormatter<'t>() =
    interface IValueConverter<'t, string> with
        member __.Convert (sourceMember:'t, context:ResolutionContext) =
            sourceMember.ToString()

let dicomUidStringFormatter = 
    ToStringFormatter<DomainModel.DicomUid>() 
    :> IValueConverter<DomainModel.DicomUid, string>

type DomainMappingProfile() as this =
    inherit Profile()

    do this.CreateMap<DomainModel.DicomAttribute, EFModel.DicomAttribute>() 
        |> ignore
    do this.CreateMap<DomainModel.DicomInstance, EFModel.DicomInstance>() 
        |> ignore
    do this.CreateMap<DomainModel.DicomSeries, EFModel.DicomSeries>() 
        |> ignore


type AbstractionMappingProfile() as this =
    inherit Profile()

    do this.CreateMap<DomainModel.DicomAttribute, Abstractions.DicomAttribute>() 
        |> ignore
    do this.CreateMap<DomainModel.DicomInstance, Abstractions.DicomInstance>() 
        |> ignore
    do this.CreateMap<DomainModel.DicomSeries, Abstractions.DicomSeries>()
        .ForMember((fun abDicomSeries -> abDicomSeries.SeriesInstanceUid), 
            (fun (opt:IMemberConfigurationExpression<DomainModel.DicomSeries, Abstractions.DicomSeries, string>) -> 
                dicomUidStringFormatter |> opt.ConvertUsing))
            |> ignore
