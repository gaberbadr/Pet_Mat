using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLayer.Enums
{
 // ==================== APPLICATION STATUS ====================

        public enum ApplicationStatus
        {
            Pending,
            Approved,
            Rejected
        }

        // ==================== LISTING STATUS ====================

        public enum ListingStatus
        {
            Active,
            Sold,
            Reserved
        }

        // ==================== ORDER STATUS ====================

        public enum OrderStatus
        {
            Pending,
            Processing,
            Shipped,
            Delivered,
            Cancelled,
            Refunded
        }


        // ==================== ANIMAL GENDER ====================

        public enum Gender
        {
            Male,
            Female,
            Unknown
        }

        // ==================== ANIMAL SIZE ====================

        public enum AnimalSize
        {
            Small,
            Medium,
            Large,
            ExtraLarge
        }

        // ==================== ANIMAL LISTING TYPE ====================

        public enum AnimalListingType
        {
            Sale,
            Adoption,
            Breeding,
            Rehoming
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

        // ==================== MESSAGE TYPE ====================

        public enum MessageType
        {
            Text,
            Image,
            Video,
            Document,
            Location
        }

        // ==================== MESSAGE CONTEXT TYPE ====================

        public enum MessageContextType
        {
            General,
            AnimalListing,
            AccessoryListing,
            PharmacyStore,
            DoctorConsultation

        }

        // ==================== POST REACTION TYPE ====================

        public enum ReactionType
        {
            Like,
            Love,
            Care,
            Funny,
            Wow,
            Sad,
            Angry
        }

        // ==================== PHARMACY LISTING CATEGORY ====================

        public enum PharmacyListingCategory
        {
            Medication,
            Supplements,
            Vaccines,
            Antiparasitics,
            FirstAid,
            Nutrition,
            Hygiene,
            Other
        }
}

//change data type of entity
//add configuration for new enum for the entity
//dto should be have same enum data type as input and output(maybe let as stringe then do dto.status.tostring() later when use)
//update mapping profile if any :.ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
//if there filtering based on enum add parsing it in specification

