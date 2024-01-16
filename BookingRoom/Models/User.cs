namespace BookingRoom.Models
{
	public class User
	{
		public string name { get; set; }
		public string email { get; set; }
		public string password { get; set; }
		public string userName { get; set; }
		public string fcmToken { get; set; }
		public string phoneNumber{ get; set; }
		public string role { get; set; }
		public string url { get; set; }
	}
	public static class CurrentUser
	{
		public static string name { get; set; }
		public static string id { get; set; }	
		public static string university { get; set; }
	}
	
	
}
