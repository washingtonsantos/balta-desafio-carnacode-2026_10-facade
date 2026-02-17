using Facade.Dto;
using Facade.SubSistemas.Interfaces;

namespace Facade.Checkout
{
    public class CheckoutFacade : ICheckoutFacade
    {
        private readonly IInventorySystem _inventory;
        private readonly IPaymentGateway _payment;
        private readonly IShippingService _shipping;
        private readonly ICouponSystem _coupon;
        private readonly INotificationService _notification;

        public CheckoutFacade(
            IInventorySystem inventorySystem,
            IPaymentGateway paymentGateway,
            IShippingService shippingService,
            ICouponSystem couponSystem,
            INotificationService notificationService)
        {
            _inventory = inventorySystem;
            _payment = paymentGateway;
            _shipping = shippingService;
            _coupon = couponSystem;
            _notification = notificationService;
        }

        public void Process(OrderDTO order)
        {
            Console.WriteLine("=== Processando Pedido ===\n");

            try
            {
                // Passo 1: Verificar estoque
                if (!_inventory.CheckAvailability(order.ProductId))
                {
                    Console.WriteLine("❌ Produto indisponível");
                    return;
                }

                // Passo 2: Reservar produto
                _inventory.ReserveProduct(order.ProductId, order.Quantity);

                // Passo 3: Validar e aplicar cupom
                decimal discount = 0;
                if (!string.IsNullOrEmpty(order.CouponCode))
                {
                    if (_coupon.ValidateCoupon(order.CouponCode))
                    {
                        discount = _coupon.GetDiscount(order.CouponCode);
                    }
                }

                // Passo 4: Calcular valores
                decimal subtotal = order.ProductPrice * order.Quantity;
                decimal discountAmount = subtotal * discount;
                decimal _shippingCost = _shipping.CalculateShipping(order.ZipCode, order.Quantity * 0.5m);
                decimal total = subtotal - discountAmount + _shippingCost;

                // Passo 5: Processar pagamento
                string transactionId = _payment.InitializeTransaction(total);

                if (!_payment.ValidateCard(order.CreditCard, order.Cvv))
                {
                    _inventory.ReleaseReservation(order.ProductId, order.Quantity);
                    Console.WriteLine("❌ Cartão inválido");
                    return;
                }

                if (!_payment.ProcessPayment(transactionId, order.CreditCard))
                {
                    _inventory.ReleaseReservation(order.ProductId, order.Quantity);
                    Console.WriteLine("❌ Pagamento recusado");
                    return;
                }

                // Passo 6: Criar envio
                string orderId = $"ORD{DateTime.Now.Ticks}";
                string labelId = _shipping.CreateShippingLabel(orderId, order.ShippingAddress);
                _shipping.SchedulePickup(labelId, DateTime.Now.AddDays(1));

                // Passo 7: Marcar cupom como usado
                if (!string.IsNullOrEmpty(order.CouponCode))
                {
                    _coupon.MarkCouponAsUsed(order.CouponCode, order.CustomerEmail);
                }

                // Passo 8: Enviar notificações
                _notification.SendOrderConfirmation(order.CustomerEmail, orderId);
                _notification.SendPaymentReceipt(order.CustomerEmail, transactionId);
                _notification.SendShippingNotification(order.CustomerEmail, labelId);

                Console.WriteLine($"\n✅ Pedido {orderId} finalizado com sucesso!");
                Console.WriteLine($"   Total: R$ {total:N2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao processar pedido: {ex.Message}");
            }
        }
    }
}
