using Sample.Statics.Repository.Models;
using Statics.ServiceBroker.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Statics.Repository.Service;

[StaticsService(Entity = typeof(Order), ApiNamespace="my.app")]
[StaticsApiService(Entity = typeof(Order), ApiNamespace="my.app")]
internal class OrderServiceImpl
{
	// [StaticsServiceMethod(MethodName="byId", MapApi=true)]

	[StaticsServiceMethod(MethodName="purchase", MapApi=true)]
	internal static Order Purchase(Guid orderId, decimal amount) => new Order();

	// will be asp.net mapped, but not part of service broker
	[StaticsApiServiceMethod(Path="/order/{id:key}:purchaseNow?{query}&{other}", HttpMethod="POST")]
	internal static bool PurchaseNow(Guid orderId, decimal amount) => true;
}
