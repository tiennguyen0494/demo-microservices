using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Entities
{
	public class OrderItem
	{
		[Key]
        public Guid Id { get; set; }

		public Guid OrderId { get; set; }

		public Guid ProductId { get; set; }

		public int Quantity { get; set; }

		public DateTime CreatedDate { get; set; }

		public virtual Order? Order { get; set; }
	}
}

