using System;
using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.EquivalencyExpression;

using DomainModel = Elekta.Capability.Dicom.Domain.Model;

namespace Elekta.Capability.Dicom.Infrastructure.Mappers
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
                        // set up collection identity
                        .EqualityComparison((dmDicomSeries, efDicomSeries) =>
                            efDicomSeries.SeriesInstanceUid == dmDicomSeries.SeriesInstanceUid.ToString())
                        /// ignore primary key and foreign keys
                        .ForMember(efDicomSeries => efDicomSeries.ID, opt => opt.Ignore())
                        // set up conversion from value objects
                        .ForMember(efDicomSeries => efDicomSeries.SeriesInstanceUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

                    cfg.CreateMap<EFModel.DicomSeries, DomainModel.DicomSeries>()
                        // set up conversion for value objects
                        .ForMember(dmDicomSeries => dmDicomSeries.SeriesInstanceUid, opt => opt.Ignore())
                        .ForCtorParam("seriesInstanceUid",
                            opt => opt.MapFrom(dmDicomSeries =>
                                new DomainModel.DicomUid(dmDicomSeries.SeriesInstanceUid)));

                    cfg.CreateMap<DomainModel.DicomInstance, EFModel.DicomInstance>()
                        // set up collection identity
                        .EqualityComparison((dmDicomInstance, efDicomInstance) =>
                            efDicomInstance.SopInstanceUid == dmDicomInstance.SopInstanceUid.ToString())
                        /// ignore primary key and foreign keys
                        .ForMember(efDicomInstance => efDicomInstance.ID, opt => opt.Ignore())
                        .ForMember(efDicomInstance => efDicomInstance.DicomSeriesId, opt => opt.Ignore())
                        .ForMember(efDicomInstance => efDicomInstance.DicomSeries, opt => opt.Ignore())
                        // set up conversion for value objects
                        .ForMember(efDicomInstance => efDicomInstance.SopInstanceUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

                    cfg.CreateMap<EFModel.DicomInstance, DomainModel.DicomInstance>()
                        // set up conversion for value objects
                        .ForMember(dmDicomInstance => dmDicomInstance.SopInstanceUid, opt => opt.Ignore())
                        .ForCtorParam("sopInstanceUid",
                            opt => opt.MapFrom(dmDicomInstance =>
                                new DomainModel.DicomUid(dmDicomInstance.SopInstanceUid)));

                    cfg.CreateMap<DomainModel.DicomAttribute, EFModel.DicomAttribute>()
                        // set up collection identity
                        .EqualityComparison((dmDicomAttribute, efDicomAttribute) =>
                            efDicomAttribute.DicomTag == dmDicomAttribute.DicomTag.ToString())
                        /// ignore primary key and foreign keys
                        .ForMember(efDicomAttribute => efDicomAttribute.ID, opt => opt.Ignore())
                        .ForMember(efDicomAttribute => efDicomAttribute.DicomInstanceId, opt => opt.Ignore())
                        .ForMember(efDicomAttribute => efDicomAttribute.DicomInstance, opt => opt.Ignore())
                        // set up conversion for value objects
                        .ForMember(efDicomAttribute => efDicomAttribute.DicomTag,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomTag>()));

                    cfg.CreateMap<EFModel.DicomAttribute, DomainModel.DicomAttribute>()
                        // set up conversion for value objects
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