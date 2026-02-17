namespace Facade.SubSistemas.Interfaces
{
    public interface IShippingService
    {
        decimal CalculateShipping(string zipCode, decimal weight);
        string CreateShippingLabel(string orderId, string address);
        void SchedulePickup(string labelId, DateTime date);
    }
}
