using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageService_Peergrade_11.Models
{
    /// <summary>
    /// Модель для создания сообщения.
    /// </summary>
    public class CreateMessageRequest
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public string SenderEmail { get; set; }
        public string ReceiverEmail { get; set; }
    }
}
