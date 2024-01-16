using BookingRoom.Models;
using Firebase.Auth;
using Firebase.Storage;
using FirebaseAdmin;
using FireSharp.Interfaces;
using FireSharp.Response;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using User = BookingRoom.Models.User;

namespace BookingRoom.Controllers
{
	public class UserController : Controller
	{
		IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
		{
			AuthSecret = "K1cf1DhpkqxvOkAeqgLElTfzWvDwy09UuRzLrZXU",
			BasePath = "https://booking-room-app-f6938-default-rtdb.asia-southeast1.firebasedatabase.app/"
		};
		IFirebaseClient client;
		private readonly FirebaseStorage _firebaseStorage;
		public UserController()
		{

			var auth = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("AIzaSyAI67BIPvZDS1mf-dQdSNxp83XmCTlxanI"));
			var authLink = auth.SignInWithEmailAndPasswordAsync("thanhdn21@gmail.com", "thanhpro123").Result;
			_firebaseStorage = new FirebaseStorage(
			"booking-room-app-f6938.firebaseapp.com",
			new FirebaseStorageOptions
			{
				AuthTokenAsyncFactory = () => Task.FromResult(authLink.FirebaseToken),
				ThrowOnCancel = true
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
			return View();
		}
		public async Task<IActionResult> DetailsAsync(string id)
		{
			client = new FireSharp.FirebaseClient(config);
			FirebaseResponse response = client.Get("users/" + id);
			User data = JsonConvert.DeserializeObject<User>(response.Body);

			var storage = new FirebaseStorage("booking-room-app-f6938.appspot.com")
				.Child("profile_pic");

			var downloadUrl = await storage.Child(data.userName).GetDownloadUrlAsync();
			data.url = downloadUrl;

			return View(data);
		}




	}
}
