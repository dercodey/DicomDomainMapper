using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TestDicomDomainMapper.Repositories
{
    /// <summary>
    /// 
    /// </summary>
    class DicomSeriesRepository : IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>
    {
        private readonly EFModel.MyContext _context;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        public DicomSeriesRepository(EFModel.MyContext context)
        {
            this._context = context;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="forKey"></param>
        /// <returns></returns>
        public DomainModel.DicomSeries GetAggregateForKey(DomainModel.DicomUid forKey)
        {
            var mapper = EFModel.MyMapper.GetMapper();

            // get the matching series
            var matchSeries = 
                _context.DicomSeries.Where(series =>
                    series.SeriesInstanceUid.CompareTo(forKey.ToString()) == 0)
                .SingleOrDefault();

            if (matchSeries == null)
            {
                throw new KeyNotFoundException();
            }

            // ensure other entities are selected -- is there a more efficient way to do this?
            _context.DicomInstances.ToList();
            _context.DicomAttributes.ToList();

            var seriesDomainModel = mapper.Map<DomainModel.DicomSeries>(matchSeries);
            return seriesDomainModel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="updatedSeries"></param>
        /// <returns></returns>
        public async Task UpdateAsync(DomainModel.DicomSeries updatedSeries)
        {
            var mapper = EFModel.MyMapper.GetMapper();

            // trigger load of all entities
            var matchSeries =
                _context.DicomSeries.Where(series =>
                    series.SeriesInstanceUid.CompareTo(updatedSeries.RootKey.ToString()) == 0)
                .SingleOrDefault();
            if (matchSeries == null)
            {
                // adding new series
                matchSeries = mapper.Map<EFModel.DicomSeries>(updatedSeries);
                _context.Entry(matchSeries).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                _context.DicomSeries.Add(matchSeries);
            }
            else
            {
                // updating existing series
                mapper.Map(updatedSeries, matchSeries);
                _context.Entry(matchSeries).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            // need to ensure sub entities are in context?

            await _context.SaveChangesAsync();
        }
    }
}
