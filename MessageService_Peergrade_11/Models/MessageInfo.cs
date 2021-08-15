using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageService_Peergrade_11.Models
{
    /// <summary>
    /// Класс - сообщение.
    /// </summary>
    public class MessageInfo
    {
        public string Subject { get; set; }
        public string Message { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
    }
}
