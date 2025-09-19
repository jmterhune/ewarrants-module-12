/*
' Copyright (c) 2020  Joe Terhune
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using tjc.Modules.eWarrants.Components;
using static Atalasoft.Imaging.ImageSources.DbImageSource;

namespace tjc.Modules.eWarrants
{
    public class ewarrantsModuleBase : PortalModuleBase
    {
        public int WarrantId
        {
            get
            {
                var qs = Request.QueryString["wid"];
                if (qs != null)
                    return Convert.ToInt32(qs);
                return -1;
            }

        }
        public int JudgeId
        {
            get
            {
                var qs = Request.QueryString["jid"];
                if (qs != null)
                    return Convert.ToInt32(qs);
                return -1;
            }

        }
        public string JudgeRole
        {
            get
            {
                if (Settings.Contains("JudgeRole"))
                    return Settings["JudgeRole"].ToString();
                else
                    return "";
            }
        }
        public bool HasUserOptedForSms(int userId)
        {
            bool hasConsent = false;
            var user = UserController.Instance.GetUserById(PortalId, userId);
            if (user.IsInRole(JudgeRole))
            {
                string smsConsent = user.Profile.GetPropertyValue("SmsConsent");
                bool.TryParse(smsConsent, out hasConsent);
                return hasConsent;
            }
            return hasConsent;
        }
        public bool HasUserOptedForSms()
        {
            bool hasConsent = false;
            var user = UserInfo != null ? UserInfo : new UserInfo();
            if (UserInfo.IsInRole(JudgeRole))
            {
                string smsConsent = user.Profile.GetPropertyValue("SmsConsent");
                bool.TryParse(smsConsent, out hasConsent);
                return hasConsent;
            }
            return hasConsent;
        }
        public int AgencyId
        {
            get
            {
                var qs = Request.QueryString["aid"];
                if (qs != null)
                    return Convert.ToInt32(qs);
                return -1;
            }

        }
        public int SettingId
        {
            get
            {
                var qs = Request.QueryString["sid"];
                if (qs != null)
                    return Convert.ToInt32(qs);
                return -1;
            }

        }
        public int AgencyUserId
        {
            get
            {
                var qs = Request.QueryString["uid"];
                if (qs != null)
                    return Convert.ToInt32(qs);
                return -1;
            }
        }
        public string SarasotaClerkEmail
        {
            get
            {
                if (Settings.Contains("SarasotaClerk"))
                    return Settings["SarasotaClerk"].ToString().Trim();
                return "";
            }
        }

        public string ManateeClerkEmail
        {
            get
            {
                if (Settings.Contains("ManateeClerk"))
                    return Settings["ManateeClerk"].ToString().Trim();
                return "";
            }
        }
        public string DeSotoClerkEmail
        {
            get
            {
                if (Settings.Contains("DeSotoClerk"))
                    return Settings["DeSotoClerk"].ToString().Trim();
                return "";
            }
        }
        #region WarrantViewProperties
        public int CompletedWarrantsThreshold
        {
            get
            {
                if (ViewState["CompletedWarrantsThreshold"] != null)
                    return System.Convert.ToInt32(ViewState["CompletedWarrantsThreshold"]);
                else
                    return 0;
            }
            set
            {
                ViewState["CompletedWarrantsThreshold"] = value;
            }
        }
        public DateTime StartDate
        {
            get
            {
                if (ViewState["StartDate"] != null)
                    return System.Convert.ToDateTime(ViewState["StartDate"]);
                else
                    return DateTime.Now.AddDays(-11);
            }
            set
            {
                ViewState["StartDate"] = value;
            }
        }
        public DateTime EndDate
        {
            get
            {
                if (ViewState["EndDate"] != null)
                    return System.Convert.ToDateTime(ViewState["EndDate"]);
                else
                    return DateTime.Now;
            }
            set
            {
                ViewState["EndDate"] = value;
            }
        }
        public bool IsAdminJudge
        {
            get
            {
                if (AdminJudgeRole != "")
                    return UserInfo.IsInRole(AdminJudgeRole);
                return false;
            }
        }

        public string AdminJudgeRole
        {
            get
            {
                if (Settings.Contains("AdminJudge"))
                    return Settings["AdminJudge"].ToString().Trim();
                return "";
            }
        }
        public bool IsJudge
        {
            get
            {
                var ctl = new JudgeController();
                Judge objJudge = ctl.GetJudge(UserId, ModuleId);
                if (objJudge != null)
                    if (AdminJudgeRole != "")
                    {
                        try
                        {
                            Components.Encryptor encrypt = new Components.Encryptor();
                            Int32.TryParse(encrypt.QueryStringDecode(objJudge.ApprovalKey, AdminJudgeRole), out int judgeId);
                            return UserId == judgeId;
                        }
                        catch
                        {
                            return false;
                        }
                    }
                return false;
            }
        }
        public bool HasJudgeRole
        {
            get
            {
                var ctl = new JudgeController();
                Judge objJudge = ctl.GetJudge(UserId, ModuleId);
                if (objJudge != null)
                    return true;
                return false;
            }
        }
        public string GetUserInfo(int createdByUserID)
        {
            string returnValue = "";
            UserInfo objUser = UserController.GetUserById(PortalId, createdByUserID);
            if (objUser != null)
            {
                string cell = objUser.Profile.Cell;
                returnValue += string.IsNullOrEmpty(cell) ? "<br />No Cell" : "" + cell;
                returnValue += "<br />";
                returnValue += objUser.Email;
            }
            return returnValue;
        }

        public string JudgeRoleName
        {
            get
            {
                if (Settings.Contains("JudgeRole"))
                    return Settings["JudgeRole"].ToString().Trim();
                return "";
            }
        }
        public string JaRoleName
        {
            get
            {
                if (Settings.Contains("JaRole"))
                    return Settings["JaRole"].ToString().Trim();
                return "";
            }
        }
        public string SergeantRoleName
        {
            get
            {
                if (Settings.Contains("SergeantRole"))
                    return Settings["SergeantRole"].ToString().Trim();
                return "";
            }
        }
        public bool IsSeargeant
        {
            get
            {
                if (SergeantRoleName != "")
                    return UserInfo.IsInRole(SergeantRoleName);
                return false;
            }
        }
        public bool IsSiteAdmin
        {
            get
            {
                bool isAdmin = UserInfo.IsSuperUser;
                if (!isAdmin)
                    return UserInfo.IsInRole("Administrators");
                return isAdmin;
            }
        }
        public string JaPage
        {
            get
            {
                if (Settings.Contains("JaPage"))
                    return Settings["JaPage"].ToString().Trim();
                return "0";
            }
        }
        public string ContactPage
        {
            get
            {
                if (Settings.Contains("ContactPage"))
                    return Settings["ContactPage"].ToString().Trim();
                return "";
            }
        }
        #endregion
        public static string FormatDate(string inDate)
        {
            DateTime.TryParse(inDate, out DateTime outDate);
            if (outDate == DotNetNuke.Common.Utilities.Null.NullDate)
                return "";
            else
                return outDate.ToShortDateString() + " " + outDate.ToShortTimeString();
        }
        public string GetIp()
        {
            string clientIp = Page.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(clientIp))
            {
                string[] forwardedIps = clientIp.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                clientIp = forwardedIps[forwardedIps.Length - 1];
            }
            else
                clientIp = Page.Request.ServerVariables["REMOTE_ADDR"];
            return clientIp;
        }
        public string RPO_CC_Email
        {
            get
            {
                if (Settings.Contains("ccRPO"))
                    return Settings["ccRPO"].ToString();
                return "";
            }
        }
        public int JudgeTypeID
        {
            get
            {
                if (Settings.Contains("RpoJudgeTypeID"))
                {
                    Int32.TryParse(Settings["RpoJudgeTypeID"].ToString(), out int judgeTypeId);
                    return judgeTypeId;
                }
                return -1;
            }
        }
        public string RPO_DeSotoEmail
        {
            get
            {
                if (Settings.Contains("dRPO"))
                    return Settings["dRPO"].ToString();
                return "";
            }
        }
        public string RPO_ManateeEmai
        {
            get
            {
                if (Settings.Contains("mRPO"))
                    return Settings["mRPO"].ToString();
                return "";
            }
        }
        public string RPO_SarasotaEmail
        {
            get
            {
                if (Settings.Contains("sRPO"))
                    return Settings["sRPO"].ToString();
                return "";
            }
        }
    }
}