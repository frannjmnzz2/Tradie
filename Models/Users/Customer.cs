﻿using Tradie.Models.Orders;
using Tradie.Models.Payments;
using Tradie.Models.Products;

namespace Tradie.Models.Users
{
    public class Customer : User
    {
        public Customer()
        {
            Role = UserRole.Customer;
            ShoppingCart = new Tradie.Models.ShoppingCart.ShoppingCart();
            Orders = new List<Order>();
        }
        public Tradie.Models.ShoppingCart.ShoppingCart ShoppingCart { get; private set; }
        public List<Order> Orders { get; private set; }

        public void BrowseProducts(Category category) { /* ... */ }

        public List<Product> SearchProducts(string query)
        {
            return new List<Product>();
        }

        public void AddToCart(Product product, int quantity)
        {
            ShoppingCart.AddItem(product, quantity);
        }
        public Order Checkout(PaymentMethod paymentMethod)
        {
            int id = 0;
            Customer customer = this;
            DateTime orderDate = DateTime.Now;
            OrderStatus status = OrderStatus.Pending;
            List<OrderItem> items;
            var order = new Order(id, customer, orderDate, status, items);
            Orders.Add(order);
            ShoppingCart.Clear();
            return order;
        }
        public void WriteProductReview(Product product, Review review)
        {
            product.AddReview(review);
        }
    }
}
