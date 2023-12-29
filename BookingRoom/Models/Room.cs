namespace BookingRoom.Models
{
	public class Room
	{
		public string RoomId { get; set; }
		public string RoomName { get; set; }
		public int MaxCapacity { get; set;}
		public string Location { get; set; }
		public bool Status { get; set; }
		public string UniversityName { get; set; }
		public string Function { get; set; }
    }
}
