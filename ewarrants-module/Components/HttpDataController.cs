using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using tjc.Modules.eWarrants.ClerkInterface;

namespace tjc.Modules.eWarrants.Components
{
    public class HttpDataController : DnnApiController
    {
        [HttpGet]
        [ActionName("judges")]
        [DnnAuthorize]
        public List<NameValueItem> GetJudges(int moduleId, int countyId, int judgeTypeId)
        {
            try
            {
                var ctl = new JudgeController();
                var judges = ctl.GetJudgesByJudgeTypeByCounty(moduleId, countyId, judgeTypeId);
                var cCtl = new CountyController();
                County county = cCtl.GetCounty(countyId, moduleId);
                return judges.Select(nl => new NameValueItem() { Text = nl.JudgeName.Replace("&nbsp;", " ") + " (" + county.CountyCode + ")", Value = nl.JudgeID.ToString() }).ToList();
            }
            catch (Exception ex)
            {
                // Log to DotNetNuke and reply with Error
                Exceptions.LogException(ex);
                return null;
            }
        }
        [HttpGet]
        [ActionName("GetPortalWarrants")]
        [DnnAuthorize]
        public IEnumerable<PortalWarrant> GetPortalWarrants()
        {
            try
            {
                var ctl = new WarrantController();
                var warrants = ctl.GetPortalWarrants();
                return warrants;
            }
            catch (Exception ex)
            {
                // Log to DotNetNuke and reply with Error
                Exceptions.LogException(ex);
                return null;
            }
        }
        [HttpGet]
        [ActionName("SetPortalStatus")]
        [DnnAuthorize]
        public PortalStatusResponse SetPortalStatus(int warrantId, int status)
        {
            try
            {
                var ctl = new WarrantController();
                ctl.UpdatePortalStatus(warrantId, status);
                return new PortalStatusResponse { ResponseResult=1, ResponseError=string.Empty };
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return new PortalStatusResponse { ResponseResult = 0, ResponseError=ex.Message }; 
            }
        }
        [HttpGet]
        [ActionName("rpojudges")]
        [DnnAuthorize]
        public List<NameValueItem> GetRpoJudges(int moduleId, int countyId, int judgeTypeId)
        {
            try
            {
                var ctl = new JudgeController();
                var jtCtl = new JudgeTypeController();
                JudgeType judgeType = jtCtl.GetJudgeTypeByName(moduleId, "RPO (Sarasota Afer Hours)");
                IEnumerable<Judge> judges = null;
                var cCtl = new CountyController();
                County county = cCtl.GetCounty(countyId, moduleId);
                DotNetNuke.Entities.Modules.ModuleController mCtl = new DotNetNuke.Entities.Modules.ModuleController();
                DotNetNuke.Entities.Modules.ModuleInfo warrantModule = mCtl.GetModule(moduleId);
                string dAOJ = System.Convert.ToString(warrantModule.TabModuleSettings["dAOJ"] ?? "");
                string mAOJ = System.Convert.ToString(warrantModule.TabModuleSettings["mAOJ"] ?? "");
                string adminJudgerole = System.Convert.ToString(warrantModule.TabModuleSettings["AdminJudge"]);
                string sAOJ = System.Convert.ToString(warrantModule.TabModuleSettings["sAOJ"] ?? "");
                List<Judge> outJudges = new List<Judge>();
                int coverJudgeId = Null.NullInteger;
                int currentHour = DateTime.Now.Hour;
                judges = ctl.GetJudgesByJudgeTypeByCounty(moduleId, countyId, judgeTypeId);
                bool includeCoverJudge = false;
                TimeSpan currentTime = DateTime.Now.TimeOfDay;
                bool isWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Sunday & DateTime.Now.DayOfWeek == DayOfWeek.Saturday;

                foreach (Judge judge in judges)
                {
                    if (judge.DayEnd.TimeOfDay > currentTime && currentTime > judge.DayStart.TimeOfDay)
                    {
                        if (county.CountyName.Substring(0, 1).ToLower() == "s")
                        {
                            if (isWeekend)
                            {
                                includeCoverJudge = true;
                            }
                            else
                            {
                                outJudges.Add(judge);
                            }
                        }
                        else
                        {
                            outJudges.Add(judge);
                        }
                    }
                    else
                    {
                        includeCoverJudge = true;
                    }
                }
                if (includeCoverJudge)
                {
                    if (county.CountyName.Substring(0, 1).ToLower() == "s")
                    {
                        outJudges.AddRange(ctl.GetJudgesByJudgeTypeByCounty(moduleId, countyId, judgeType.JudgeTypeID));
                    }
                    else
                    {
                        switch (county.CountyName.Substring(0, 1).ToLower())
                        {
                            case "d":
                                {
                                    if (!string.IsNullOrEmpty(dAOJ))
                                        coverJudgeId = System.Convert.ToInt32(dAOJ);
                                    break;
                                }

                            case "m":
                                {
                                    if (!string.IsNullOrEmpty(mAOJ))
                                        coverJudgeId = System.Convert.ToInt32(mAOJ);
                                    break;
                                }
                        }
                        Judge coverJudge = ctl.GetJudge(coverJudgeId, moduleId);
                        if (coverJudge != null)
                        {
                            outJudges.Add(coverJudge);
                        }
                    }
                }
                return outJudges.Select(nl => new NameValueItem() { Text = nl.User.DisplayName.Replace("&nbsp;", " ") + " (" + county.CountyCode + ")", Value = nl.JudgeID.ToString() }).ToList();
            }
            catch (Exception ex)
            {
                // Log to DotNetNuke and reply with Error
                Exceptions.LogException(ex);
                return null;
            }
        }
       
