using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Amazon.Core.EntitiesInventory;

namespace Amazon.DAL.Inventory
{
    public class InventoryContext : DbContext
    {
        public DbSet<Item> Items { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<Core.EntitiesInventory.Inventory> Inventories { get; set; }
        public DbSet<ItemOrderMapping> ItemOrderMappings { get; set; }
        public DbSet<ItemInventoryMapping> ItemInventoryMappings { get; set; }
        public DbSet<ViewBarcode> ViewBarcodes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemOrderMapping>().HasKey(m => new {m.ItemId, m.OrderId});
            modelBuilder.Entity<ItemInventoryMapping>().HasKey(m => new {m.ItemId, m.InventoryId});
            base.OnModelCreating(modelBuilder);
        }
    }
}
