using MessageService_Peergrade_11.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MessageService_Peergrade_11.Controllers
{
    /// <summary>
    /// Контроллер, отвечающий за работу с пользователями (создание, просмотр).
    /// </summary>
    public class UserController : Controller
    {
        private static List<UserInfo> _users = Data.Users;

        const string pathToNamesDictionary = "Randomizing/NamesDictionary.txt";

        /// <summary>
        /// Создать пользователя.
        /// </summary>
        /// <param name="req">Модель пользователя</param>
        /// <returns></returns>
        [HttpPost("create-user")]
        public IActionResult CreateUser([FromBody] CreateUserRequest req)
        {
            // Не может быть двух пользователей с одинаковым почтовым адресом.
            if (_users.Any(u => u.Email == req.Email))
            {
                return BadRequest(new
                {
                    Message = $"User with Email = {req.Email} already exists."
                });
            }

            var user = new UserInfo()
            {
                // Так как юзеров нельзя удалять, можно сделать простое определение Id.
                Id = _users.Count + 1,
                Email = req.Email,
                UserName = req.UserName
            };
            
            _users.Add(user);
            SortUsersByEmail();
            return Ok(user);
        }

        /// <summary>
        /// Создать случайных пользователей (предыдущие удаляются).
        /// </summary>
        /// <param name="count">Кол-во пользователей</param>
        /// <returns></returns>
        [HttpPost("create-random-users")]
        public IActionResult CreateRandomUsers([FromQuery] int count)
        {
            // Сначала загружается список имен из локальных файлов.
            string[] randomNames;
            try
            {
                randomNames = LoadNamesDictionary();
            }
            catch (IOException)
            {
                return BadRequest(new
                {
                    Message = $"Service unaviable due to local files corruption. Check this path: APP_LOCATION/{pathToNamesDictionary}"
                });
            }

            _users.Clear();
            Random rand = new Random();

            if (count < 0 || count > randomNames.Length)
            {
                return BadRequest(new
                {
                    Message = $"Number of users should be >= 0 and <= {randomNames.Length}."
                });
            }

            // Массив имен перемешивается, берутся первые count имен.
            randomNames = randomNames.OrderBy(n => rand.Next()).Take(count).ToArray();

            for (int i = 0; i < count; i++)
            {
                var user = new UserInfo()
                {
                    UserName = randomNames[i],
                    Email = randomNames[i] + "@hse.ru",
                    Id = _users.Count + 1
                };
                _users.Add(user);
            }
            return Ok(_users);
        }

        /// <summary>
        /// Загрузить из файла pathToNamesDictionary список имен.
        /// </summary>
        /// <exception cref="IOException"></exception>
        /// <returns></returns>
        [NonAction]
        private string[] LoadNamesDictionary()
        {
            using (StreamReader sr = new StreamReader(pathToNamesDictionary))
            {
                return sr.ReadToEnd().Split(new string[] { " ", ",", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// Найти пользователя по почте.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("get-user-by-email")]
        public IActionResult GetUserByEmail([FromQuery] string email)
        {
            var result = _users.Where(u => u.Email == email).ToList();
            if (result.Count == 0)
            {
                return NotFound(new
                {
                    Message = $"User with Email = {email} was not found."
                });
            }
            return Ok(result.First());
        }

        /// <summary>
        /// Получить список всех пользователей.
        /// </summary>
        /// <returns></returns>
        [HttpGet("get-all-users")]
        public IActionResult GetAllUsers()
        {
            return Ok(_users);
        }

        /// <summary>
        /// Получить список всех пользователей для постраничной выборки.
        /// </summary>
        /// <param name="limit"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        [HttpGet("get-users-with-offset-and-limit")]
        public IActionResult GetAllUsers([FromQuery] int limit, [FromQuery] int offset)
        {
            if (offset < 0)
            {
                return BadRequest(new
                {
                    Message = "Offset should be >= 0"
                });
            }
            if (limit <= 0)
            {
                return BadRequest(new
                {
                    Message = "Limit of users to show should be > 0"
                });
            }
            return Ok(_users.Skip(offset).Take(limit));
        }

        /// <summary>
        /// Получить id пользователя по его почте. Вернет null, если подходящего пользователя нет.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [NonAction]
        public static int? GetUserIdByEmail(string email)
        {
            return _users.Find(u => u.Email == email)?.Id;
        }

        /// <summary>
        /// Сортировать пользователей по почте лексикографически.
        /// </summary>
        [NonAction]
        public static void SortUsersByEmail()
        {
            _users = _users.OrderBy(u => u.Email).ToList();
        }
    }
}
