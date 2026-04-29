# 📊 E-Ticaret Sistemi — Test Raporu

**Proje:** ECommerceApp  
**Test Framework:** NUnit 4.x   
**Toplam Test:** 20 | ✅ Pass: 11 | ❌ Fail: 9 | **Başarı Oranı: %55**

---

## 🗂️ Proje Yapısı

```
ECommerceApp/
├── Core/
│   ├── Product.cs          ← Ürün modeli (2 kasıtlı bug)
│   ├── Cart.cs             ← Sepet (2 kasıtlı bug)
│   └── OrderService.cs     ← Sipariş servisi (2 kasıtlı bug)
├── Tests/
│   ├── UnitTests/
│   │   ├── WhiteBoxTests.cs    ← 6 test (3 fail)
│   │   ├── BlackBoxTests.cs    ← 6 test (2 fail)
│   │   └── GrayBoxTests.cs     ← 4 test (1 fail)
│   └── IntegrationTests/
│       └── IntegrationTests.cs ← 4 test (1 fail)
└── Program.cs
```

---

## 🐛 Sistemdeki Kasıtlı Hatalar (Bugs)

| # | Dosya | Metot | Hata Açıklaması |
|---|-------|-------|-----------------|
| B1 | `Product.cs` | Constructor | Negatif fiyat/stok değerleri için validation yok |
| B2 | `Product.cs` | `ReduceStock()` | Stok sınırı kontrolü yok → negatif stok oluşuyor |
| B3 | `Product.cs` | `GetDiscountedPrice()` | %100+ indirim engellenmiyor → negatif fiyat |
| B4 | `Cart.cs` | `AddItem()` | Aynı ürün tekrar eklenince miktar güncellenmez, duplicate oluşur |
| B5 | `Cart.cs` | `GetTotal()` | Kupon indirimi iki kez uygulanıyor (çifte indirim) |
| B6 | `OrderService.cs` | `ProcessPayment()` | Eksik ödeme de siparişi Confirmed yapıyor |
| B7 | `OrderService.cs` | `CreateOrder()` | Sipariş verilince stok düşülmüyor |

---

## 1️⃣ WHITE BOX TEST SONUÇLARI

> İç implementasyona tam erişimle yapılan testler. Hangi satırın hatalı olduğunu bilerek hedefliyoruz.

| Test ID | Test Adı | Beklenen | Gerçek Sonuç | Durum | Fail Nedeni |
|---------|----------|----------|--------------|-------|-------------|
| TC-WB-01 | Negatif fiyatlı ürün oluşturma | `ArgumentException` fırlatılmalı | Exception yok, ürün oluştu | ❌ **FAIL** | Constructor'da `Price < 0` kontrolü yok |
| TC-WB-02 | Stoktan fazla miktar düşme | `InvalidOperationException` | Exception yok, Stock = -7 | ❌ **FAIL** | `ReduceStock()` içinde `quantity > Stock` koşulu yok |
| TC-WB-03 | %100+ indirim uygulaması | `ArgumentOutOfRangeException` | Exception yok, `-100 TL` döndü | ❌ **FAIL** | `GetDiscountedPrice()` yüzde aralığı kontrolü yok |
| TC-WB-04 | %20 indirim doğru hesaplama | `400 TL` | `400 TL` | ✅ **PASS** | — |
| TC-WB-05 | 100 TL kupon bir kez uygulanmalı | `900 TL` | `800 TL` | ❌ **FAIL** | `GetTotal()` içinde `couponDiscount` iki kez çıkarılıyor |
| TC-WB-06 | Aynı ürün duplicate eklenmemeli | `Items.Count = 1, Qty = 2` | `Items.Count = 2` | ❌ **FAIL** | `AddItem()` mevcut item kontrolü yapmıyor |

**Özet:** 6 testten 4'ü FAIL ❌

---

## 2️⃣ BLACK BOX TEST SONUÇLARI

> Kullanıcı bakış açısı: Sadece girdi/çıktı. İç koda bakılmaz.

