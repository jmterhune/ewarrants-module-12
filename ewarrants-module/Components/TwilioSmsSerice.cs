using System;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace tjc.Modules.eWarrants.Components
{
    public class TwilioSmsService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _fromPhoneNumber;

        public TwilioSmsService(string accountSid, string authToken, string fromPhoneNumber)
        {
            _accountSid = accountSid;
            _authToken = authToken;
            _fromPhoneNumber = fromPhoneNumber;
        }

        public async Task<string> SendSmsAsync(string toPhoneNumber, string message)
        {
            try
            {
                TwilioClient.Init(_accountSid, _authToken);

                var messageResource = await MessageResource.CreateAsync(
                    body: message,
                    from: new PhoneNumber(_fromPhoneNumber),
                    to: new PhoneNumber(toPhoneNumber)
                );

                return $"SMS sent successfully with SID: {messageResource.Sid}";
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return $"Error sending SMS: {ex.Message}";
            }
        }
    }
}