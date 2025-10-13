using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sample.Statics.Repository.DB;
using Sample.Statics.Repository.Models;
using Statics.ServiceBroker.Attributes;

namespace Sample.Statics.Repository;

[StaticsRepository(Entity = typeof(User), CrudContext = typeof(SampleDbContext), CrudCalls = ["GET"])]
internal static class UserRepositoryImpl
{

	[StaticsRepositoryMethod(MethodName="byId")]
	public static string GetUserById(int id) => $"User_{id}";


	[StaticsRepositoryMethod(MethodName="create", IsInstanceMethod=false)]
	public static User Add(string name, string email)
	{
		return new User
		{
			Id = new Random().Next(1, 1000),
			UserName = name,
			Email = email
		};
	}
}


[StaticsRepository]
internal static class ImageServiceRepositoryImpl
{
	/// <summary>
	/// Converts an image to a profile picture by cropping it to a circle.
	/// </summary>
	/// <param name="input"></param>
	/// <param name="mimeType"></param>
	/// <param name="zoomLevel"></param>
	/// <param name="radius"></param>
	/// <returns></returns>
	[StaticsRepositoryMethod]
	public static byte[] ToProfilePicture(byte[] input, string mimeType, double centerPoint, double zoomLevel) => new byte[] { 1, 2, 3, 4, 5 };	

}
