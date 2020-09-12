using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using PrestaQi.Model;
using PrestaQi.Model.Dto.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace PrestaQi.Service.Tools
{
    public static class Utilities
    {
        public static string GetPasswordRandom()
        {
            Random rdn = new Random();
            string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890%$#@";
            int longitud = characters.Length;
            char letra;
            int passwordLength = 10;
            string passwordRandom = string.Empty;
            for (int i = 0; i < passwordLength; i++)
            {
                letra = characters[rdn.Next(longitud)];
                passwordRandom += letra.ToString();
            }

            return passwordRandom;
        }

        public static bool SendEmail(List<string> emails, MessageMail messageMail, Configuration configuration, FileMail file = null)
        {
            ConfigurationEmail configurationEmail = JsonConvert.DeserializeObject<ConfigurationEmail>(configuration.Configuration_Value);
            
            SmtpClient smtpClient = new SmtpClient(configurationEmail.Host, configurationEmail.Port)
            {
                EnableSsl = configurationEmail.EnableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(configurationEmail.User, configurationEmail.Password),
                Timeout = 2000000
            };

            var mail = new MailMessage(configurationEmail.User, string.Join(",", emails), messageMail.Subject, messageMail.Message);

            if (file != null)
                mail.Attachments.Add(new Attachment(file.File, file.FileName));

            mail.IsBodyHtml = true;
            smtpClient.Send(mail);

            mail.Dispose();
            smtpClient.Dispose();

            return true;
        }

        public static byte[] GetFile(List<Configuration> configurations, string endPointName)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(endPointName);

            request.KeepAlive = true;
            request.UsePassive = true;
            request.UseBinary = true;

            request.Method = WebRequestMethods.Ftp.DownloadFile;
            request.Credentials = new NetworkCredential(configurations.Find(p => p.Configuration_Name == "FTP_USER").Configuration_Value,
                configurations.Find(p => p.Configuration_Name == "FTP_PASSWORD").Configuration_Value);

            FtpWebResponse ftpWebResponse = (FtpWebResponse)request.GetResponse();

            Stream stream = ftpWebResponse.GetResponseStream();
            byte[] file;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                file = memoryStream.ToArray();
            }

            ftpWebResponse.Close();
            request = null;

            return file;
        }

        public static void GenerateEmptyCell(int num, List<Cell> cells)
        {
            for (int i = 0; i < num; i++)
            {
                cells.Add(new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(8)
                    .Add(new Paragraph(string.Empty)));
            }
        }

        public static void GenerateCell(string text, List<Cell> cells, int fontSize, TextAlignment alignment)
        {
            cells.Add(new Cell(1, 1)
                    .SetTextAlignment(alignment)
                    .SetFontSize(fontSize)
                    .Add(new Paragraph(text)));
        }

        public static (DateTime, DateTime, bool) CalcuateDates(string filter)
        {
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            bool today = false;

            switch (filter)
            {
                case "current-month":
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
                    break;
                case "today":
                    today = true;
                    break;
                case "current-biweekly":
                    if (DateTime.Now.Day > 15)
                    {
                        startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 16);
                        endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month));
                    }
                    else
                    {
                        startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15);
                    }
                    break;
                case "before-biweekly":
                    if (DateTime.Now.Day > 15)
                    {
                        startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 15);
                    }
                    else
                    {
                        startDate = startDate.AddMonths(-1);

                        startDate = new DateTime(startDate.Year, startDate.Month, 15);
                        endDate = new DateTime(startDate.Year, startDate.Month, DateTime.DaysInMonth(startDate.Year, startDate.Month)); 
                    }
                    break;
                case "current-week":
                    startDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday);
                    endDate = startDate.AddDays(5);//DateTime.Now.StartOfWeek(DayOfWeek.Saturday);
                    break;
                case "before-week":
                    startDate = DateTime.Now.StartOfWeek(DayOfWeek.Monday);

                    startDate = startDate.AddDays(-7);
                    endDate = startDate.AddDays(5);
                    break;
            }

            return (startDate, endDate, today);
        }

        public static Stream GenerateStreamFromString(string text)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", nameof(source));
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public static T CloneJson<T>(this T source)
        {
            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(source), deserializeSettings);
        }

        public static double RoundUpValue(double value, int decimalpoint)
        {
            var result = Math.Round(value, decimalpoint);
            if (result < value)
            {
                result += Math.Pow(10, -decimalpoint);
            }
            return result;
        }
    }
}
