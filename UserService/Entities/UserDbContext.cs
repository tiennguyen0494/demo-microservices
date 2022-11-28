using System;
using Microsoft.EntityFrameworkCore;

namespace UserService.Entities
{
	public class UserDbContext : DbContext
	{
		public UserDbContext(DbContextOptions options) : base(options) 
		{
		}

		public DbSet<User> Users { get; set; }
	}
}

