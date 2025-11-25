using System.ComponentModel.DataAnnotations;

namespace AirGnG.Models
{

        public class Order
        {
            public int Id { get; set; }

            [DataType(DataType.DateTime)]
            public DateTime OrderTime { get; set; }

            [DataType(DataType.DateTime)]
            public DateTime CompletionTime { get; set; }

            // Navigation property to link MenuItems in this order
            public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

            public Order()
            {
                // Set OrderTime to now and CompletionTime to 30 minutes later
                OrderTime = DateTime.Now;
                CompletionTime = OrderTime.AddMinutes(30);
            }
        }

        // Helper model to link MenuItems to an Order (many-to-many relationship)
        public class OrderItem
        {
            public int Id { get; set; }
            public int OrderId { get; set; }
            public int MenuItemId { get; set; }
            public int Quantity { get; set; } // Added quantity for realism

            public Order Order { get; set; } = null!;
            public MenuItem MenuItem { get; set; } = null!;
        }
    
}
