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
	public class RoomController : Controller
	{
        IFirebaseConfig config = new FireSharp.Config.FirebaseConfig
        {
            AuthSecret = "K1cf1DhpkqxvOkAeqgLElTfzWvDwy09UuRzLrZXU",
            BasePath = "https://booking-room-app-f6938-default-rtdb.asia-southeast1.firebasedatabase.app/"
        };
        IFirebaseClient client;
        private readonly FirebaseStorage _firebaseStorage;
        public RoomController()
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
            FirebaseResponse response = client.Get("Rooms");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<Room>();
            if (data != null)
            {
                foreach (var item in data)
                {
                    list.Add(JsonConvert.DeserializeObject<Room>(((JProperty)item).Value.ToString()));
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
        public async Task<IActionResult> CreateAsync(Room room)
        {
            try
            {
                client = new FireSharp.FirebaseClient(config);
                var data = room;
                PushResponse response = client.Push("Rooms/", data);
                data.RoomId = response.Result.name;
                SetResponse setResponse = client.Set("Rooms/" + data.RoomId, data);

                if (setResponse.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return RedirectToAction("Index", "Room"); // Redirect to the student list or another appropriate action
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
            return View(room);
        }

        [HttpGet]
        public ActionResult Edit(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Rooms/" + id);
            Room data = JsonConvert.DeserializeObject<Room>(response.Body);
            return View(data);
        }

        [HttpPost]
        public async Task<ActionResult> Edit(Room room)
        {
            client = new FireSharp.FirebaseClient(config);
            SetResponse response = client.Set("Rooms/" + room.RoomId, room);
            return RedirectToAction("Index", "Room");
        }
        public async Task<ActionResult> Delete(string id)
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse getResponse = client.Get("Rooms/" + id);
            if (getResponse != null && getResponse.Body != "null")
            {
                FirebaseResponse deleteResponse = client.Delete("Rooms/" + id);
            }
            return RedirectToAction("Index", "Room");
        }

    }
}
