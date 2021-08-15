using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageService_Peergrade_11.Models
{
    /// <summary>
    /// Модель для создания пользователя.
    /// </summary>
    public class CreateUserRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
