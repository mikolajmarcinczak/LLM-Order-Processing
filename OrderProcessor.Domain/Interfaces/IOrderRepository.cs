using System.Collections.Generic;
using System.Threading.Tasks;
using OrderProcessor.Domain.Entities;

namespace OrderProcessor.Domain.Interfaces;

public interface IOrderRepository
{
  Task AddOrderAsync(Order order);
  Task<IEnumerable<Order>> GetAllOrdersAsync();
  Task<Order> GetOrderByIdAsync(int id);
}