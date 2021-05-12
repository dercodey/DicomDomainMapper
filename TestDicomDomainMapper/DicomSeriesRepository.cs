using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestDicomDomainMapper
{
    class DicomSeriesRepository : IAggregateRepository<DomainModel.DicomSeries, DomainModel.DicomUid>
    {
        private readonly EFModel.MyContext _context;

        public DicomSeriesRepository(EFModel.MyContext context)
        {
            this._context = context;
        }

        public DomainModel.DicomSeries GetAggregateForKey(DomainModel.DicomUid forKey)
        {
            var mapper = MappingExtensions.GetMapper();

            // trigger load of all entities
            var matchSeries = 
                _context.DicomSeries.Where(series =>
                    series.SeriesInstanceUid.CompareTo(forKey.ToString()) == 0)
                .SingleOrDefault();
            if (matchSeries == null)
            {
                throw new KeyNotFoundException();
            }

            var seriesDomainModel = mapper.Map<DomainModel.DicomSeries>(matchSeries);
            return seriesDomainModel;
        }

        public async Task UpdateAsync(DomainModel.DicomSeries updatedSeries)
        {
            var mapper = MappingExtensions.GetMapper();

            // trigger load of all entities
            var matchSeries =
                _context.DicomSeries.Where(series =>
                    series.SeriesInstanceUid.CompareTo(updatedSeries.RootId.ToString()) == 0)
                .SingleOrDefault();
            if (matchSeries == null)
            {
                matchSeries = mapper.Map<EFModel.DicomSeries>(updatedSeries);
                _context.Entry(matchSeries).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                _context.DicomSeries.Add(matchSeries);
            }
            else
            {
                _context.Entry(matchSeries).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            // need to ensure sub entities are in context?

            await _context.SaveChangesAsync();
        }
    }
}
