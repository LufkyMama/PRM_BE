namespace PRM_BE.Model.Enums
{
    public enum UserRole { Admin = 1, Staff = 2, Customer = 3 }

    public enum OrderStatus
    {
        Pending = 1, Confirmed, Preparing, OutForDelivery, Delivered, Cancelled, Refunded
    }

    public enum PaymentStatus
    {
        Unpaid = 0, Authorized = 1, Paid = 2, Failed = 3, Refunded = 4, PartiallyRefunded = 5
    }

    public enum PaymentMethod
    {
        Unknown = 0, CashOnDelivery = 1 
    }

    public enum DeliveryStatus
    {
        NotStarted = 0, Assigned = 1, PickedUp = 2, InTransit = 3, Delivered = 4, Failed = 5, Returned = 6
    }
    //staff
    public enum DeliveryTimeWindow
    {
        Anytime = 0, Morning = 1, Afternoon = 2, Evening = 3
    }

    public enum FlowerCategory
    {
        Roses = 1, Tulips = 2, Daisies = 3, Lilies = 4, Orchids = 5, Sunflowers = 6, Carnations = 7, MixedBouquets = 8
    }
}
