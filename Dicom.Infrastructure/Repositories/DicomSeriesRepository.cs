using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DomainModel = Elektrum.Capability.Dicom.Domain.Model;
using Elektrum.Capability.Dicom.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Elektrum.Capability.Dicom.Infrastructure.Repositories
{
    /// <summary>
    /// an aggregate repository for DICOM series aggregates
    /// </summary>
    public class DicomSeriesRepository : IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>, IDisposable
    {
        private readonly EFModel.DicomDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<DicomSeriesRepository> _logger;

        /// <summary>
        /// construct a new repository with the given DB context
        /// </summary>
        /// <param name="context">the DB context to use for query/update</param>
        public DicomSeriesRepository(EFModel.DicomDbContext context, IMapper mapper, ILogger<DicomSeriesRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        #region IAggregateRepository

        /// <summary>
        /// gets the DICOM series for the given series instance UID
        /// </summary>
        /// <param name="forKey">series instance UID</param>
        /// <returns>the matching DicomSeries</returns>
        public DomainModel.DicomSeries GetAggregateForKey(DomainModel.DicomUid forKey)
        {
            // get the matching series
            var matchSeries = 
                _context.DicomSeries
                    .Where(series =>
                        series.SeriesInstanceUid == forKey.ToString())
                    .Include(series => series.DicomInstances)
                    .ThenInclude(instance => instance.DicomAttributes)
                    .SingleOrDefault();

            // did an entity get found?
            if (matchSeries == null)
            {
                return null;
            }

            // map to the domain model
            var seriesDomainModel = _mapper.Map<DomainModel.DicomSeries>(matchSeries);

            // and return the result
            return seriesDomainModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selectFunc"></param>
        /// <returns></returns>
        public IEnumerable<DomainModel.DicomSeries> SelectAggregates(Func<DomainModel.DicomSeries, bool> selectFunc)
        {
            // TODO: how to make this more efficient?
            var allDmDicomSeries = _context.DicomSeries.Select(_mapper.Map<DomainModel.DicomSeries>);
            var matchingSeries = allDmDicomSeries.Where(selectFunc);
            return matchingSeries;
        }

        /// <summary>
        /// updates the aggregate, or creates if it is new
        /// </summary>
        /// <param name="updatedSeries">DicomSeries to be updated</param>
        /// <returns>task representing the work</returns>
        public async Task UpdateAsync(DomainModel.DicomSeries updatedSeries)
        {
            _context.DicomSeries.Persist(_mapper).InsertOrUpdate(updatedSeries);

            // now perform the save
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="forKey"></param>
        /// <returns></returns>
        public async Task RemoveAsync(DomainModel.DicomUid forKey)
        {
            var matchSeries = GetAggregateForKey(forKey);

            _context.DicomSeries.Persist(_mapper).Remove(matchSeries);

            await _context.SaveChangesAsync();
        }

        #endregion

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
