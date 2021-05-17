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
                        .ForMember(efDicomSeries => efDicomSeries.ID, opt => opt.Ignore())
                        .ForMember(efDicomSeries => efDicomSeries.SeriesInstanceUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

                    cfg.CreateMap<EFModel.DicomSeries, DomainModel.DicomSeries>()
                        .ForMember(dmDicomSeries => dmDicomSeries.SeriesInstanceUid, opt => opt.Ignore())
                        .ForCtorParam("seriesInstanceUid",
                            opt => opt.MapFrom(dmDicomSeries =>
                                new DomainModel.DicomUid(dmDicomSeries.SeriesInstanceUid)));

                    cfg.CreateMap<DomainModel.DicomInstance, EFModel.DicomInstance>()
                        .EqualityComparison((dmDicomInstance, efDicomInstance) =>
                            efDicomInstance.SopInstanceUid == dmDicomInstance.SopInstanceUid.ToString())
                        .ForMember(efDicomInstance => efDicomInstance.ID, opt => opt.Ignore())
                        .ForMember(efDicomInstance => efDicomInstance.DicomSeriesId, opt => opt.Ignore())
                        .ForMember(efDicomInstance => efDicomInstance.DicomSeries, opt => opt.Ignore())
                        .ForMember(efDicomInstance => efDicomInstance.SopInstanceUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

                    cfg.CreateMap<EFModel.DicomInstance, DomainModel.DicomInstance>()
                        .ForMember(dmDicomInstance => dmDicomInstance.SopInstanceUid, opt => opt.Ignore())
                        .ForCtorParam("sopInstanceUid",
                            opt => opt.MapFrom(dmDicomInstance =>
                                new DomainModel.DicomUid(dmDicomInstance.SopInstanceUid)));

                    cfg.CreateMap<DomainModel.DicomElement, EFModel.DicomElement>()
                        .EqualityComparison((dmDicomAttribute, efDicomAttribute) =>
                            efDicomAttribute.DicomTag == dmDicomAttribute.DicomTag.ToString())
                        .ForMember(efDicomAttribute => efDicomAttribute.ID, opt => opt.Ignore())
                        .ForMember(efDicomAttribute => efDicomAttribute.DicomInstanceId, opt => opt.Ignore())
                        .ForMember(efDicomAttribute => efDicomAttribute.DicomInstance, opt => opt.Ignore())
                        .ForMember(efDicomAttribute => efDicomAttribute.DicomTag,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomTag>()));

                    cfg.CreateMap<EFModel.DicomElement, DomainModel.DicomElement>()
                        .ForMember(dmDicomAttribute => dmDicomAttribute.DicomTag, opt => opt.Ignore())
                        .ForCtorParam("dicomTag",
                            opt => opt.MapFrom(dmDicomAttribute =>
                                DomainModel.DicomTag.GetTag(dmDicomAttribute.DicomTag)));
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