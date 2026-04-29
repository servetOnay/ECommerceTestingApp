using ECommerceApp.Core;

Console.WriteLine("=== E-TİCARET SİSTEMİ - DEMO ===\n");

// Ürünler oluştur
var laptop   = new Product(1, "Gaming Laptop",    25000m, 10, "Elektronik");
var mouse    = new Product(2, "Wireless Mouse",     500m, 50, "Aksesuar");
var keyboard = new Product(3, "Mekanik Klavye",    1500m,  3, "Aksesuar");

Console.WriteLine("📦 Ürünler:");
Console.WriteLine($"  {laptop}");
Console.WriteLine($"  {mouse}");
Console.WriteLine($"  {keyboard}\n");

// Sepet oluştur
var cart = new Cart("demo-musteri");
cart.AddItem(laptop, 1);
cart.AddItem(mouse, 2);
Console.WriteLine($"🛒 {cart}");
Console.WriteLine($"   Subtotal: {cart.GetSubtotal():C}");
Console.WriteLine($"   GetTotal(100 TL kupon): {cart.GetTotal(100):C}  ← BUG: 200 TL düşüyor!\n");

// Sipariş oluştur
var service = new OrderService();
var order = service.CreateOrder(cart, PaymentMethod.CreditCard);
Console.WriteLine($"📋 {order}");
Console.WriteLine($"   Laptop stoğu sipariş sonrası: {laptop.Stock}  ← BUG: 10 kalmalı, 9 olmalıydı!\n");

// Ödeme
service.ProcessPayment(order, order.TotalAmount);
Console.WriteLine($"💳 Ödeme sonrası durum: {order.Status}");
Console.WriteLine("\n✅ Test raporunu görmek için Tests/TestReport.md dosyasını inceleyin.");
