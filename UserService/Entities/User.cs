using Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Entities
{
	public class User
	{
		[Key]
		public Guid Id { get; set; }

		[MaxLength(250)]
		public string? Name { get; set; }

		public UserStatus Status { get; set; }

		public DateTime CreatedDate { get; set; }
	}
}

