using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Atalasoft.Imaging.Codec;
using DotNetNuke.Entities.Users;
using Newtonsoft.Json;

namespace tjc.Modules.eWarrants.Components
{
    public static class Helper
    {
        public static string EnumDisplayNameFor(this Enum item)
        {
            var type = item.GetType();
            var member = type.GetMember(item.ToString());
            DisplayAttribute displayName = (DisplayAttribute)member[0].GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault();
            if (displayName != null)
                return displayName.Name.ToString();

            return item.ToString();
        }

        public static void AddPdfDecoder()
        {
            bool pdfRegistered = false;

            foreach (ImageDecoder decoder in RegisteredDecoders.Decoders)
            {
                var type = decoder.GetType();
                if (type.Name.ToLower() == "pdfdecoder")
                    pdfRegistered = true;
            }
            if (!pdfRegistered)
                RegisteredDecoders.Decoders.Add(new Atalasoft.Imaging.Codec.Pdf.PdfDecoder());
        }
        public static string GetProfileValue(UserInfo user, string propertyName)
        {
            string value = string.Empty;
            var ppd = user.Profile.GetProperty(propertyName);
            if (ppd != null)
            {
                value = ppd.PropertyValue ?? (string.IsNullOrEmpty(ppd.DefaultValue) ? string.Empty : ppd.DefaultValue);
            }
            return value;
        }

        public static string GetPhoneNumbersJson(UserInfo user)
        {
            string phoneNumbers = user.Profile.GetPropertyValue("PhoneNumber");
            if (string.IsNullOrEmpty(phoneNumbers))
                return "[]";
            var numbers = phoneNumbers.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(n => n.Trim())
                                     .Where(n => !string.IsNullOrEmpty(n))
                                     .ToList();
            return JsonConvert.SerializeObject(numbers);
        }

        public static async void SendSmsNotifications(UserInfo user, string apiKey, string fromPhoneNumber, string subject, string body)
        {
            if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(fromPhoneNumber))
                return;

            string phoneNumbersJson = GetPhoneNumbersJson(user);
            var phoneNumbers = JsonConvert.DeserializeObject<List<string>>(phoneNumbersJson);
            if (phoneNumbers != null && phoneNumbers.Count > 0)
            {
                var smsService = new TelnyxSmsService(apiKey, fromPhoneNumber);
                foreach (var number in phoneNumbers)
                {
                    await smsService.SendSmsAsync(number, body);
                }
            }
        }
    }
}