namespace OrderService.Models;

public class CreateOrderRequest
{
    public int UserId { get; set; }

    public string Product { get; set; } = string.Empty;

    public int Quantity { get; set; }
}