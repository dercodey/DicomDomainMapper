using System.Threading.Tasks;
using DomainModel = Elekta.Capability.Dicom.Domain.Model;

namespace Elekta.Capability.Dicom.Application.Messaging
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMessaging
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="series"></param>
        /// <returns></returns>
        Task SendNewSeriesEvent(DomainModel.DicomSeries series);
    }
}
