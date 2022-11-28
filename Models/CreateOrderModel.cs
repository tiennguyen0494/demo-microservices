using System;
namespace Models
{
	public class CreateOrderModel
	{
		public Guid OrderId { get; set; }

		public Guid UserId { get; set; }

		public List<OrderItemModel> ProductList { get; set; } = new List<OrderItemModel>();
	}

	public class OrderItemModel
	{
		public Guid ProductId { get; set; }

		public int Quantity { get; set; }
	}
}

