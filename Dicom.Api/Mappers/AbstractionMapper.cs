using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Api.Mappers
{
    public static class AbstractionMapper
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

                    cfg.CreateMap<DomainModel.DicomSeries, Abstractions.DicomSeries>()
                        .ForMember(s => s.SeriesUid,
                            opt => opt.ConvertUsing(new ToStringFormatter<DomainModel.DicomUid>()));

                    cfg.CreateMap<Abstractions.DicomSeries, DomainModel.DicomSeries>()
                        .ForCtorParam("seriesInstanceUid",
                            opt => opt.MapFrom(src =>
                                new DomainModel.DicomUid(src.SeriesUid))); 
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
