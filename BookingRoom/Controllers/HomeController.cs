using BookingRoom.Models;
using Firebase.Auth;
using Firebase.Storage;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using FireSharp.Interfaces;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BookingRoom.Controllers
{
	public class HomeController : Controller
	{
		IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
		{
			AuthSecret = "K1cf1DhpkqxvOkAeqgLElTfzWvDwy09UuRzLrZXU",
			BasePath = "https://booking-room-app-f6938-default-rtdb.asia-southeast1.firebasedatabase.app/"
		};
		IFirebaseClient client;
		private readonly FirebaseStorage _firebaseStorage;
		
		public HomeController()
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

		
		public IActionResult Index(string name,string university,string email,string password)
		{
			CurrentUser.name = email;

		
			return View();
		}

		public IActionResult Privacy()
		{
			return View();
		}
		public IActionResult Login()
		{
			return View();
		}
		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}

		

	}
}