using System.Threading.Tasks;
using DomainModel = Elekta.Capability.Dicom.Domain.Model;

namespace Elekta.Capability.Dicom.Application.Messaging
{
    public interface IMessaging
    {
        Task SendNewSeriesEvent(DomainModel.DicomSeries series);
    }
}