| Test ID | Test Adı | Beklenen | Gerçek Sonuç | Durum | Fail Nedeni |
|---------|----------|----------|--------------|-------|-------------|
| TC-BB-01 | Stoklu ürün sepete ekleme | Başarılı ekleme | Başarılı ekleme | ✅ **PASS** | — |
| TC-BB-02 | Stoksuz ürün sepete eklenemez | `InvalidOperationException` | Exception fırlatıldı | ✅ **PASS** | — |
| TC-BB-03 | Boş sepet ile sipariş verilemez | `InvalidOperationException` | Exception fırlatıldı | ✅ **PASS** | — |
| TC-BB-04 | Tam ödeme → Confirmed statüsü | `Status = Confirmed` | `Status = Confirmed` | ✅ **PASS** | — |
| TC-BB-05 | Eksik ödeme → Confirmed olmamalı | `Status ≠ Confirmed` | `Status = Confirmed` | ❌ **FAIL** | `ProcessPayment()` sadece `> 0` kontrol ediyor, tutar karşılaştırması yok |
| TC-BB-06 | Sipariş sonrası stok azalmalı | `Stock = 3` (5-2) | `Stock = 5` | ❌ **FAIL** | `CreateOrder()` stok düşme işlemi yapmıyor |

**Özet:** 6 testten 2'si FAIL ❌

---

## 3️⃣ GRAY BOX TEST SONUÇLARI

> Kısmi iç bilgi: Metodların varlığını ve veri yapılarını biliyoruz.

| Test ID | Test Adı | Beklenen | Gerçek Sonuç | Durum | Fail Nedeni |
|---------|----------|----------|--------------|-------|-------------|
| TC-GB-01 | Sipariş sonrası sepet temizlenir | `cart.IsEmpty = true` | `true` | ✅ **PASS** | — |
| TC-GB-02 | Kargodaki sipariş iptal edilemez | `InvalidOperationException` | Exception fırlatıldı | ✅ **PASS** | — |
| TC-GB-03 | Negatif kupon reddedilmeli | `ArgumentException` | Exception yok, hesaplama devam etti | ❌ **FAIL** | `GetTotal()` negatif kupon parametresini validate etmiyor |
| TC-GB-04 | Müşteri siparişleri doğru listelenmeli | `Count = 2` (musteri-A) | `Count = 2` | ✅ **PASS** | — |

**Özet:** 4 testten 1'i FAIL ❌

---

## 4️⃣ INTEGRATION TEST SONUÇLARI

> Uçtan uca sistem akışları test edildi.

| Test ID | Test Adı | Beklenen | Gerçek Sonuç | Durum | Fail Nedeni |
|---------|----------|----------|--------------|-------|-------------|
| TC-IT-01 | Tam alışveriş akışı (happy path) | Tüm adımlar başarılı | Başarılı | ✅ **PASS** | — |
| TC-IT-02 | Sipariş sonrası stok entegrasyonu | `Stock = 7` (10-3) | `Stock = 10` | ❌ **FAIL** | `CreateOrder()` `ReduceStock()` çağırmıyor — **KRİTİK** |
| TC-IT-03 | Pending sipariş iptali | `Status = Cancelled` | `Status = Cancelled` | ✅ **PASS** | — |
| TC-IT-04 | Çoklu müşteri izolasyonu | Siparişler birbirini etkilemiyor | İzole çalışıyor | ✅ **PASS** | — |

**Özet:** 4 testten 1'i FAIL ❌

---

## 📈 Genel Özet Tablosu

| Test Türü | Toplam | ✅ Pass | ❌ Fail | Başarı Oranı |
|-----------|--------|---------|---------|-------------|
| White Box | 6 | 2 | 4 | %33 |
| Black Box | 6 | 4 | 2 | %67 |
| Gray Box | 4 | 3 | 1 | %75 |
| Integration | 4 | 3 | 1 | %75 |
| **TOPLAM** | **20** | **11** | **9** | **%55** |

> ⚠️ Not: TC-IT-02 ve TC-BB-06 aynı bug'ı (B7) farklı katmanlarda test ediyor.

