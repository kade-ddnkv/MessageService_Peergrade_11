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
    /// Контроллер, отвечающий за работу с сообщениями (создание, просмотр).
    /// </summary>
    public class MessageController : Controller
    {
        // Перменные-ссылки на общие данные.
        private static readonly List<UserInfo> _users = Data.Users;
        private static readonly List<MessageInfo> _messages = Data.Messages;

        const string pathToSubjectsDictionary = "Randomizing/SubjectsDictionary.txt";
        const int maxNumberOfWordsInMessage = 8;

        /// <summary>
        /// Создать новое сообщение.
        /// </summary>
        /// <param name="req">Модель сообщения</param>
        /// <returns></returns>
        [HttpPost("create-message")]
        public IActionResult CreateMessage([FromBody] CreateMessageRequest req)
        {
            int? senderId = UserController.GetUserIdByEmail(req.SenderEmail);
            int? receiverId = UserController.GetUserIdByEmail(req.ReceiverEmail);

            if (senderId is null)
            {
                return BadRequest(new
                {
                    Message = $"No user with Email = {req.SenderEmail}."
                });
            }
            if (receiverId is null)
            {
                return BadRequest(new
                {
                    Message = $"No user with Email = {req.ReceiverEmail}."
                });
            }

            var message = new MessageInfo()
            {
                Subject = req.Subject,
                Message = req.Message,
                SenderId = senderId.Value,
                ReceiverId = receiverId.Value
            };

            _messages.Add(message);
            return Ok(message);
        }

        /// <summary>
        /// Создать случайные сообщения (предыдущие удаляются).
        /// </summary>
        /// <param name="count">Кол-во сообщений</param>
        /// <returns></returns>
        [HttpPost("create-random-messages")]
        public IActionResult CreateRandomMessages([FromQuery] int count)
        {
            // Сначала загружается список слов из локальных файлов.
            string[] randomSubjects;
            try
            {
                randomSubjects = LoadSubjectsDictionary();
                if (randomSubjects.Length < maxNumberOfWordsInMessage)
                {
                    throw new IOException();
                }
            }
            catch (IOException)
            {
                return BadRequest(new
                {
                    Message = $"Service unaviable due to local files corruption. Check this path: APP_LOCATION/{pathToSubjectsDictionary}"
                });
            }

            _messages.Clear();
            Random rand = new Random();

            if (count < 0 || count > randomSubjects.Length)
            {
                return BadRequest(new
                {
                    Message = $"Number of messages should be >= 0 and {randomSubjects.Length}."
                });
            }

            if (_users.Count == 0)
            {
                return BadRequest(new
                {
                    Message = "No users to post messages."
                });
            }

            for (int i = 0; i < count; i++)
            {
                // Каждый раз список слов перемешивается.
                randomSubjects = randomSubjects.OrderBy(n => rand.Next()).ToArray();
                var message = new MessageInfo()
                {
                    Subject = randomSubjects[0],
                    Message = string.Join(' ', randomSubjects.Take(rand.Next(1, maxNumberOfWordsInMessage))),
                    SenderId = _users[rand.Next(_users.Count)].Id,
                    ReceiverId = _users[rand.Next(_users.Count)].Id
                };
                _messages.Add(message);
            }

            return Ok(_messages);
        }

        /// <summary>
        /// Возвращает список тем из файла pathToSubjectsDictionary.
        /// </summary>
        /// <returns></returns>
        [NonAction]
        public string[] LoadSubjectsDictionary()
        {
            using (StreamReader sr = new StreamReader(pathToSubjectsDictionary))
            {
                return sr.ReadToEnd().Split(new string[] { " ", ",", Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        /// <summary>
        /// Получить сообщения по почте отправителя.
        /// </summary>
        /// <param name="senderEmail"></param>
        /// <returns></returns>
        [HttpGet("get-messages-by-sender-email")]
        public IActionResult GetMessagesBySenderEmail([FromQuery] string senderEmail)
        {
            int? senderId = UserController.GetUserIdByEmail(senderEmail);
            return Ok(_messages.Where(m => m.SenderId == senderId));
        }

        /// <summary>
        /// Получить сообщения по почте получателя.
        /// </summary>
        /// <param name="receiverEmail"></param>
        /// <returns></returns>
        [HttpGet("get-messages-by-receiver-email")]
        public IActionResult GetMessagesByReceiverEmail([FromQuery] string receiverEmail)
        {
            int? receiverId = UserController.GetUserIdByEmail(receiverEmail);
            return Ok(_messages.Where(m => m.ReceiverId == receiverId));
        }

        /// <summary>
        /// Получить сообщения по почтам отправителя и получателя.
        /// </summary>
        /// <param name="senderEmail"></param>
        /// <param name="receiverEmail"></param>
        /// <returns></returns>
        [HttpGet("get-messages-by-sender-and-reciever-emails")]
        public IActionResult GetMessagesBySenderAndReceiverEmails([FromQuery] string senderEmail, [FromQuery] string receiverEmail)
        {
            int? senderId = UserController.GetUserIdByEmail(senderEmail);
            int? receiverId = UserController.GetUserIdByEmail(receiverEmail);
            return Ok(_messages.Where(m => m.SenderId == senderId && m.ReceiverId == receiverId));
        }
    }
}
