using System;
using WarehouseService.Entities;
using Microsoft.EntityFrameworkCore;

namespace UserService.Entities
{
	public class WarehouseDbContext : DbContext
	{
		public WarehouseDbContext(DbContextOptions options) : base(options) 
		{
		}

		public DbSet<Warehouse> Warehouses { get; set; }
	}
}