---

## 🔧 Düzeltme Önerileri

### Bug B1 — Product Constructor Validation
```csharp
// ❌ Mevcut (hatalı)
public Product(int id, string name, decimal price, int stock, ...)
{
    Price = price;
    Stock = stock;
}

// ✅ Düzeltilmiş
if (price < 0)  throw new ArgumentException("Fiyat negatif olamaz.", nameof(price));
if (stock < 0)  throw new ArgumentException("Stok negatif olamaz.", nameof(stock));
Price = price;
Stock = stock;
```

### Bug B2 — ReduceStock Sınır Kontrolü
```csharp
// ✅ Düzeltilmiş
public void ReduceStock(int quantity)
{
    if (quantity > Stock)
        throw new InvalidOperationException($"Yetersiz stok. Mevcut: {Stock}, İstenen: {quantity}");
    Stock -= quantity;
}
```

### Bug B3 — GetDiscountedPrice Aralık Kontrolü
```csharp
// ✅ Düzeltilmiş
public decimal GetDiscountedPrice(double discountPercent)
{
    if (discountPercent < 0 || discountPercent > 100)
        throw new ArgumentOutOfRangeException(nameof(discountPercent), "İndirim 0-100 arasında olmalı.");
    return Price - (Price * (decimal)discountPercent / 100);
}
```

### Bug B4 — AddItem Duplicate Kontrolü
```csharp
// ✅ Düzeltilmiş
public void AddItem(Product product, int quantity)
{
    var existing = _items.FirstOrDefault(i => i.Product.Id == product.Id);
    if (existing != null)
        existing.Quantity += quantity;
    else
        _items.Add(new CartItem(product, quantity));
}
```

### Bug B5 — GetTotal Çifte İndirim
```csharp
// ✅ Düzeltilmiş
public decimal GetTotal(decimal couponDiscount = 0)
{
    if (couponDiscount < 0) throw new ArgumentException("Kupon indirimi negatif olamaz.");
    decimal total = _items.Sum(i => i.Product.Price * i.Quantity) - couponDiscount;
    return total < 0 ? 0 : total;
}
```

### Bug B6 — ProcessPayment Tutar Kontrolü
```csharp
// ✅ Düzeltilmiş
public bool ProcessPayment(Order order, decimal paymentAmount)
{
    if (paymentAmount < order.TotalAmount) return false; // Eksik ödeme reddedilir
    order.IsPaid = true;
    order.Status = OrderStatus.Confirmed;
    return true;
}
```

### Bug B7 — CreateOrder Stok Düşme
```csharp
// ✅ Düzeltilmiş
public Order CreateOrder(Cart cart, PaymentMethod paymentMethod)
{
    // ... mevcut kod ...
    foreach (var item in cart.Items)
        item.Product.ReduceStock(item.Quantity); // EKSIK SATIR!
    _orders.Add(order);
    cart.Clear();
    return order;
}
```

---

## 🎯 Test Metodolojisi Özeti

### White Box
Kodun içine bakarak zayıf noktaları direkt hedefledik. `ReduceStock`, `GetDiscountedPrice`, `GetTotal` ve constructor gibi metodların iç implementasyonunu bilerek sınır değerlerini test ettik.

### Black Box
Kullanıcı senaryolarını taklit ettik: "Sepete ekle → Sipariş ver → Öde." Kodun nasıl çalıştığını bilmeden beklenen davranışları test ettik. İki kritik kullanıcı senaryosunda (eksik ödeme, stok takibi) bug yakaladık.

### Gray Box
Sistemin veri modelini ve temel metodları bilerek ancak implementasyon detaylarına bakmadan test ettik. Özellikle sınır durumlarına (negatif kupon, iptal koşulları) odaklandık.

### Integration
Tüm katmanların (Product + Cart + OrderService) birlikte çalıştığı end-to-end senaryoları test ettik. En kritik entegrasyon hatası: stok yönetiminin sipariş servisiyle entegre olmaması.

---

