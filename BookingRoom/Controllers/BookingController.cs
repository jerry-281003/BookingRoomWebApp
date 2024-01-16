using BookingRoom.Models;
using Firebase.Auth;
using Firebase.Storage;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using FirebaseAdmin.Messaging;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using static Google.Apis.Requests.BatchRequest;
using Microsoft.VisualBasic;
using Microsoft.AspNetCore.Http;
using System.Globalization;

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

			
			if (FirebaseApp.DefaultInstance == null)
			{
				FirebaseApp.Create(new AppOptions()
				{
					Credential = GoogleCredential.FromFile("wwwroot/booking-room-app-f6938-firebase-adminsdk-9dz98-d7d26db137.json")
				});
			}
			else
			{
				// Use the existing app
				var app = FirebaseApp.GetInstance;
			}
		}
		public IActionResult Index()
		{
			client = new FireSharp.FirebaseClient(config);
			FirebaseResponse response = client.Get("Bookings/UEF");
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
			FirebaseResponse response = client.Get("Bookings/UEF/" + id);
			Booking booking = JsonConvert.DeserializeObject<Booking>(response.Body);
			booking.status = "Approved";
			SetResponse Response = client.Set("Bookings/" + booking.bookingId, booking);

			PushNotify(booking.userName,true);
			return RedirectToAction("Index", "Booking");
		}
		[HttpGet]
		public async Task<ActionResult> Declined(string id)
		{
			client = new FireSharp.FirebaseClient(config);
			FirebaseResponse response = client.Get("Bookings/UEF/" + id);
			Booking booking = JsonConvert.DeserializeObject<Booking>(response.Body);
			booking.status = "Declined";
			SetResponse Response = client.Set("Bookings/" + booking.bookingId, booking);
			PushNotify(booking.userName, false);
			return RedirectToAction("Index", "Booking");
		}

		public void PushNotify(String username,bool status)
		{
			FirebaseResponse response = client.Get("users/" + username);
			Models.User user = JsonConvert.DeserializeObject<Models.User>(response.Body);
			// Send push notification
			if (status == true)
			{
				var messaging = FirebaseMessaging.DefaultInstance;
				var message = new Message()
				{

					Token = user.fcmToken,
					Notification = new Notification()
					{
						Title = "Booking Approved",
						Body = "A new booking has been approved."
					}
				};

				messaging.SendAsync(message);
			}
			else
			{
				var messaging = FirebaseMessaging.DefaultInstance;
				var message = new Message()
				{

					Token = user.fcmToken,
					Notification = new Notification()
					{
						Title = "Booking Declined",
						Body = "A new booking has been declined."
					}
				};

				messaging.SendAsync(message);
			}
			
		}
		public IActionResult Chart()
		{
			//get room
			client = new FireSharp.FirebaseClient(config);
			FirebaseResponse response = client.Get("Rooms");
			dynamic room = JsonConvert.DeserializeObject<dynamic>(response.Body);
			var list = new List<RoomStatistical>();
			if (room != null)
			{
				foreach (var item in room)
				{
					list.Add(JsonConvert.DeserializeObject<RoomStatistical>(((JProperty)item).Value.ToString()));				
				}
				foreach (RoomStatistical roomstatistical in list)
				{
					roomstatistical.numBookingpPerYear = CountBooking("year",roomstatistical.RoomName);
					roomstatistical.numBookingpPerMonth = CountBooking("month", roomstatistical.RoomName);
				}
			}
			
			

			return View(list);
		}
		public int CountBooking(String type, String roomName)
		{
			// get booking
			
			FirebaseResponse response = client.Get("Bookings/UEF");
			dynamic booking = JsonConvert.DeserializeObject<dynamic>(response.Body);
			
			int count = 0;
			if (booking != null)
			{
				foreach (var item in booking)
				{
					Booking booking1 = JsonConvert.DeserializeObject<Booking>(((JProperty)item).Value.ToString());
					string format = "HH:mm dd/MM/yyyy";				
					DateTime datetime = DateTime.ParseExact(booking1.date, format, CultureInfo.InvariantCulture);
					
					
					if (booking1 != null && booking1.status == "Approved" && booking1.roomName==roomName && datetime.Year==2024 && type== "year" )
					{					
						count++;
					}
					if (booking1 != null && booking1.status == "Approved" && booking1.roomName == roomName && datetime.Month == 1 && type == "month")
					{
						count++;
					}
					return count;
				}
			}
			return count;
		}
	}
}
