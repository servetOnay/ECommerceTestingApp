namespace ECommerceApp.Core
{
    /// <summary>
    /// Alışveriş sepeti - Kasıtlı Bug: Toplam hesaplama ve miktar kontrolü hatalı.
    /// </summary>
    public class Cart
    {
        private readonly List<CartItem> _items = new();
        public IReadOnlyList<CartItem> Items => _items.AsReadOnly();
        public string CustomerId { get; }

        public Cart(string customerId)
        {
            CustomerId = customerId ?? throw new ArgumentNullException(nameof(customerId));
        }

        /// <summary>
        /// Sepete ürün ekle - BUG: Aynı ürün tekrar eklendiğinde miktar güncellenmek yerine duplicate ekleniyor!
        /// </summary>
        public void AddItem(Product product, int quantity)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (quantity <= 0) throw new ArgumentException("Miktar sıfırdan büyük olmalı.");
            if (!product.IsInStock()) throw new InvalidOperationException("Ürün stokta yok.");

            // BUG: Var olan item kontrolü YOK → aynı ürün birden fazla kez ekleniyor!
            _items.Add(new CartItem(product, quantity));
        }

        /// <summary>
        /// Sepetten ürün çıkar - Doğru çalışıyor
        /// </summary>
        public bool RemoveItem(int productId)
        {
            var item = _items.FirstOrDefault(i => i.Product.Id == productId);
            if (item == null) return false;
            _items.Remove(item);
            return true;
        }

        /// <summary>
        /// Toplam tutarı hesapla - BUG: Kupon indirimi iki kez uygulanıyor!
        /// </summary>
        public decimal GetTotal(decimal couponDiscount = 0)
        {
            decimal subtotal = _items.Sum(i => i.Product.Price * i.Quantity);

            // BUG: İndirim subtotal'dan düşülüp sonra tekrar uygulanıyor → çifte indirim!
            decimal afterDiscount = subtotal - couponDiscount;
            decimal total = afterDiscount - couponDiscount; // BUG: İndirim 2x uygulandı!

            return total < 0 ? 0 : total;
        }

        /// <summary>
        /// Doğru toplam hesaplama (referans için)
        /// </summary>
        public decimal GetSubtotal() =>
            _items.Sum(i => i.Product.Price * i.Quantity);

        public int ItemCount => _items.Sum(i => i.Quantity);

        public bool IsEmpty => !_items.Any();

        public void Clear() => _items.Clear();

        public override string ToString() =>
            $"Sepet ({CustomerId}): {_items.Count} çeşit ürün, Toplam: {GetSubtotal():C}";
    }

    public class CartItem
    {
        public Product Product { get; }
        public int Quantity { get; set; }
        public decimal LineTotal => Product.Price * Quantity;

        public CartItem(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
        }
    }
}
