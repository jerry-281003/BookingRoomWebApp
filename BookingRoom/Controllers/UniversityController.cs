using Firebase.Auth;
using Firebase.Storage;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Mvc;
using BookingRoom.Models;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection;

namespace BookingRoom.Controllers
{
	public class UniversityController : Controller
	{

		IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
		{
			AuthSecret = "K1cf1DhpkqxvOkAeqgLElTfzWvDwy09UuRzLrZXU",
			BasePath = "https://booking-room-app-f6938-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
		IFirebaseClient client;
		private readonly FirebaseStorage _firebaseStorage;
		public UniversityController()
		{

			var auth = new FirebaseAuthProvider(new Firebase.Auth.FirebaseConfig("AIzaSyA_tQLMK2xQX0qLO--ZMTYvDTb7OikpqrQ"));
			var authLink = auth.SignInWithEmailAndPasswordAsync("thanhdn21@gmail.com", "thanhpro123").Result;
			_firebaseStorage = new FirebaseStorage(
            "booking-room-app-f6938.appspot.com",
			new FirebaseStorageOptions
			{
				AuthTokenAsyncFactory = () => Task.FromResult(authLink.FirebaseToken)
			});
		}
		public IActionResult Index()
		{
			client = new FireSharp.FirebaseClient(config);
			FirebaseResponse response = client.Get("Universitys");
			dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
			var list = new List<University>();
			if (data != null)
			{
				foreach (var item in data)
				{
					list.Add(JsonConvert.DeserializeObject<University>(((JProperty)item).Value.ToString()));
				}
			}
			return View(list);
		}
		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}
		[HttpPost]
		public async Task<IActionResult> CreateAsync(University university, IFormFile image)
        {
			try
			{
                if (image != null)
                {
                    var storagePath = "University_logo/" + university.UniversityName;
                    // Upload the image to Firebase Storage
                    var imageUrl = await _firebaseStorage
                    .Child(storagePath)
                            .PutAsync(image.OpenReadStream());
                    university.LogoPath = university.UniversityName + ".png";
					university.LogoUrl = imageUrl;

                    var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/University_logo");
                    var fileName = university.UniversityName+".png";
                    var filePath = Path.Combine(uploadsDirectory, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }
                }
                client = new FireSharp.FirebaseClient(config);
                var data = university;
                PushResponse response = client.Push("Universitys/", data);
                data.UniversityId = response.Result.name;
                SetResponse setResponse = client.Set("Universitys/" + data.UniversityId, data);
               
               

				if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
				{
					return RedirectToAction("Index", "University"); // Redirect to the student list or another appropriate action
				}
				else
				{
					ModelState.AddModelError(string.Empty, "Something went wrong!!");
				}
			}
			catch (Exception ex)
			{
				// Log the exception for debugging
				Console.WriteLine(ex);

				ModelState.AddModelError(string.Empty, "An error occurred while saving the data.");
			}


			return View(university);
		}
        [HttpGet]
        public ActionResult Edit(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Universitys/" + id);
            University data = JsonConvert.DeserializeObject<University>(response.Body);
            return View(data);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(University university, IFormFile image)
        {
            client = new FireSharp.FirebaseClient(config);
            if (image != null)
            {
                //delete file
                await _firebaseStorage.Child("University_logo").Child(university.UniversityName).DeleteAsync();
                var FilePath = Path.Combine("wwwroot/University_logo", university.LogoPath);
                if (System.IO.File.Exists(FilePath))
                {      
                    System.IO.File.Delete(FilePath);
                    ViewBag.Message = "File deleted successfully!";
                }

                var storagePath = "University_logo/" + university.UniversityName;
                // Upload the image to Firebase Storage
                var imageUrl = await _firebaseStorage
                .Child(storagePath)
                        .PutAsync(image.OpenReadStream());
                university.LogoPath = university.UniversityName + ".png";
                university.LogoUrl = imageUrl;

                var uploadsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/University_logo");
                var fileName = university.UniversityName + ".png";
                var filePath = Path.Combine(uploadsDirectory, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                SetResponse response = client.Set("Universitys/" + university.UniversityId, university);
            }
            else
            {
                SetResponse response = client.Set("Universitys/" + university.UniversityId + "/UniversityName", university.UniversityName);
            }
               
            return RedirectToAction("Index", "University");
        }
        public async Task<ActionResult> Delete(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse getResponse = client.Get("Universitys/" + id);
            var university = JsonConvert.DeserializeObject<University>(getResponse.Body);
            if (getResponse != null && getResponse.Body != "null")
            {
                FirebaseResponse deleteResponse = client.Delete("Universitys/" + id);
                await _firebaseStorage.Child("University_logo").Child(university.UniversityName).DeleteAsync();
            }
            var filePath = Path.Combine("wwwroot/University_logo", university.LogoPath);

            // Check if the file exists before attempting to delete
            if (System.IO.File.Exists(filePath))
            {
                // Delete the file
                System.IO.File.Delete(filePath);

                ViewBag.Message = "File deleted successfully!";
            }
            return RedirectToAction("Index", "University");
        }
    }

}
