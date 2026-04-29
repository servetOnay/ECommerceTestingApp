namespace ECommerceApp.Core
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }

    public enum PaymentMethod
    {
        CreditCard,
        BankTransfer,
        Cash
    }

    public class Order
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string CustomerId { get; set; }
        public List<CartItem> Items { get; set; } = new();
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public bool IsPaid { get; set; }

        public override string ToString() =>
            $"Sipariş {Id.ToString()[..8]} | {CustomerId} | {TotalAmount:C} | {Status}";
    }

    /// <summary>
    /// Sipariş servisi - Kasıtlı Bug: Stok düşme ve ödeme onayı hatalı.
    /// </summary>
    public class OrderService
    {
        private readonly List<Order> _orders = new();
        public IReadOnlyList<Order> Orders => _orders.AsReadOnly();

        /// <summary>
        /// Sipariş oluştur - BUG: Stok miktarı kontrol edilmiyor, ürün stoğu düşülmüyor!
        /// </summary>
        public Order CreateOrder(Cart cart, PaymentMethod paymentMethod)
        {
            if (cart == null) throw new ArgumentNullException(nameof(cart));
            if (cart.IsEmpty) throw new InvalidOperationException("Sepet boş, sipariş verilemez.");

            var order = new Order
            {
                CustomerId = cart.CustomerId,
                Items = cart.Items.ToList(),
                TotalAmount = cart.GetSubtotal(),
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.Now,
                PaymentMethod = paymentMethod,
                IsPaid = false
            };

            // BUG: Stok düşme işlemi YOK!
            // Doğrusu: foreach (var item in cart.Items) item.Product.ReduceStock(item.Quantity);

            _orders.Add(order);
            cart.Clear();
            return order;
        }

        /// <summary>
        /// Ödeme işle - BUG: Negatif tutar kontrolü yok; başarısız ödeme yine Confirmed yapılıyor!
        /// </summary>
        public bool ProcessPayment(Order order, decimal paymentAmount)
        {
            if (order == null) throw new ArgumentNullException(nameof(order));

            // BUG: paymentAmount negatif olabilir!
            // BUG: paymentAmount < order.TotalAmount olsa bile Confirmed yapılıyor!
            if (paymentAmount > 0)
            {
                order.IsPaid = true;
                order.Status = OrderStatus.Confirmed; // BUG: Eksik ödeme de Confirmed oluyor!
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sipariş iptal et - Doğru çalışıyor (stok iade edilmiyor ama bu başka bir bug)
        /// </summary>
        public bool CancelOrder(Guid orderId)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId);
            if (order == null) return false;
            if (order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered)
                throw new InvalidOperationException("Kargodaki veya teslim edilmiş sipariş iptal edilemez.");

            order.Status = OrderStatus.Cancelled;
            return true;
        }

        /// <summary>
        /// Sipariş durumunu güncelle
        /// </summary>
        public void UpdateStatus(Guid orderId, OrderStatus newStatus)
        {
            var order = _orders.FirstOrDefault(o => o.Id == orderId)
                ?? throw new KeyNotFoundException($"Sipariş bulunamadı: {orderId}");
            order.Status = newStatus;
        }

        public Order? GetOrder(Guid orderId) =>
            _orders.FirstOrDefault(o => o.Id == orderId);

        public List<Order> GetCustomerOrders(string customerId) =>
            _orders.Where(o => o.CustomerId == customerId).ToList();
    }
}
