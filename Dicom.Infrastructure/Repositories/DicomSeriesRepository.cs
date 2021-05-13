using AutoMapper.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainModel = Dicom.Domain.Model;

namespace Dicom.Infrastructure.Repositories
{
    /// <summary>
    /// an aggregate repository for DICOM series aggregates
    /// </summary>
    public class DicomSeriesRepository : IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>, IDisposable
    {
        private readonly EFModel.MyContext _context;

        /// <summary>
        /// construct a new repository with the given DB context
        /// </summary>
        /// <param name="context">the DB context to use for query/update</param>
        public DicomSeriesRepository(EFModel.MyContext context)
        {
            this._context = context;
        }

        /// <summary>
        /// gets the DICOM series for the given series instance UID
        /// </summary>
        /// <param name="forKey">series instance UID</param>
        /// <returns>the matching DicomSeries</returns>
        public DomainModel.DicomSeries GetAggregateForKey(DomainModel.DicomUid forKey)
        {
            // get the mapper to help
            var mapper = Mappers.MyMapper.GetMapper();

            // get the matching series
            var matchSeries = 
                _context.DicomSeries.Where(series =>
                    series.SeriesInstanceUid.CompareTo(forKey.ToString()) == 0)
                .SingleOrDefault();

            // did an entity get found?
            if (matchSeries == null)
            {
                throw new KeyNotFoundException();
            }

            // ensure other entities are selected\
            // TODO: is there a more efficient way to do this?
            _context.DicomInstances.ToList();
            _context.DicomAttributes.ToList();

            // map to the domain model
            var seriesDomainModel = mapper.Map<DomainModel.DicomSeries>(matchSeries);

            // and return the result
            return seriesDomainModel;
        }

        /// <summary>
        /// updates the aggregate, or creates if it is new
        /// </summary>
        /// <param name="updatedSeries">DicomSeries to be updated</param>
        /// <returns>task representing the work</returns>
        public async Task UpdateAsync(DomainModel.DicomSeries updatedSeries)
        {
            var mapper = Mappers.MyMapper.GetMapper();

#if USE_AUTOMAPPER
            _context.DicomSeries.Persist(mapper).InsertOrUpdate(updatedSeries);
#else
            // trigger load of all entities
            var matchSeries =
                _context.DicomSeries.Where(series =>
                    series.SeriesInstanceUid.CompareTo(updatedSeries.RootKey.ToString()) == 0)
                .SingleOrDefault();

            // did we find no match?
            if (matchSeries == null)
            {
                // so we are adding new series -- just map directly
                matchSeries = mapper.Map<EFModel.DicomSeries>(updatedSeries);

                // add to the context
                _context.DicomSeries.Add(matchSeries);

                // set the entity state
                _context.Entry(matchSeries).State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }
            else
            {
                // updating existing series
                mapper.Map(updatedSeries, matchSeries);

                // set the entity state
                _context.Entry(matchSeries).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }
#endif
            // now perform the save
            await _context.SaveChangesAsync();
        }

#region IDisposable

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _context.Dispose();
        }

#endregion

    }
}
