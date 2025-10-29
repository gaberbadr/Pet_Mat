using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Enums
{
    // ==================== LISTING STATUS ====================

    public enum ListingStatus
    {
        Active,
        Sold,
        Reserved
    }

    // ==================== ACCESSORY CONDITION ====================

    public enum AccessoryCondition
    {
        New,
        LikeNew,
        Good,
        Used
    }
    // ==================== ACCESSORY CATEGORY ====================

    public enum AccessoryCategory
    {
        Toys,
        Clothing,
        Bedding,
        Bowls,
        Leashes,
        Collars,
        Carriers,
        Grooming,
        Training,
        Other
    }
}
