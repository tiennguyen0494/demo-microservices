using System;
namespace Models
{
	public class OrderHasBeenCreatedModel : CreateOrderModel
	{
		public bool IsCompleted { get; set; }
        public string Message { get; set; }
    }
}

