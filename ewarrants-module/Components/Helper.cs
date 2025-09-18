using Atalasoft.Imaging.Codec;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Mail;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;

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
            string phoneNumbers = user.Profile.GetPropertyValue("SMSPhoneNumbers");
            if (string.IsNullOrEmpty(phoneNumbers))
                return "[]";
            var numbers = phoneNumbers.Split(new[] { ',' }).Where(n => !string.IsNullOrEmpty(n.Trim())).ToList();
            return JsonConvert.SerializeObject(numbers);
        }

        public static bool SendSmsNotifications(UserInfo user, string subject, string body)
        {

            try
            {
                string phoneNumbersJson = GetPhoneNumbersJson(user);
                var phoneNumbers = JsonConvert.DeserializeObject<List<string>>(phoneNumbersJson);
                if (phoneNumbers != null && phoneNumbers.Count > 0)
                {

                    var smsService = new Components.MicrosoftSmsSender();
                    smsService.SendSmsToMultiple(phoneNumbers, subject + " - " + body);
                }
                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return false;
            }
        }
        public static void SendToAlternateContact(string replyEmail, string contactPoint, string subject, string body)
        {
            if (!string.IsNullOrEmpty(contactPoint) && contactPoint.Contains("@"))
                Mail.SendEmail(replyEmail, contactPoint, subject, body);
        }
    }
}