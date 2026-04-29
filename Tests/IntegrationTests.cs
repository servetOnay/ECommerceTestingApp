using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests
{
    /// <summary>
    /// INTEGRATION TESTLER - Tüm sistemin uçtan uca çalışmasını test ediyoruz.
    /// Product + Cart + OrderService birlikte çalışmalı.
    /// </summary>
    [TestFixture]
    public class IntegrationTests
    {
        private OrderService _orderService;

        [SetUp]
        public void SetUp()
        {
            _orderService = new OrderService();
        }

        // ──────────────────────────────────────────────────────────
        // TC-IT-01: Tam alışveriş akışı - mutlu yol (happy path)
        // Ürün seç → Sepete ekle → Sipariş ver → Ödeme yap
        // Beklenen: PASS ✓
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("Integration")]
        [Description("TC-IT-01: Eksiksiz alışveriş akışı başarıyla tamamlanmalı")]
        public void FullShoppingFlow_HappyPath_ShouldSucceed()
        {
            // Arrange
            var laptop = new Product(101, "Gaming Laptop", 25000m, 10);
            var mouse  = new Product(102, "Wireless Mouse",  500m, 50);
            var cart   = new Cart("entegrasyon-musteri-01");

            // Act - Adım 1: Ürünleri sepete ekle
            cart.AddItem(laptop, 1);
            cart.AddItem(mouse, 2);

            // Assert - Sepet kontrolü
            Assert.That(cart.ItemCount, Is.EqualTo(3));
            Assert.That(cart.GetSubtotal(), Is.EqualTo(26000m));

            // Act - Adım 2: Sipariş oluştur
            var order = _orderService.CreateOrder(cart, PaymentMethod.CreditCard);

            // Assert - Sipariş kontrolü
            Assert.That(order, Is.Not.Null);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Pending));
            Assert.That(order.TotalAmount, Is.EqualTo(26000m));
            Assert.That(cart.IsEmpty, Is.True, "Sipariş sonrası sepet boşalmalı");

            // Act - Adım 3: Ödeme yap
            bool paid = _orderService.ProcessPayment(order, 26000m);

            // Assert - Ödeme kontrolü
            Assert.That(paid, Is.True);
            Assert.That(order.IsPaid, Is.True);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Confirmed));
        }

        // ──────────────────────────────────────────────────────────
        // TC-IT-02: Sipariş sonrası stok güncellemesi
        // Beklenen: Stok azalır | Gerçek: Azalmıyor → FAIL ✗
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("Integration")]
        [Description("TC-IT-02: Sipariş verilince stok otomatik güncellenmeli")]
        public void CreateOrder_Integration_StockShouldDecrease()
        {
            // Arrange
            var phone = new Product(201, "Akıllı Telefon", 15000m, 10);
            var cart  = new Cart("entegrasyon-musteri-02");
            cart.AddItem(phone, 3);

            // Act
            _orderService.CreateOrder(cart, PaymentMethod.BankTransfer);

            // Assert
            // BUG: Stok düşme yok → hâlâ 10 → FAIL
            Assert.That(phone.Stock, Is.EqualTo(7),
                "3 adet sipariş sonrası stok 10→7 olmalıydı!");
        }

        // ──────────────────────────────────────────────────────────
        // TC-IT-03: İptal edilen sipariş durumu
        // Beklenen: PASS ✓
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("Integration")]
        [Description("TC-IT-03: Pending sipariş iptal edilebilmeli")]
        public void CancelOrder_PendingOrder_ShouldSucceed()
        {
            // Arrange
            var cart = new Cart("entegrasyon-musteri-03");
            cart.AddItem(new Product(301, "Kulaklık", 2000m, 5), 1);
            var order = _orderService.CreateOrder(cart, PaymentMethod.Cash);

            // Act
            bool cancelled = _orderService.CancelOrder(order.Id);

            // Assert
            Assert.That(cancelled, Is.True);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Cancelled));
        }

        // ──────────────────────────────────────────────────────────
        // TC-IT-04: Çoklu müşteri siparişleri birbirini etkilememeli
        // Beklenen: PASS ✓
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("Integration")]
        [Description("TC-IT-04: Farklı müşterilerin siparişleri izole olmalı")]
        public void MultipleCustomers_OrdersShouldBeIsolated()
        {
            // Arrange & Act
            var sharedProduct = new Product(401, "Ortak Ürün", 100m, 100);

            var cart1 = new Cart("musteri-X");
            cart1.AddItem(sharedProduct, 5);
            var order1 = _orderService.CreateOrder(cart1, PaymentMethod.CreditCard);

            var cart2 = new Cart("musteri-Y");
            cart2.AddItem(sharedProduct, 3);
            var order2 = _orderService.CreateOrder(cart2, PaymentMethod.Cash);

            // Assert
            Assert.That(order1.CustomerId, Is.EqualTo("musteri-X"));
            Assert.That(order2.CustomerId, Is.EqualTo("musteri-Y"));
            Assert.That(order1.Id, Is.Not.EqualTo(order2.Id));

            var xOrders = _orderService.GetCustomerOrders("musteri-X");
            var yOrders = _orderService.GetCustomerOrders("musteri-Y");

            Assert.That(xOrders.Count, Is.EqualTo(1));
            Assert.That(yOrders.Count, Is.EqualTo(1));
        }
    }
}
