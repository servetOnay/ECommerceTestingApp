namespace ECommerceApp.Core
{
    /// <summary>
    /// Ürün modeli - Kasıtlı Bug: Negatif fiyat ve stok kontrolü yapılmıyor.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }     // BUG: Negatif fiyat engellenmiyor
        public int Stock { get; set; }          // BUG: Negatif stok engellenmiyor
        public string Category { get; set; }

        public Product(int id, string name, decimal price, int stock, string category = "General")
        {
            Id = id;
            Name = name;
            Price = price;      // BUG: Validation yok - negatif değer atanabilir
            Stock = stock;      // BUG: Validation yok - negatif değer atanabilir
            Category = category;
        }

        /// <summary>
        /// Stoktan düş - BUG: quantity > stock olduğunda hata fırlatmıyor, negatif stok oluşuyor!
        /// </summary>
        public void ReduceStock(int quantity)
        {
            // BUG: Stock < quantity kontrolü YOK → negatif stok!
            Stock -= quantity;
        }

        /// <summary>
        /// Stok ekle - düzgün çalışıyor
        /// </summary>
        public void AddStock(int quantity)
        {
            if (quantity < 0)
                throw new ArgumentException("Eklenecek miktar negatif olamaz.");
            Stock += quantity;
        }

        /// <summary>
        /// İndirimli fiyat hesapla - BUG: %100'den fazla indirim engellenmiyor
        /// </summary>
        public decimal GetDiscountedPrice(double discountPercent)
        {
            // BUG: discountPercent > 100 olursa negatif fiyat döner!
            return Price - (Price * (decimal)discountPercent / 100);
        }

        public bool IsInStock() => Stock > 0;

        public override string ToString() =>
            $"[{Id}] {Name} - {Price:C} (Stok: {Stock})";
    }
}
