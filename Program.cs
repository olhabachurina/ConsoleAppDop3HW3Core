using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using static ConsoleAppDop3HW3Core.Program;
namespace ConsoleAppDop3HW3Core;

class Program
{
    static void Main()
    {
        using (var db = new ApplicationContext())
        {
            db.Database.EnsureCreated();
            // Создание нового клиента
            string customerName = "Мария Николаева";
            string customerEmail = "nikolava@ukr.net";
            var customer = db.Customers.FirstOrDefault(c => c.Email == customerEmail);

            if (customer == null)
            {
                customer = new Customer
                {
                    Name = customerName,
                    Email = customerEmail
                };
                db.Customers.Add(customer);
                db.SaveChanges();
                Console.WriteLine("Новый клиент успешно добавлен.");
            }
            else
            {
                Console.WriteLine("Клиент уже существует в базе данных.");
            }
            //Создание нового заказа
            var order = new Order
            {
                TotalAmount = 2000.0m,
                Status = OrderStatus.New.ToString(),
                CustomerId = customer.Id
            };
            db.Orders.Add(order);
            db.SaveChanges();

            Console.WriteLine("Заказ успешно создан.");
            // Чтение заказа
            var savedOrder = db.Orders.FirstOrDefault(o => o.Id == order.Id);
            if (savedOrder != null)
            {
                Console.WriteLine($"Заказ #{savedOrder.Id} на сумму {savedOrder.TotalAmount} для клиента {savedOrder.CustomerId} создан.");
            }
            // Обновление заказа
            if (savedOrder != null)
            {
                savedOrder.TotalAmount = 1100.0m; 
                db.SaveChanges();
                Console.WriteLine($"Заказ #{savedOrder.Id} обновлен. Новая сумма: {savedOrder.TotalAmount}");
            }
            // Удаление заказа
            if (savedOrder != null)
            {
                db.Orders.Remove(savedOrder);
                db.SaveChanges();
                Console.WriteLine($"Заказ #{savedOrder.Id} удален.");
            }
            // Удаление клиента и проверка каскадного удаления заказов
            db.Customers.Remove(customer);
            db.SaveChanges();
            if (!db.Orders.Any(o => o.CustomerId == customer.Id))
            {
                Console.WriteLine("Все заказы связанные с удаленным клиентом также были удалены.");
            }
        }
    }
}

public class Order
    {
        public int Id { get; set; }

        [Required]
        public DateOnly CreationDate { get; private set; } = DateOnly.FromDateTime(DateTime.Now);

        [Range(0, double.MaxValue)]
        public decimal TotalAmount { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public int CustomerId { get; set; }

        public Customer Customer { get; set; }

        public Order()
        {
            Status = OrderStatus.New.ToString();
        }
    }
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [EmailAddress]
        public string Email { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }
    public enum OrderStatus
    {
        New,
        Processing,
        Shipped,
        Delivered,
        Canceled
    }

    public class ApplicationContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }
        public DbSet<Customer> Customers { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=DESKTOP-4PCU5RA\\SQLEXPRESS;Database=OrderManagement;Trusted_Connection=True;TrustServerCertificate=True;");
                optionsBuilder.LogTo(e => Debug.WriteLine(e), new[] { RelationalEventId.CommandExecuted });
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
           
        }
    }
