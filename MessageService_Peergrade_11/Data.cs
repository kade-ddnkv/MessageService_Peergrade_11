using MessageService_Peergrade_11.Controllers;
using MessageService_Peergrade_11.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MessageService_Peergrade_11
{
    /// <summary>
    /// Класс общих данных, хранящий список пользователей и сообщений.
    /// </summary>
    public class Data
    {
        public static List<UserInfo> Users = new List<UserInfo>();
        public static List<MessageInfo> Messages = new List<MessageInfo>();

        /// <summary>
        /// Возвращает экземпляр DataSerializable для сериализации.
        /// </summary>
        /// <returns></returns>
        public static DataSerializable GetSerializableData()
        {
            return new DataSerializable() { Users = Data.Users, Messages = Data.Messages };
        }

        /// <summary>
        /// Загружает в статический класс Data пользователей и сообщения из переданного класса.
        /// </summary>
        /// <param name="data"></param>
        public static void LoadFromSerializableData(DataSerializable data)
        {
            Users.Clear();
            Users.AddRange(data.Users);
            Messages.Clear();
            Messages.AddRange(data.Messages);
        }

        /// <summary>
        /// Проверяет корректность текущих данных. Может выбросить InvalidOperationException.
        /// </summary>
        public static void CheckDataCorrectness()
        {
            // Не может быть двух пользователей с одним id.
            // Не может быть сообщений из ниоткуда или в никуда.

            var moreIdGroups = Users.GroupBy(u => u.Id).Where(g => g.Count() > 1);
            if (moreIdGroups.Count() > 0)
            {
                throw new InvalidOperationException("Wrong data: two users with one id.");
            }
            
            foreach (MessageInfo m in Messages)
            {
                if (Users.Find(u => u.Id == m.SenderId) == null
                    || Users.Find(u => u.Id == m.ReceiverId) == null)
                {
                    throw new InvalidOperationException("Wrong data: message have an id that doesn't match any user.");
                }
            }

            UserController.SortUsersByEmail();
        }
    }
}
