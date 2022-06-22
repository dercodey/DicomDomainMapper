using System.Threading.Tasks;
using DomainModel = Elektrum.Capability.Dicom.Domain.Model;

namespace Elektrum.Capability.Dicom.Application.Messaging
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