        [HttpGet]
        [ActionName("notifyjudge")]
        [DnnAuthorize]
        public NotificationResponse NotifiyJudge(int moduleId, int judgeId)
        {
            var ctl = new NotificationController();
            NotificationResponse notification = new NotificationResponse
            {
                Available = true,
                StartOfDay = ""
            };
            IEnumerable<Notification> colNotifications = ctl.GetCurrentNotificationsByJudge(judgeId);
            var jCtl = new JudgeController();
            Judge judge;
            if (colNotifications != null && colNotifications.Count() > 0)
            {
                Notification objNotification = colNotifications.FirstOrDefault();

                if (objNotification.CoveringJudgeID > 0)
                    judge = jCtl.GetJudge(objNotification.CoveringJudgeID, moduleId);
                else
                    judge = jCtl.GetJudge(judgeId, moduleId);
            }
            else
                judge = jCtl.GetJudge(judgeId, moduleId);
            if (judge.DayEnd == Null.NullDate | judge.DayStart == Null.NullDate)
                return notification;
            if (DateTime.Now.TimeOfDay > judge.DayEnd.TimeOfDay | DateTime.Now.TimeOfDay < judge.DayStart.TimeOfDay | DateTime.Now.DayOfWeek == DayOfWeek.Saturday | DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
            {
                notification.Available = false;

                notification.StartOfDay = judge.DayStart.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday | DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                    notification.StartOfDay = "Monday " + notification.StartOfDay;
                return notification;
            }
            return notification;
        }
        
        [HttpPost]
        [ActionName("AddAnnotation")]
        [DnnAuthorize]
        public HttpResponseMessage AddAnnotationText(AnnotationTextItem text)
        {
            try
            {
                var ctl = new CustomAnnotationController();
                IEnumerable<CustomAnnotation> colAnnotations = ctl.GetCustomAnnotationsByUser(UserInfo.UserID);
                foreach (var a in colAnnotations)
                {
                    if (a.AnnotationText.ToLower().Trim() == text.AnnotationText.ToLower().Trim())
                        return Request.CreateResponse(HttpStatusCode.OK);
                }
                if (!string.IsNullOrEmpty(text.AnnotationText))
                {
                    CustomAnnotation customAnnotation = new CustomAnnotation { AnnotationText = text.AnnotationText.Trim(), UserID = UserInfo.UserID };
                    ctl.CreateCustomAnnotation(customAnnotation);
                }
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
        
        [HttpPost]
        [ActionName("DeleteAnnotation")]
        [DnnAuthorize]
        public HttpResponseMessage DeleteAnnotationText(AnnotationIdItem annotationId)
        {
            try
            {
                var ctl = new CustomAnnotationController();
                ctl.DeleteCustomAnnotation(annotationId.AnnotationId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [ActionName("GetAnnotations")]
        [DnnAuthorize]
        public List<NameValueItem> GetAnnotations()
        {
            try
            {
                var ctl = new CustomAnnotationController();
                return ctl.GetCustomAnnotationsByUser(UserInfo.UserID).Select(nl => new NameValueItem() { Text = nl.AnnotationText, Value = nl.AnnotationID.ToString() }).OrderBy(n => n.Text).ToList();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return null;
            }
        }

       
        public class NameValueItem
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }

        public class NotificationResponse
        {
            public bool Available { get; set; }
            public string StartOfDay { get; set; }
        }

        public class AnnotationTextItem
        {
            public string AnnotationText { get; set; }
        }

        public class AnnotationIdItem
        {
            public int AnnotationId { get; set; }
        }
        public class PortalStatusResponse
        {
            public int ResponseResult { get; set; }
            public string ResponseError { get; set; }

        }

        private bool IsApproved(string encryptedText, int judgeId, string AdminJudgeRole)
        {
            bool approved = false;
            int portalId = PortalSettings.PortalId;
            UserInfo judgeInfo = UserController.GetUserById(portalId, judgeId);
            if (judgeInfo != null)
            {
                int tempJudgeId = 0;
                Encryptor encrypt = new Encryptor();
                try
                {
                    tempJudgeId = Int32.Parse(encrypt.QueryStringDecode(encryptedText, AdminJudgeRole));
                }
                catch
                {
                }
                if (tempJudgeId == judgeId)
                    approved = true;
            }
            return approved;
        }

    }
}