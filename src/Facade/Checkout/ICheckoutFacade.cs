using Facade.Dto;

namespace Facade.Checkout
{
    public interface ICheckoutFacade
    {
        void Process(OrderDTO order);
    }
}
