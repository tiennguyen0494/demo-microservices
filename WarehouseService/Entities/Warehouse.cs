using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WarehouseService.Entities
{
	public class Warehouse
	{
		[Key]
		public Guid Id { get; set; }

		public Guid ProductId { get; set; }

		public int Quantity { get; set; }
	}
}

