using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests
{
    /// <summary>
    /// BLACK BOX TESTLER - Sadece girdi/çıktı ile test; iç koda bakmıyoruz.
    /// Kullanıcı bakış açısıyla: "Bu işlev böyle davranmalı."
    /// </summary>
    [TestFixture]
    public class BlackBoxTests
    {
        // ──────────────────────────────────────────────────────────
        // TC-BB-01: Stokta olan ürün sepete eklenebilmeli
        // Beklenen: PASS ✓
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("BlackBox")]
        [Description("TC-BB-01: Stoklu ürün sepete başarıyla eklenmeli")]
        public void AddItem_InStockProduct_ShouldSucceed()
        {
            // Arrange
            var cart = new Cart("kullanici-01");
            var product = new Product(10, "Tablet", 3000m, 5);

            // Act & Assert
            Assert.DoesNotThrow(() => cart.AddItem(product, 2),
                "Stoklu ürün sepete eklenebilmeli");
            Assert.That(cart.IsEmpty, Is.False);
        }

        // ──────────────────────────────────────────────────────────
        // TC-BB-02: Stokta olmayan ürün sepete eklenemez
        // Beklenen: InvalidOperationException | Sonuç: PASS ✓
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("BlackBox")]
        [Description("TC-BB-02: Stoksuz ürün sepete eklenmeye çalışılınca hata fırlatılmalı")]
        public void AddItem_OutOfStockProduct_ShouldThrowException()
        {
            // Arrange
            var cart = new Cart("kullanici-02");
            var product = new Product(11, "Klavye", 500m, 0); // Stok = 0

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                cart.AddItem(product, 1),
                "Stoksuz ürün sepete eklenemez!");
        }

        // ──────────────────────────────────────────────────────────
        // TC-BB-03: Boş sepet ile sipariş verilememeli
        // Beklenen: InvalidOperationException | Sonuç: PASS ✓
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("BlackBox")]
        [Description("TC-BB-03: Boş sepet ile sipariş oluşturulamaz")]
        public void CreateOrder_EmptyCart_ShouldThrowException()
        {
            // Arrange
            var cart = new Cart("kullanici-03");
            var service = new OrderService();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                service.CreateOrder(cart, PaymentMethod.CreditCard),
                "Boş sepet ile sipariş oluşturulamaz!");
        }

        // ──────────────────────────────────────────────────────────
        // TC-BB-04: Ödeme sonrası sipariş "Confirmed" durumuna geçmeli
        // Beklenen: PASS ✓ (ama eksik ödeme de geçiyor - bu ayrı bir bug)
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("BlackBox")]
        [Description("TC-BB-04: Tam ödeme sonrası sipariş Confirmed statüsünde olmalı")]
        public void ProcessPayment_FullAmount_OrderShouldBeConfirmed()
        {
            // Arrange
            var cart = new Cart("kullanici-04");
            cart.AddItem(new Product(1, "Ürün", 200m, 10), 1);
            var service = new OrderService();
            var order = service.CreateOrder(cart, PaymentMethod.CreditCard);

            // Act
            bool result = service.ProcessPayment(order, 200m); // Tam ödeme

            // Assert
            Assert.That(result, Is.True);
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Confirmed));
            Assert.That(order.IsPaid, Is.True);
        }

        // ──────────────────────────────────────────────────────────
        // TC-BB-05: Eksik ödeme yapılınca sipariş Confirmed OLMAMALI
        // Beklenen: Confirmed değil | Gerçek: Confirmed → FAIL ✗ (bug!)
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("BlackBox")]
        [Description("TC-BB-05: Eksik ödeme yapılınca sipariş onaylanmamalı")]
        public void ProcessPayment_PartialAmount_OrderShouldNotBeConfirmed()
        {
            // Arrange
            var cart = new Cart("kullanici-05");
            cart.AddItem(new Product(2, "Telefon", 10000m, 5), 1);
            var service = new OrderService();
            var order = service.CreateOrder(cart, PaymentMethod.BankTransfer);

            // Act
            service.ProcessPayment(order, 5000m); // Eksik ödeme: 5000 / 10000

            // Assert
            // BUG: ProcessPayment sadece > 0 kontrol ediyor → FAIL
            Assert.That(order.Status, Is.Not.EqualTo(OrderStatus.Confirmed),
                "Eksik ödeme ile sipariş onaylanmamalı!");
        }

        // ──────────────────────────────────────────────────────────
        // TC-BB-06: Sipariş oluştuktan sonra stok azalmalı
        // Beklenen: Stok 3 → Sonuç: Stok 3 (azalmıyor) → FAIL ✗
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("BlackBox")]
        [Description("TC-BB-06: Sipariş verilince ürün stoğu azalmalı")]
        public void CreateOrder_ShouldDecreaseProductStock()
        {
            // Arrange
            var product = new Product(3, "Monitör", 4000m, 5);
            var cart = new Cart("kullanici-06");
            cart.AddItem(product, 2);
            var service = new OrderService();

            // Act
            service.CreateOrder(cart, PaymentMethod.Cash);

            // Assert
            // BUG: CreateOrder stok düşmüyor → Stock hâlâ 5 → FAIL
            Assert.That(product.Stock, Is.EqualTo(3),
                "2 adet siparişten sonra stok 5→3 olmalıydı!");
        }
    }
}
