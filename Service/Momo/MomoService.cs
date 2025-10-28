using Microsoft.Extensions.Options;
using System.Text;
using System.Security.Cryptography;
using PRM_BE.Model.Momo;
using PRM_BE.Model;
using RestSharp;
using Newtonsoft.Json;
using PRM_BE.Data;
using Microsoft.EntityFrameworkCore;
using PRM_BE.Model.Enums;
using PRM_BE.Data.Repository;
using PRM_BE.Service.Models; // Add this using directive

namespace PRM_BE.Service.Momo
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;
        private readonly AppDbContext _context;
        private readonly PaymentService _paymentService; // Injected PaymentService

        public MomoService(IOptions<MomoOptionModel> options, AppDbContext context, PaymentService paymentService) // Updated constructor
        {
            _options = options;
            _context = context;
            _paymentService = paymentService;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentMomo(OrderInfoModel model)
        {
            string internalOrderId = model.OrderId;
            string momoTransactionId = $"{internalOrderId}_{Guid.NewGuid()}";

            var orderInfo = "Thanh toan don hang #" + internalOrderId;

            var amount = (long)model.Amount;
            var rawData =
                $"partnerCode={_options.Value.PartnerCode}" +
                $"&accessKey={_options.Value.AccessKey}" +
                $"&requestId={momoTransactionId}" +
                $"&amount={amount}" +
                $"&orderId={momoTransactionId}" +
                $"&orderInfo={orderInfo}" +
                $"&returnUrl={_options.Value.ReturnUrl}" +
                $"&notifyUrl={_options.Value.NotifyUrl}" +
                $"&extraData={internalOrderId}";

            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);
            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            var requestData = new
            {
                accessKey = _options.Value.AccessKey,
                partnerCode = _options.Value.PartnerCode,
                requestType = _options.Value.RequestType,
                notifyUrl = _options.Value.NotifyUrl,
                returnUrl = _options.Value.ReturnUrl,
                orderId = momoTransactionId,
                amount = amount.ToString(),
                orderInfo = orderInfo,
                requestId = momoTransactionId,
                extraData = internalOrderId,
                signature = signature
            };

            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);
            var response = await client.ExecuteAsync(request);

            return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
        }

        public async Task<MomoExecuteResponseModel> PaymentExecuteAsync(IQueryCollection collection)
        {
            var lookup = collection.ToDictionary(k => k.Key, v => v.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            // Corrected rawData string for signature validation based on Momo's expected format
            // Order of parameters is crucial for Momo signature validation
            var rawData = $"partnerCode={lookup["partnerCode"]}&accessKey={lookup["accessKey"]}&requestId={lookup["requestId"]}&amount={lookup["amount"]}&orderId={lookup["orderId"]}&orderInfo={lookup["orderInfo"]}&orderType={lookup["orderType"]}&transId={lookup["transId"]}&message={lookup["message"]}&localMessage={lookup["localMessage"]}&responseTime={lookup["responseTime"]}&errorCode={lookup["errorCode"]}&payType={lookup["payType"]}&extraData={lookup["extraData"]}";
            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            if (signature != lookup["signature"])
            {
                return new MomoExecuteResponseModel() { Message = "Invalid signature!" };
            }

            var response = new MomoExecuteResponseModel();
            response.OrderId = lookup.TryGetValue("orderId", out var orderId) ? orderId : string.Empty;
            response.Amount = lookup.TryGetValue("amount", out var amount) ? amount : string.Empty;
            response.OrderInfo = lookup.TryGetValue("orderInfo", out var orderInfo) ? orderInfo : string.Empty;

                        // If payment is successful, update database

                        if (int.TryParse(lookup["extraData"], out int orderIdFromExtraData))

                        {

                            var payment = await _context.Payments.FirstOrDefaultAsync(p => p.OrderId == orderIdFromExtraData);

                            if (payment != null)

                            {

                                if (lookup["errorCode"] == "0")

                                {

                                    response.Message = "Payment successful!";

                                    var updateDto = new PaymentStatusUpdateDto { Status = PaymentStatus.Paid };

                                    await _paymentService.UpdateStatusAsync(payment.Id, updateDto);

                                }

                                else

                                {

                                    response.Message = lookup["message"];

                                    var updateDto = new PaymentStatusUpdateDto { Status = PaymentStatus.Failed, FailureReason = lookup["message"] };

                                    await _paymentService.UpdateStatusAsync(payment.Id, updateDto);

                                }

                            }

                            else

                            {

                                response.Message = "Could not find associated payment record.";

                            }

                        }

                        else

                        {

                            response.Message = "Invalid extraData.";

                        }
            return response;
        }

        private String ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] hashBytes;
            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }
            var hashString = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return hashString;
        }
    }
}