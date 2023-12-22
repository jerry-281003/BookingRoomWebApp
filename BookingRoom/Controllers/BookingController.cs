using BookingRoom.Models;
using Firebase.Auth;
using Firebase.Storage;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace BookingRoom.Controllers
{
	public class BookingController : Controller
	{
		IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
		{
			AuthSecret = "K1cf1DhpkqxvOkAeqgLElTfzWvDwy09UuRzLrZXU",
			BasePath = "https://booking-room-app-f6938-default-rtdb.asia-southeast1.firebasedatabase.app/"
		};
		IFirebaseClient client;
		private readonly FirebaseStorage _firebaseStorage;
		public BookingController()
		{

			var auth = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("AIzaSyAI67BIPvZDS1mf-dQdSNxp83XmCTlxanI"));
			var authLink = auth.SignInWithEmailAndPasswordAsync("thanhdn21@gmail.com", "thanhpro123").Result;
			_firebaseStorage = new FirebaseStorage(
			"booking-room-app-f6938.firebaseapp.com",
			new FirebaseStorageOptions
			{
				AuthTokenAsyncFactory = () => Task.FromResult(authLink.FirebaseToken)
			});
		}
		public IActionResult Index()
		{
			client = new FireSharp.FirebaseClient(config);
			FirebaseResponse response = client.Get("Bookings");
			dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
			var list = new List<Booking>();

			if (data != null)
			{
				foreach (var item in data)
				{
					Booking booking = JsonConvert.DeserializeObject<Booking>(((JProperty)item).Value.ToString());

					// Check if the status is "Pending"
					if (booking != null && booking.status == "Pending")
					{
						list.Add(booking);
					}
				}
			}

			return View(list);

		}
		[HttpGet]
		public async Task<ActionResult> Approved(string id)
		{
			client = new FireSharp.FirebaseClient(config);
			FirebaseResponse response = client.Get("Bookings/" + id);
			Booking data = JsonConvert.DeserializeObject<Booking>(response.Body);
			data.status = "Approved";
			SetResponse Response = client.Set("Bookings/" + data.bookingId, data);
			return RedirectToAction("Index", "Booking");
		}
		[HttpGet]
		public async Task<ActionResult> Declined(string id)
		{
			client = new FireSharp.FirebaseClient(config);
			FirebaseResponse response = client.Get("Bookings/" + id);
			Booking data = JsonConvert.DeserializeObject<Booking>(response.Body);
			data.status = "Declined";
			SetResponse Response = client.Set("Bookings/" + data.bookingId, data);
			return RedirectToAction("Index", "Booking");
		}


	}
}
