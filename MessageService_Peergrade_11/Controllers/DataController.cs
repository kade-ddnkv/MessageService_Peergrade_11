using MessageService_Peergrade_11.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace MessageService_Peergrade_11.Controllers
{
    /// <summary>
    /// Контроллер, отвечающий за сохранение/загрузку данных.
    /// </summary>
    public class DataController : Controller
    {
        /// <summary>
        /// Загрузить данные из файла.
        /// </summary>
        /// <param name="fileName">Имя json-файла.</param>
        /// <returns></returns>
        [HttpGet("load-data")]
        public IActionResult LoadData([FromQuery] string fileName)
        {
            try
            {
                if (!FileNameIsValid(fileName, true))
                {
                    return BadRequest(new
                    {
                        Message = $"FileName is not valid." +
                        $" File should: be in the same directory with the app;" +
                        $" have extension .json"
                    });
                }
                // Тут десериализация.
                using (StreamReader sr = new StreamReader(fileName))
                {
                    string json = sr.ReadToEnd();
                    var deserializedObj = JsonSerializer.Deserialize<DataSerializable>(json);
                    Data.LoadFromSerializableData(deserializedObj);
                    Data.CheckDataCorrectness();
                }
            }
            catch (InvalidOperationException)
            {
                // InvalidOperationException может выбросится из Data.CheckDataCorrectness().
                // Некорректные данные очищаются.
                Data.Users.Clear();
                Data.Messages.Clear();
                return BadRequest(new
                {
                    Message = $"A loaded data was corrupted. Please check nothing was changed in datafile."
                });
            }
            catch (Exception)
            {
                Data.Users.Clear();
                Data.Messages.Clear();
                return BadRequest(new
                {
                    Message = $"An error occured while loading data." +
                    $" File should: be in the same directory with the app;" +
                    $" have extension .json"
                });
            }

            return Ok(new { Users = Data.Users, Messages = Data.Messages });
        }

        /// <summary>
        /// Сохранить данные в файл.
        /// </summary>
        /// <param name="fileName">Имя json-файла.</param>
        /// <returns></returns>
        [HttpGet("save-data")]
        public IActionResult SaveData([FromQuery] string fileName)
        {
            string serializedData;
            try
            {
                if (!FileNameIsValid(fileName, false))
                {
                    return BadRequest(new
                    {
                        Message = "FileName should: not have any subdirectories;" +
                        " have extension .json"
                    });
                }
                using (StreamWriter sw = new StreamWriter(fileName))
                {
                    serializedData = JsonSerializer.Serialize(Data.GetSerializableData());
                    sw.WriteLine(serializedData);
                }
            }
            catch (Exception)
            {
                return BadRequest(new
                {
                    Message = $"An error occured while saving data. Try to change fileName."
                });
            }

            return Ok(serializedData);
        }

        /// <summary>
        /// Определяет корректность имени файла. 
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="IOException"></exception>
        /// <exception cref="Exception"></exception>
        /// <param name="fileName"></param>
        /// <param name="alreadyExists"></param>
        /// <returns></returns>
        [NonAction]
        public bool FileNameIsValid(string fileName, bool alreadyExists)
        {
            FileInfo fi = new FileInfo(fileName);
            // Файл должен лежать в текущей директории.
            // И иметь расширение .json

            bool result = fi.Extension == ".json";
            if (alreadyExists)
            {
                result = result 
                    && fi.Exists 
                    && fi.Directory.FullName == Directory.GetCurrentDirectory();
            }
            else
            {
                result = result 
                    && fileName.Split(new string[] { "/", "\\" }, StringSplitOptions.None).Length == 1;
            }
            return result;
        }
    }
}
