using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests
{
    /// <summary>
    /// GRAY BOX TESTLER - Kısmi iç bilgi: Hangi metodun çağrıldığını ve
    /// veri yapısını biliyoruz, ama implementasyonun tamamını değil.
    /// </summary>
    [TestFixture]
    public class GrayBoxTests
    {
        // ──────────────────────────────────────────────────────────
        // TC-GB-01: Sepet temizlendikten sonra sipariş oluşmamalı
        // Beklenen: Exception | Sonuç: PASS ✓ (Clear doğru çalışıyor)
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("GrayBox")]
        [Description("TC-GB-01: CreateOrder sonrası sepet temizlenmeli, tekrar sipariş verilememeli")]
        public void CreateOrder_CartShouldBeClearedAfterOrder()
        {
            // Arrange - Cart.Clear() çağrılacağını biliyoruz
            var cart = new Cart("gb-musteri-01");
            cart.AddItem(new Product(1, "Diz Üstü", 8000m, 10), 1);
            var service = new OrderService();

            // Act
            service.CreateOrder(cart, PaymentMethod.CreditCard);

            // Assert - Cart.IsEmpty özelliğini kontrol ediyoruz
            Assert.That(cart.IsEmpty, Is.True,
                "Sipariş sonrası sepet boşaltılmalı!");

            // İkinci sipariş girişimi exception atmalı
            Assert.Throws<InvalidOperationException>(() =>
                service.CreateOrder(cart, PaymentMethod.CreditCard));
        }

        // ──────────────────────────────────────────────────────────
        // TC-GB-02: Kargodaki sipariş iptal edilememeli
        // Beklenen: Exception | Sonuç: PASS ✓
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("GrayBox")]
        [Description("TC-GB-02: Kargoya verilmiş sipariş iptal edilemez")]
        public void CancelOrder_ShippedOrder_ShouldThrowException()
        {
            // Arrange - UpdateStatus metodunu biliyoruz
            var cart = new Cart("gb-musteri-02");
            cart.AddItem(new Product(2, "Kamera", 6000m, 5), 1);
            var service = new OrderService();
            var order = service.CreateOrder(cart, PaymentMethod.BankTransfer);
            service.UpdateStatus(order.Id, OrderStatus.Shipped); // Kargoya ver

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.CancelOrder(order.Id),
                "Kargodaki sipariş iptal edilemez!");
        }

        // ──────────────────────────────────────────────────────────
        // TC-GB-03: GetTotal, negatif kupon ile mantıklı davranmalı
        // Beklenen: Exception | Gerçek: Hesaplama yapılıyor → FAIL ✗
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("GrayBox")]
        [Description("TC-GB-03: Negatif kupon indirimi reddedilmeli")]
        public void Cart_NegativeCoupon_ShouldThrowException()
        {
            // Arrange - GetTotal metodunun couponDiscount parametresini biliyoruz
            var cart = new Cart("gb-musteri-03");
            cart.AddItem(new Product(3, "Saat", 1500m, 8), 1);

            // Act & Assert
            // BUG: GetTotal negatif kupon kontrolü yapmıyor → FAIL
            Assert.Throws<ArgumentException>(() =>
            {
                var total = cart.GetTotal(couponDiscount: -200m); // Negatif kupon
            }, "Negatif kupon indirimi kabul edilmemeli!");
        }

        // ──────────────────────────────────────────────────────────
        // TC-GB-04: Müşteriye ait siparişler listelenebilmeli
        // Beklenen: PASS ✓
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("GrayBox")]
        [Description("TC-GB-04: GetCustomerOrders doğru müşterinin siparişlerini döndürmeli")]
        public void GetCustomerOrders_ShouldReturnCorrectOrders()
        {
            // Arrange
            var service = new OrderService();

            var cart1 = new Cart("musteri-A");
            cart1.AddItem(new Product(4, "Ürün X", 100m, 20), 1);
            service.CreateOrder(cart1, PaymentMethod.Cash);

            var cart2 = new Cart("musteri-B");
            cart2.AddItem(new Product(5, "Ürün Y", 200m, 20), 1);
            service.CreateOrder(cart2, PaymentMethod.CreditCard);

            var cart3 = new Cart("musteri-A");
            cart3.AddItem(new Product(6, "Ürün Z", 300m, 20), 1);
            service.CreateOrder(cart3, PaymentMethod.CreditCard);

            // Act
            var ordersA = service.GetCustomerOrders("musteri-A");

            // Assert
            Assert.That(ordersA.Count, Is.EqualTo(2),
                "Müşteri A'nın 2 siparişi olmalı!");
            Assert.That(ordersA.All(o => o.CustomerId == "musteri-A"), Is.True);
        }
    }
}
