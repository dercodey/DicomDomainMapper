using System;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.EquivalencyExpression;

using DomainModel = Dicom.Domain.Model;

namespace Dicom.Infrastructure.Mappers
{
    /// <summary>
    /// wrapper for Automapper configured to map between domain model and EF model
    /// </summary>
    public static class DomainMapper
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
                    cfg.AddCollectionMappers();

                    cfg.CreateMap<DomainModel.DicomSeries, EFModel.DicomSeries>()
                        .EqualityComparison((dmDicomSeries, efDicomSeries) =>
                            efDicomSeries.SeriesInstanceUid == dmDicomSeries.SeriesInstanceUid.ToString())
                        .ForMember(s => s.ID, opt => opt.Ignore())
                        .ForMember(s => s.SeriesInstanceUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

                    cfg.CreateMap<EFModel.DicomSeries, DomainModel.DicomSeries>()
                        .ForCtorParam("seriesInstanceUid",
                            opt => opt.MapFrom(src =>
                                new DomainModel.DicomUid(src.SeriesInstanceUid)));

                    cfg.CreateMap<DomainModel.DicomInstance, EFModel.DicomInstance>()
                        .EqualityComparison((dmDicomInstance, efDicomInstance) =>
                            efDicomInstance.SopInstanceUid == dmDicomInstance.SopInstanceUid.ToString())
                        .ForMember(s => s.ID, opt => opt.Ignore())
                        .ForMember(s => s.DicomSeriesId, opt => opt.Ignore())
                        .ForMember(s => s.DicomSeries, opt => opt.Ignore())
                        .ForMember(s => s.SopInstanceUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

                    cfg.CreateMap<EFModel.DicomInstance, DomainModel.DicomInstance>()
                        .ForCtorParam("sopInstanceUid",
                            opt => opt.MapFrom(src =>
                                new DomainModel.DicomUid(src.SopInstanceUid)));

                    cfg.CreateMap<DomainModel.DicomAttribute, EFModel.DicomAttribute>()
                        .EqualityComparison((dmDicomAttribute, efDicomAttribute) =>
                            efDicomAttribute.DicomTag == dmDicomAttribute.DicomTag.ToString())
                        .ForMember(s => s.ID, opt => opt.Ignore())
                        .ForMember(s => s.DicomInstanceId, opt => opt.Ignore())
                        .ForMember(s => s.DicomInstance, opt => opt.Ignore())
                        .ForMember(s => s.DicomTag,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomTag>()));

                    cfg.CreateMap<EFModel.DicomAttribute, DomainModel.DicomAttribute>()
                        .ForMember(dmDicomAttribute => dmDicomAttribute.DicomTag, opt => opt.Ignore())
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