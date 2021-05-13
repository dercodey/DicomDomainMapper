using AutoMapper;
using AutoMapper.EquivalencyExpression;
using System;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Infrastructure.EFModel
{
    /// <summary>
    /// wrapper for Automapper configured to map between domain model and EF model
    /// </summary>
    static class MyMapper
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
                    cfg.AllowNullCollections = true;
                    cfg.AddCollectionMappers();
                    cfg.CreateMap<DomainModel.DicomAttribute, EFModel.DicomAttribute>()
                        .ForMember(s => s.DicomTag,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomTag>()));
                    cfg.CreateMap<EFModel.DicomAttribute, DomainModel.DicomAttribute>()
                        .ForCtorParam("dicomTag",
                            opt => opt.MapFrom(src =>
                                // TODO: need to look up the actual tag to get a proper value representation
                                new DomainModel.DicomTag(src.DicomTag, typeof(string), false)));

                    cfg.CreateMap<DomainModel.DicomInstance, EFModel.DicomInstance>()
                        .ForMember(s => s.SopInstanceUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));
                    cfg.CreateMap<EFModel.DicomInstance, DomainModel.DicomInstance>()
                        .ForCtorParam("sopInstanceUid",
                            opt => opt.MapFrom(src =>
                                new DomainModel.DicomUid(src.SopInstanceUid)));

                    cfg.CreateMap<DomainModel.DicomSeries, EFModel.DicomSeries>()
                        .ForMember(s => s.SeriesInstanceUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));
                    cfg.CreateMap<EFModel.DicomSeries, DomainModel.DicomSeries>()
                        .ForCtorParam("seriesInstanceUid",
                            opt => opt.MapFrom(src =>
                                new DomainModel.DicomUid(src.SeriesInstanceUid)));
                    // cfg.UseEntityFrameworkCoreModel<EFModel.MyContext>();
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