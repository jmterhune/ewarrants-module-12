using Azure.Communication.Sms;
using System;
using System.Collections.Generic;
using System.Configuration;


namespace tjc.Modules.eWarrants.Components
{
    public class MicrosoftSmsSender
    {
        private readonly SmsClient _smsClient;

        /// <summary>
        /// Initializes a new instance of the MicrosoftSmsSender class.
        /// </summary>
        /// <param name="connectionString">The Azure Communication Services connection string.</param>
       public MicrosoftSmsSender()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["AzureSmsConnection"].ConnectionString; 
            _smsClient = new SmsClient(connectionString);
        }

        /// <summary>
        /// Sends an SMS message to a single recipient.
        /// </summary>
        /// <param name="fromPhoneNumber">The sender's phone number in E.164 format (e.g., +14255550123).</param>
        /// <param name="toPhoneNumber">The recipient's phone number in E.164 format.</param>
        /// <param name="message">The message content.</param>
        /// <returns>The SMS send result.</returns>
        public SmsSendResult SendSms(string toPhoneNumber, string message)
        {
            return _smsClient.Send(
                from: "18666227990",
                to: toPhoneNumber,
                message: message
            );
        }

        /// <summary>
        /// Sends an SMS message to multiple recipients with options.
        /// </summary>
        /// <param name="fromPhoneNumber">The sender's phone number in E.164 format.</param>
        /// <param name="toPhoneNumbers">A list of recipient phone numbers in E.164 format.</param>
        /// <param name="message">The message content.</param>
        /// <param name="enableDeliveryReport">Whether to enable delivery reports (default: false).</param>
        /// <param name="tag">Optional custom tag for the message.</param>
        /// <returns>A list of SMS send results.</returns>
        public IReadOnlyList<SmsSendResult> SendSmsToMultiple( List<string> toPhoneNumbers, string message, bool enableDeliveryReport = false, string tag = null)
        {
            var options = new SmsSendOptions(enableDeliveryReport)
            {
                Tag = tag
            };

            var response = _smsClient.Send(
                from: "18666227990",
                to: toPhoneNumbers,
                message: message,
                options: options
            );

            return response.Value;
        }
    }
}