using Microsoft.EntityFrameworkCore;
using OrderProcessor.Domain.Entities;
using OrderProcessor.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderProcessor.Infrastructure.Data;

public class OrderRepository : IOrderRepository
{
  private readonly OrderProcessorDbContext _context;

  public OrderRepository(OrderProcessorDbContext context)
  {
    _context = context;
  }

  public async Task AddOrderAsync(Order order)
  {
    await _context.Orders.AddAsync(order);
    await _context.SaveChangesAsync();
  }

  public async Task<IEnumerable<Order>> GetAllOrdersAsync()
  {
    return await _context.Orders.Include(o => o.OrderItems).ToListAsync();
  }

  public async Task<Order> GetOrderByIdAsync(int id)
  {
    return await _context.Orders.Include(o => o.OrderItems)
                                .FirstOrDefaultAsync(o => o.Id == id);
  }
}