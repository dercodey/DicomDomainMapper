using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using AutoMapper.EquivalencyExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TestDicomDomainMapper
{

    class Program
    {
        static IMapper CreateMapper(Action<IMapperConfigurationExpression> cfg)
        {
            var map = new MapperConfiguration(cfg);
            map.CompileMappings();

            var mapper = map.CreateMapper();
            mapper.ConfigurationProvider.AssertConfigurationIsValid();
            return mapper;
        }

        static void CreatePopulateEFModel()
        {
            var context = new EFModel.MyContext();
            var series = new EFModel.DicomSeries()
            {
                SeriesInstanceUid = "1.2.5.4",
                PatientID = "12345",
                Modality = "CT",
                AcquisitionDateTime = DateTime.Now
            };
            context.DicomSeries.Add(series);

            context.DicomInstances.Add(
                new EFModel.DicomInstance()
                {
                    DicomSeries = series,
                    SopInstanceUid = "1.2.5.5",
                });

            context.DicomInstances.Add(
                new EFModel.DicomInstance()
                {
                    DicomSeries = series,
                    SopInstanceUid = "1.2.5.6",
                });

            context.DicomInstances.Add(
                new EFModel.DicomInstance()
                {
                    DicomSeries = series,
                    SopInstanceUid = "1.2.5.7",
                });

            context.SaveChanges();
        }

        /// <summary>
        /// creates a new series aggregate
        /// </summary>
        /// <returns></returns>
        static DomainModel.DicomSeries CreateSeries()
        {
            var series =
                DomainModel.DicomSeries.Create(
                    "1.2.3.7",
                    "98765",
                    "CT",
                    new DateTime(2021, 01, 02),
                    new List<DomainModel.DicomInstance>());

            return series;
        }

        /// <summary>
        /// creates a few new instances and adds to the series
        /// </summary>
        /// <param name="series"></param>
        static void AddInstancesToSeries(DomainModel.DicomSeries series)
        {
            for (int n = 0; n < 3; n++)
            {
                var attributes = new List<DomainModel.DicomAttribute>()
                {
                    DomainModel.DicomAttribute.Create("PATIENTID", "98765"),
                    DomainModel.DicomAttribute.Create("ACQUISITIONDATETIME", (new DateTime(2021,01,02)).ToString()),
                    DomainModel.DicomAttribute.Create("MODALITY", "CT"),
                };

                var instance =
                    DomainModel.DicomInstance.Create(
                        $"1.2.3.{n + 7}",
                        attributes);

                series.AddInstance(instance);
            }
        }

        static void Main(string[] args)
        {
            var mapper = CreateMapper(cfg => {
                // cfg.AddCollectionMappers();
                cfg.CreateMap<EFModel.DicomAttribute, DomainModel.DicomAttribute>().ReverseMap();
                cfg.CreateMap<EFModel.DicomInstance, DomainModel.DicomInstance>().ReverseMap();
                cfg.CreateMap<EFModel.DicomSeries, DomainModel.DicomSeries>().ReverseMap();
                // cfg.UseEntityFrameworkCoreModel<EFModel.MyContext>();
            });

            // 1. try just creating an EF model
            // CreatePopulateEFModel();

            // 2.
            string newSeriesUid = null;
            {
                var context = new EFModel.MyContext();

                var newSeriesDomainModel = CreateSeries();
                var newSeriesEFModel = mapper.Map<EFModel.DicomSeries>(newSeriesDomainModel);
                context.DicomSeries.Add(newSeriesEFModel);

                context.SaveChanges();

                // capture new series ID
                newSeriesUid = newSeriesDomainModel.SeriesInstanceUid;
            }

            // 3.
            {
                var context = new EFModel.MyContext();

                var existingSeriesEFModel =
                    context.DicomSeries.First(series =>
                        series.SeriesInstanceUid.CompareTo(newSeriesUid.ToString()) == 0);
                var updateSeriesDomainModel = 
                    mapper.Map<DomainModel.DicomSeries>(existingSeriesEFModel);

                AddInstancesToSeries(updateSeriesDomainModel);

                var updatedSeriesEFModel = 
                    mapper.Map<EFModel.DicomSeries>(updateSeriesDomainModel);
                context.DicomSeries.Remove(existingSeriesEFModel);
                context.DicomSeries.Add(updatedSeriesEFModel);
                context.SaveChanges();
            }
        }
    }
}
