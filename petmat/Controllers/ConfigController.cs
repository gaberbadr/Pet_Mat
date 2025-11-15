using CoreLayer.Dtos;
using CoreLayer.Enums;
using CoreLayer.Helper.EnumsHelperMethod;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace petmat.Controllers
{

    public class ConfigController : BaseApiController
    {
        [HttpGet]
        public IActionResult GetAllEnums()
        {
            var result = new Dictionary<string, List<EnumResponseDto>>
            {
                { "applicationStatus", EnumHelper.GetEnumValues<ApplicationStatus>() },
                { "listingStatus", EnumHelper.GetEnumValues<ListingStatus>() },
                { "orderStatus", EnumHelper.GetEnumValues<OrderStatus>() },
                { "gender", EnumHelper.GetEnumValues<Gender>() },
                { "animalSize", EnumHelper.GetEnumValues<AnimalSize>() },
                { "animalListingType", EnumHelper.GetEnumValues<AnimalListingType>() },
                { "accessoryCondition", EnumHelper.GetEnumValues<AccessoryCondition>() },
                { "accessoryCategory", EnumHelper.GetEnumValues<AccessoryCategory>() },
                { "messageType", EnumHelper.GetEnumValues<MessageType>() },
                { "messageContextType", EnumHelper.GetEnumValues<MessageContextType>() },
                { "reactionType", EnumHelper.GetEnumValues<ReactionType>() },
                { "pharmacyListingCategory", EnumHelper.GetEnumValues<PharmacyListingCategory>()},
                { "paymentMethod", EnumHelper.GetEnumValues<PaymentMethod>()}
            };

            return Ok(result);
        }
    }
}
