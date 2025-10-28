using PRM_BE.Model.Momo;
using PRM_BE.Model;

namespace PRM_BE.Service.Momo
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model);
        Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection);

    }
}