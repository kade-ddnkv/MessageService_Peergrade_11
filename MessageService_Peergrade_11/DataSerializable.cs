using MessageService_Peergrade_11.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MessageService_Peergrade_11
{
    /// <summary>
    /// Класс-болванка для сериализации статического класса Data.
    /// </summary>
    public class DataSerializable
    {
        public List<UserInfo> Users { get; set; }
        public List<MessageInfo> Messages { get; set; }
    }
}
