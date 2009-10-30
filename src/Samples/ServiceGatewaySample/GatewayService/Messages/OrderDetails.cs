using System;
using GatewayService.Interfaces;

namespace GatewayService.Messages
{
	public class OrderDetails :
		OrderDetailsReceived
	{
		public string OrderId { get;  set; }
		public string CustomerId { get;  set; }
		public DateTime Created { get;  set; }
		public OrderStatus Status { get;  set; }
	}
}