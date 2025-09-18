using DotNetNuke.Abstractions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PhoneNumbers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using tjc.Modules.eWarrants.Components;

namespace tjc.Modules.eWarrants
{
    public partial class SMS : ewarrantsModuleBase
    {
        private readonly INavigationManager _navigationManager;
        public SMS()
        {
            _navigationManager = DependencyProvider.GetRequiredService<INavigationManager>();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!Page.IsPostBack)
                {
                    if (DotNetNuke.Framework.AJAX.IsInstalled())
                        DotNetNuke.Framework.AJAX.RegisterScriptManager();
                    if (Request.QueryString["msg"] != null)
                        DotNetNuke.UI.Skins.Skin.AddModuleMessage(this, Request.QueryString["msg"].ToString().Trim(), DotNetNuke.UI.Skins.Controls.ModuleMessage.ModuleMessageType.YellowWarning);

                    lnkCancel.NavigateUrl = _navigationManager.NavigateURL();
                }
                ltMesssage.Text = string.Empty;
                ltPhoneError.Text = string.Empty;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                SetMessage("danger", "exclamation-triangle", "Error loading page. Please try again or contact support.");
            }
        }

        protected string GetPhoneNumbersJson()
        {
            string json = Helper.GetPhoneNumbersJson(UserInfo);
            if (Page.IsPostBack && ViewState["tempPhones"] != null)
            {
                json = ViewState["tempPhones"].ToString();
            }
            return json;
        }

        protected void btnSaveOptIn_Click(object sender, EventArgs e)
        {
            try
            {
                var user = UserController.GetUserById(PortalId, UserId);
                if (user == null)
                    throw new InvalidOperationException("User not found.");
                bool consent = chkSmsConsent.Checked;
                var phoneNumbers = new List<string>();

                // Server-side validation for consent
                if (!consent)
                {
                    ltPhoneError.Text = "<ul class=\"error-list\"><li>You must agree to receive SMS notifications to opt in.</li></ul>";
                    ViewState["tempPhones"] = hdPhoneNumbers.Value;
                    return;
                }

                if (!string.IsNullOrEmpty(hdPhoneNumbers.Value))
                {
                    try
                    {
                        phoneNumbers = JsonConvert.DeserializeObject<List<string>>(hdPhoneNumbers.Value) ?? new List<string>();
                    }
                    catch (JsonException)
                    {
                        ltPhoneError.Text = "<ul class=\"error-list\"><li>Invalid phone data.</li></ul>";
                        ViewState["tempPhones"] = hdPhoneNumbers.Value;
                        return;
                    }
                }

                if (phoneNumbers.Count == 0)
                {
                    ltPhoneError.Text = "<ul class=\"error-list\"><li>At least one phone number is required to opt in.</li></ul>";
                    ViewState["tempPhones"] = hdPhoneNumbers.Value;
                    return;
                }

                if (consent)
                {
                    var phoneUtil = PhoneNumberUtil.GetInstance();
                    var validNumbers = new List<string>();
                    foreach (var number in phoneNumbers)
                    {
                        try
                        {
                            var parsedNumber = phoneUtil.Parse(number, null); // Attempt to parse the number
                            validNumbers.Add(phoneUtil.Format(parsedNumber, PhoneNumberFormat.E164)); // Reformat to E.164
                        }
                        catch (NumberParseException exc)
                        {
                            ltPhoneError.Text = $"<ul class=\"error-list\"><li>Unable to parse phone number: {number}. Please ensure it is a valid number.</li></ul>";
                            ViewState["tempPhones"] = hdPhoneNumbers.Value;
                            Exceptions.LogException(exc);
                            return;
                        }
                    }
                    user.Profile.SetProfileProperty("SMSPhoneNumbers", string.Join(",", validNumbers));
                }
                else
                {
                    user.Profile.SetProfileProperty("SMSPhoneNumbers", "");
                }

                user.Profile.SetProfileProperty("SmsConsent", consent.ToString());
                UserController.UpdateUser(PortalId, user);
                if (consent)
                {
                    SetMessage("success", "thumbs-up", "SMS preferences saved successfully!");
                }
                else
                {
                    SetMessage("warning", "exclamation-triangle", "You have opted out of SMS notifications.");
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                SetMessage("danger", "exclamation-triangle", "Error saving SMS preferences: " + exc.Message);
            }
        }

        protected void btnOptOut_Click(object sender, EventArgs e)
        {
            try
            {
                var user = UserController.GetUserById(PortalId, UserId);
                if (user == null)
                    throw new InvalidOperationException("User not found.");
                user.Profile.SetProfileProperty("SMSPhoneNumbers", "");
                user.Profile.SetProfileProperty("SmsConsent", "False");
                UserController.UpdateUser(PortalId, user);
                SetMessage("warning", "exclamation-triangle", "You have opted out of SMS notifications.");
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
                SetMessage("danger", "exclamation-triangle", "Error opting out: " + exc.Message);
            }
        }

        //protected void upSmsModal_Unload(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        MethodInfo methodInfo = typeof(ScriptManager).GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
        //            .FirstOrDefault(i => i.Name.Equals("System.Web.UI.IScriptManagerInternal.RegisterUpdatePanel"));
        //        if (methodInfo != null)
        //            methodInfo.Invoke(ScriptManager.GetCurrent(Page), new object[] { sender as UpdatePanel });
        //    }
        //    catch (Exception exc)
        //    {
        //        Exceptions.ProcessModuleLoadException(this, exc);
        //    }
        //}

        protected void SetMessage(string alertType, string iconType, string message)
        {
            ltMesssage.Text = string.Format("<div class=\"alert alert-{0}\"><i class=\"fa fa-{1}\"></i> {2}</div>", alertType, iconType, message);
        }
    }
}