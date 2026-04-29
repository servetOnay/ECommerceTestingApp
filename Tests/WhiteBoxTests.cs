using NUnit.Framework;
using ECommerceApp.Core;

namespace ECommerceApp.Tests
{
    /// <summary>
    /// WHITE BOX TESTLER - İç implementasyonu bilerek test ediyoruz.
    /// Kaynak koda erişimimiz var; bug'ları direktif olarak hedefliyoruz.
    /// </summary>
    [TestFixture]
    public class WhiteBoxTests
    {
        // ──────────────────────────────────────────────────────────
        // TC-WB-01: Product negatif fiyat kabul etmemeli
        // Beklenen: Exception | Sonuç: FAIL ✗ (bug: validation yok)
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("WhiteBox")]
        [Description("TC-WB-01: Negatif fiyatlı ürün oluşturulunca exception fırlatılmalı")]
        public void Product_NegativePrice_ShouldThrowException()
        {
            // Arrange & Act & Assert
            // BUG: Constructor'da fiyat validasyonu yok → test FAIL olacak
            Assert.Throws<ArgumentException>(() =>
            {
                var product = new Product(1, "Test Ürün", -50m, 10);
            }, "Negatif fiyat kabul edilmemeli!");
        }

        // ──────────────────────────────────────────────────────────
        // TC-WB-02: ReduceStock stoku negatife düşürmemeli
        // Beklenen: Exception | Sonuç: FAIL ✗ (bug: kontrol yok)
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("WhiteBox")]
        [Description("TC-WB-02: Stoktan fazla miktar düşülünce exception fırlatılmalı")]
        public void Product_ReduceStockBeyondAvailable_ShouldThrowException()
        {
            // Arrange
            var product = new Product(1, "Laptop", 5000m, 3);

            // Act & Assert
            // BUG: ReduceStock kontrolsüzce çıkarıyor → Stock = -7, exception yok → FAIL
            Assert.Throws<InvalidOperationException>(() =>
            {
                product.ReduceStock(10); // 3 stok var, 10 çıkarılmak isteniyor
            });
        }

        // ──────────────────────────────────────────────────────────
        // TC-WB-03: GetDiscountedPrice %100+ indirime izin vermemeli
        // Beklenen: Exception | Sonuç: FAIL ✗ (bug: kontrol yok)
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("WhiteBox")]
        [Description("TC-WB-03: %100 üzeri indirim negatif fiyat üretmemeli")]
        public void Product_DiscountOver100Percent_ShouldThrowException()
        {
            // Arrange
            var product = new Product(2, "Mouse", 200m, 5);

            // Act & Assert
            // BUG: %150 indirim → -100 TL fiyat döner → FAIL
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var discounted = product.GetDiscountedPrice(150);
            });
        }

        // ──────────────────────────────────────────────────────────
        // TC-WB-04: Geçerli indirim doğru hesaplanmalı
        // Beklenen: PASS ✓ (bu kısım düzgün çalışıyor)
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("WhiteBox")]
        [Description("TC-WB-04: %20 indirim doğru hesaplanmalı")]
        public void Product_ValidDiscount_CalculatesCorrectly()
        {
            // Arrange
            var product = new Product(3, "Kulaklık", 500m, 10);

            // Act
            var discounted = product.GetDiscountedPrice(20); // %20 indirim

            // Assert
            Assert.That(discounted, Is.EqualTo(400m));
        }

        // ──────────────────────────────────────────────────────────
        // TC-WB-05: Cart.GetTotal çifte indirim uyguluyor
        // Beklenen: 100 TL indirim → 900 TL | Gerçek: 800 TL → FAIL ✗
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("WhiteBox")]
        [Description("TC-WB-05: 100 TL kupon indirimi toplam fiyattan bir kez düşülmeli")]
        public void Cart_CouponDiscount_ShouldApplyOnce()
        {
            // Arrange
            var cart = new Cart("musteri-001");
            var product = new Product(1, "Ürün A", 1000m, 10);
            cart.AddItem(product, 1);

            // Act
            var total = cart.GetTotal(couponDiscount: 100m); // 100 TL kupon

            // Assert
            // BUG: GetTotal 100'ü iki kez çıkarıyor → 800 TL döner, 900 bekleniyor → FAIL
            Assert.That(total, Is.EqualTo(900m),
                "100 TL kupon indirimi yalnızca bir kez uygulanmalı!");
        }

        // ──────────────────────────────────────────────────────────
        // TC-WB-06: AddItem aynı ürünü duplicate eklememeli
        // Beklenen: 1 item, Quantity=2 | Gerçek: 2 item → FAIL ✗
        // ──────────────────────────────────────────────────────────
        [Test]
        [Category("WhiteBox")]
        [Description("TC-WB-06: Aynı ürün iki kez eklenince miktar artmalı, duplicate olmamalı")]
        public void Cart_AddSameProductTwice_ShouldUpdateQuantityNotDuplicate()
        {
            // Arrange
            var cart = new Cart("musteri-002");
            var product = new Product(5, "Kalem", 10m, 50);

            // Act
            cart.AddItem(product, 1);
            cart.AddItem(product, 1); // Aynı ürün tekrar ekleniyor

            // Assert
            // BUG: 2 ayrı CartItem oluşuyor, Items.Count = 2 → FAIL
            Assert.That(cart.Items.Count, Is.EqualTo(1),
                "Aynı ürün birden fazla CartItem oluşturmamalı!");
            Assert.That(cart.Items[0].Quantity, Is.EqualTo(2),
                "Ürün miktarı 2 olmalıydı!");
        }
    }
}
