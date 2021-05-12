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
        static void Main(string[] args)
        {
            // 1. try just creating an EF model
            // TestData.CreatePopulateEFModel();

            // 2.
            DomainModel.DicomUid newSeriesUid = null;
            {
                var context = new EFModel.MyContext();
                var repository = new DicomSeriesRepository(context);

                var newSeriesDomainModel = TestData.CreateSeries();

                repository.UpdateAsync(newSeriesDomainModel).Wait();

                // capture new series ID
                newSeriesUid = newSeriesDomainModel.SeriesInstanceUid;
            }

            // 3.
            {
                var context = new EFModel.MyContext();
                var repository = new DicomSeriesRepository(context);

                var updateSeriesDomainModel = repository.GetAggregateForKey(newSeriesUid);

                TestData.AddInstancesToSeries(updateSeriesDomainModel);

                repository.UpdateAsync(updateSeriesDomainModel).Wait();
            }
        }
    }
}
