using System;
using AutoMapper;
using DomainModel = Elektrum.Capability.Dicom.Domain.Model;
using AbstractionModel = Elektrum.Capability.Dicom.Abstractions.Models;

namespace Elektrum.Capability.Dicom.Api.Mappers
{
    public class AbstractionMapperProfile : Profile
    {
        public AbstractionMapperProfile()
        {
            CreateMap<DomainModel.DicomSeries, AbstractionModel.DicomSeries>()
                .ForMember(abDicomSeries => abDicomSeries.SeriesInstanceUid,
                    opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

            CreateMap<AbstractionModel.DicomSeries, DomainModel.DicomSeries>()
                .ForMember(dmDicomSeries => dmDicomSeries.SeriesInstanceUid, opt => opt.Ignore())
                .ForMember(dmDicomSeries => dmDicomSeries.DicomInstances, opt => opt.Ignore())
                .ForCtorParam("seriesInstanceUid",
                    opt => opt.MapFrom(src =>
                        new DomainModel.DicomUid(src.SeriesInstanceUid)));

            CreateMap<DomainModel.DicomInstance, AbstractionModel.DicomInstance>()
                .ForMember(abDicomInstance => abDicomInstance.SopInstanceUid,
                    opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

            CreateMap<AbstractionModel.DicomInstance, DomainModel.DicomInstance>()
                .ForMember(dmDicomSeries => dmDicomSeries.SopInstanceUid, opt => opt.Ignore())
                .ForCtorParam("sopInstanceUid",
                    opt => opt.MapFrom(src =>
                        new DomainModel.DicomUid(src.SopInstanceUid)));

            CreateMap<DomainModel.DicomAttribute, AbstractionModel.DicomAttribute>()
                .ForMember(abDicomElement => abDicomElement.DicomTag,
                    opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomTag>()));

            CreateMap<AbstractionModel.DicomAttribute, DomainModel.DicomAttribute>()
                .ForMember(dmDicomElement => dmDicomElement.DicomTag, opt => opt.Ignore())
                .ForCtorParam("dicomTag",
                    opt => opt.MapFrom(src =>
                        DomainModel.DicomTag.GetTag(src.DicomTag)));
        }
    }

    /// <summary>
    /// formatter to use ToString
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ToStringFormatter<T> : IValueConverter<T, string>
    {
        string IValueConverter<T, string>.Convert(T sourceMember, ResolutionContext context)
        {
            return sourceMember.ToString();
        }
    }
}
