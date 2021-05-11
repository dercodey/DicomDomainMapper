using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestDicomDomainMapper
{
    static class MappingExtensions
    {
        static IMapper CreateMapper(Action<IMapperConfigurationExpression> cfg)
        {
            var map = new MapperConfiguration(cfg);
            map.CompileMappings();

            var mapper = map.CreateMapper();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            return mapper;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IMapper GetMapper()
        {
            return mapper;
        }

        static IMapper mapper = CreateMapper(cfg => {
            // cfg.AddCollectionMappers();
            cfg.CreateMap<EFModel.DicomAttribute, DomainModel.DicomAttribute>().ReverseMap();
            cfg.CreateMap<EFModel.DicomInstance, DomainModel.DicomInstance>().ReverseMap();
            cfg.CreateMap<EFModel.DicomSeries, DomainModel.DicomSeries>().ReverseMap();
            // cfg.UseEntityFrameworkCoreModel<EFModel.MyContext>();
        });

        public static EFModel.DicomAttribute ToEFModel(this DomainModel.DicomAttribute fromAttribute,
            EFModel.MyContext context, EFModel.DicomInstance forInstance)
        {
            var convertedAttribute = GetMapper().Map<EFModel.DicomAttribute>(fromAttribute);
            context.DicomAttributes.Add(convertedAttribute);

            return convertedAttribute;
        }

        public static EFModel.DicomInstance ToEFModel(this DomainModel.DicomInstance fromInstance, 
            EFModel.MyContext context, EFModel.DicomSeries forSeries)
        {
            var matchInstance =
                    context.DicomInstances.Where(instance =>
                        instance.SopInstanceUid.CompareTo(fromInstance.SopInstanceUid.ToString()) == 0)
                        .SingleOrDefault();
            if (matchInstance != null)
            {
                // can't modify an existing DicomInstance
                throw new NotSupportedException();
            }

            var newInstance = GetMapper().Map<EFModel.DicomInstance>(fromInstance);
            newInstance.DicomSeries = forSeries;
            forSeries.DicomInstances.Add(newInstance);
            context.DicomInstances.Add(newInstance);

            var convertedAttributes =
                fromInstance.DicomAttributes.Select(fromAttribute =>
                    fromAttribute.ToEFModel(context, newInstance)).ToList();
            newInstance.DicomAttributes = convertedAttributes;

            return newInstance;
        }

        /// <summary>
        /// maps from the domail model to the EF model
        /// </summary>
        /// <param name="context">db context to be used for mapping</param>
        /// <param name="fromSeries">series to map</param>
        public static EFModel.DicomSeries ToEFModel(this DomainModel.DicomSeries fromSeries, EFModel.MyContext context)
        {
            // see if series is already present
            var matchSeries =
                context.DicomSeries.Where(series =>
                    series.SeriesInstanceUid.CompareTo(fromSeries.SeriesInstanceUid.ToString()) == 0)
                    .SingleOrDefault();
            if (matchSeries == null)
            {
                matchSeries = GetMapper().Map<EFModel.DicomSeries>(fromSeries);
                context.DicomSeries.Add(matchSeries);
            }
            else
            {
                // otherwise we are updating
            }

            // see if instances need mapping
            var convertedInstances = 
                fromSeries.DicomInstances.Select(fromInstance =>
                    fromInstance.ToEFModel(context, matchSeries)).ToList();

            return matchSeries;
        }

        public static DomainModel.DicomAttribute ToDomainModel(this EFModel.DicomAttribute fromAttribute)
        {
            return GetMapper().Map<DomainModel.DicomAttribute>(fromAttribute);
        }

        public static DomainModel.DicomInstance ToDomainModel(this EFModel.DicomInstance fromInstance)
        {
            return new DomainModel.DicomInstance(
                fromInstance.SopInstanceUid,
                fromInstance.DicomAttributes?.Select(fromAttribute => 
                    fromAttribute.ToDomainModel()));
        }

        public static DomainModel.DicomSeries ToDomainModel(this EFModel.DicomSeries fromSeries)
        {
            return new DomainModel.DicomSeries(
                fromSeries.SeriesInstanceUid,
                fromSeries.PatientID,
                fromSeries.Modality,
                fromSeries.AcquisitionDateTime,
                fromSeries.DicomInstances?.Select(fromInstance => 
                    fromInstance.ToDomainModel()));
        }
    }
}
