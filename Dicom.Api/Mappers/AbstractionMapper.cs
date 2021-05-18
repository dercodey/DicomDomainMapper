using System;
using AutoMapper;
using DomainModel = Elekta.Capability.Dicom.Domain.Model;
using AbstractionModel = Elekta.Capability.Dicom.Abstractions.Models;

namespace Elekta.Capability.Dicom.Api.Mappers
{
    public class AbstractionMapper
    {
        /// <summary>
        /// gets (and creates) the mapper
        /// </summary>
        /// <returns></returns>
        public static IMapper GetMapper()
        {
            if (mapper == null)
            {
                mapper = CreateMapper(cfg =>
                {
                    cfg.CreateMap<DomainModel.DicomSeries, AbstractionModel.DicomSeries>()
                        .ForMember(abDicomSeries => abDicomSeries.SeriesInstanceUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

                    cfg.CreateMap<AbstractionModel.DicomSeries, DomainModel.DicomSeries>()
                        .ForMember(dmDicomSeries => dmDicomSeries.SeriesInstanceUid, opt => opt.Ignore())
                        .ForMember(dmDicomSeries => dmDicomSeries.DicomInstances, opt => opt.Ignore())
                        .ForCtorParam("seriesInstanceUid",
                            opt => opt.MapFrom(src =>
                                new DomainModel.DicomUid(src.SeriesInstanceUid)));

                    cfg.CreateMap<DomainModel.DicomInstance, AbstractionModel.DicomInstance>()
                        .ForMember(abDicomInstance => abDicomInstance.SopInstanceUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

                    cfg.CreateMap<AbstractionModel.DicomInstance, DomainModel.DicomInstance>()
                        .ForMember(dmDicomSeries => dmDicomSeries.SopInstanceUid, opt => opt.Ignore())
                        .ForCtorParam("sopInstanceUid",
                            opt => opt.MapFrom(src =>
                                new DomainModel.DicomUid(src.SopInstanceUid)));

                    cfg.CreateMap<DomainModel.DicomElement, AbstractionModel.DicomElement>()
                        .ForMember(abDicomElement => abDicomElement.DicomTag,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomTag>()));

                    cfg.CreateMap<AbstractionModel.DicomElement, DomainModel.DicomElement>()
                        .ForMember(dmDicomElement => dmDicomElement.DicomTag, opt => opt.Ignore())
                        .ForCtorParam("dicomTag",
                            opt => opt.MapFrom(src =>
                                DomainModel.DicomTag.GetTag(src.DicomTag)));
                });
            }

            return mapper;
        }

        static IMapper CreateMapper(Action<IMapperConfigurationExpression> cfg)
        {
            var map = new MapperConfiguration(cfg);
            map.CompileMappings();

            var mapper = map.CreateMapper();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            return mapper;
        }

        static IMapper mapper = null;
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
