using System;
using AutoMapper;
using AutoMapper.EquivalencyExpression;
using DomainModel = Dicom.Domain.Model;
using AbstractionModel = Elekta.Capability.Dicom.Abstractions.Models;

namespace Dicom.Api.Mappers
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
