using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using NuGet.Protocol;
using Students.Data;
using Students.Models;

namespace Students.Controllers
{
    public class JsonData
    {
        public string Data { get; set; }
    }

    public class IdentityUserExtended : IdentityUser
    {
        public string Json { get; set; }
    }

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ApplicationDbContext _dbContext;
        public IConfiguration Configuration { get; }

        public HomeController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ILogger<HomeController> logger, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> Authenticated()
        {
            return Ok(new
            {
                User.Identity.IsAuthenticated,
                Username = User.Identity.IsAuthenticated ? User.Identity.Name : string.Empty,
                AuthenticationMethod = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.AuthenticationMethod)?.Value,
                DisplaySetPassword = User.Identity.IsAuthenticated
                                     && !(await _userManager.HasPasswordAsync(
                                         (await _userManager.GetUserAsync(User))
                                         ))
            });
        }

        [HttpGet]
        public async Task<ActionResult> GetJsonOfUser()
        {
            IdentityUser user = await _userManager.GetUserAsync(User);
            string jsonReturn = String.Empty;

            using (var connection = new MySqlConnection("Server=localhost;Database=MFT_Students;Uid=root;Pwd=Pune2024$;"))
            {
                connection.Open();
                using (var command = new MySqlCommand("SELECT Json FROM AspNetUsers WHERE Id = @Id", connection))
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Id", user.Id);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (!reader.IsDBNull(0))
                                {
                                    jsonReturn = reader.GetString(0);                                 
                                }
                            }
                        }                       
                    }
                }
            }

            return Json(jsonReturn);
        }

        [HttpPost]
        public async Task<ActionResult> SubmitDetails([FromBody] JsonObject myJSON)
        {
            var plainJson = JsonSerializer.Serialize(myJSON);
            plainJson.Replace('\"', '"');

            IdentityUser user = await _userManager.GetUserAsync(User);

            using (var connection = new MySqlConnection("Server=localhost;Database=MFT_Students;Uid=root;Pwd=Pune2024$;"))
            {
                connection.Open();
                using(var command = new MySqlCommand("UPDATE AspNetUsers SET Json=@Json WHERE Id = @Id", connection))
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@Json", plainJson);
                    command.ExecuteNonQuery();
                }
            }

            return Json("Ok");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
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
