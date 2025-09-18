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
                if (moduleId <= 0 || countyId <= 0 || judgeTypeId <= 0)
                    throw new ArgumentException("Invalid parameters: moduleId, countyId, and judgeTypeId must be positive integers.");

                var ctl = new JudgeController();
                var judges = ctl.GetJudgesByJudgeTypeByCounty(moduleId, countyId, judgeTypeId) ?? new List<Judge>();
                var cCtl = new CountyController();
                var county = cCtl.GetCounty(countyId, moduleId);
                if (county == null)
                    throw new InvalidOperationException("County not found.");

                return judges.Select(nl => new NameValueItem { Text = nl.JudgeName + " (" + county.CountyCode + ")", Value = nl.JudgeID.ToString() }).ToList();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error retrieving judges: " + ex.Message));
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
                var warrants = ctl.GetPortalWarrants() ?? new List<PortalWarrant>();
                return warrants;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error retrieving portal warrants: " + ex.Message));
            }
        }

        [HttpGet]
        [ActionName("SetPortalStatus")]
        [DnnAuthorize]
        public PortalStatusResponse SetPortalStatus(int warrantId, int status)
        {
            try
            {
                if (warrantId <= 0)
                    throw new ArgumentException("Invalid warrant ID.");

                var ctl = new WarrantController();
                ctl.UpdatePortalStatus(warrantId, status);
                return new PortalStatusResponse { ResponseResult = 1, ResponseError = string.Empty };
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return new PortalStatusResponse { ResponseResult = 0, ResponseError = ex.Message };
            }
        }

        [HttpGet]
        [ActionName("rpojudges")]
        [DnnAuthorize]
        public List<NameValueItem> GetRpoJudges(int moduleId, int countyId, int judgeTypeId)
        {
            try
            {
                if (moduleId <= 0 || countyId <= 0 || judgeTypeId <= 0)
                    throw new ArgumentException("Invalid parameters: moduleId, countyId, and judgeTypeId must be positive integers.");

                var ctl = new JudgeController();
                var jtCtl = new JudgeTypeController();
                var judgeType = jtCtl.GetJudgeTypeByName(moduleId, "RPO (Sarasota After Hours)");
                if (judgeType == null)
                    throw new InvalidOperationException("RPO judge type not found.");

                var cCtl = new CountyController();
                var county = cCtl.GetCounty(countyId, moduleId);
                if (county == null)
                    throw new InvalidOperationException("County not found.");

                var mCtl = new DotNetNuke.Entities.Modules.ModuleController();
                var warrantModule = mCtl.GetModule(moduleId);
                if (warrantModule == null)
                    throw new InvalidOperationException("Module not found.");

                string dAOJ = Convert.ToString(warrantModule.TabModuleSettings["dAOJ"] ?? "");
                string mAOJ = Convert.ToString(warrantModule.TabModuleSettings["mAOJ"] ?? "");
                string sAOJ = Convert.ToString(warrantModule.TabModuleSettings["sAOJ"] ?? "");

                var outJudges = new List<Judge>();
                int coverJudgeId = Null.NullInteger;
                var currentTime = DateTime.Now.TimeOfDay;
                bool isWeekend = DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday;

                var judges = ctl.GetJudgesByJudgeTypeByCounty(moduleId, countyId, judgeTypeId) ?? new List<Judge>();
                bool includeCoverJudge = false;

                foreach (var judge in judges)
                {
                    if (judge.DayEnd.TimeOfDay > currentTime && currentTime > judge.DayStart.TimeOfDay)
                    {
                        if (county.CountyName.ToLower().StartsWith("s"))
                        {
                            if (isWeekend)
                                includeCoverJudge = true;
                            else
                                outJudges.Add(judge);
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
                    if (county.CountyName.ToLower().StartsWith("s"))
                    {
                        outJudges.AddRange(ctl.GetJudgesByJudgeTypeByCounty(moduleId, countyId, judgeType.JudgeTypeID) ?? new List<Judge>());
                    }
                    else
                    {
                        switch (county.CountyName.ToLower().Substring(0, 1))
                        {
                            case "d":
                                if (!string.IsNullOrEmpty(dAOJ) && Int32.TryParse(dAOJ, out coverJudgeId))
                                    break;
                                break;
                            case "m":
                                if (!string.IsNullOrEmpty(mAOJ) && Int32.TryParse(mAOJ, out coverJudgeId))
                                    break;
                                break;
                        }
                        if (coverJudgeId > 0)
                        {
                            var coverJudge = ctl.GetJudge(coverJudgeId, moduleId);
                            if (coverJudge != null)
                                outJudges.Add(coverJudge);
                        }
                    }
                }

                return outJudges.Select(nl => new NameValueItem { Text = nl.User.DisplayName + " (" + county.CountyCode + ")", Value = nl.JudgeID.ToString() }).ToList();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error retrieving RPO judges: " + ex.Message));
            }
        }

        [HttpGet]
        [ActionName("notifyjudge")]
        [DnnAuthorize]
        public NotificationResponse NotifyJudge(int moduleId, int judgeId)
        {
            try
            {
                if (moduleId <= 0 || judgeId <= 0)
                    throw new ArgumentException("Invalid parameters: moduleId and judgeId must be positive integers.");

                var ctl = new NotificationController();
                var notification = new NotificationResponse
                {
                    Available = true,
                    StartOfDay = ""
                };
                var colNotifications = ctl.GetCurrentNotificationsByJudge(judgeId) ?? new List<Notification>();
                var jCtl = new JudgeController();
                Judge judge;

                if (colNotifications.Any())
                {
                    var objNotification = colNotifications.FirstOrDefault();
                    judge = objNotification.CoveringJudgeID > 0
                        ? jCtl.GetJudge(objNotification.CoveringJudgeID, moduleId)
                        : jCtl.GetJudge(judgeId, moduleId);
                }
                else
                {
                    judge = jCtl.GetJudge(judgeId, moduleId);
                }

                if (judge == null)
                    throw new InvalidOperationException("Judge not found.");

                if (judge.DayEnd == Null.NullDate || judge.DayStart == Null.NullDate)
                    return notification;

                if (DateTime.Now.TimeOfDay > judge.DayEnd.TimeOfDay || DateTime.Now.TimeOfDay < judge.DayStart.TimeOfDay || DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                {
                    notification.Available = false;
                    notification.StartOfDay = judge.DayStart.ToString("hh:mm tt", CultureInfo.InvariantCulture);
                    if (DateTime.Now.DayOfWeek == DayOfWeek.Saturday || DateTime.Now.DayOfWeek == DayOfWeek.Sunday)
                        notification.StartOfDay = "Monday " + notification.StartOfDay;
                }

                return notification;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error checking judge availability: " + ex.Message));
            }
        }

        [HttpPost]
        [ActionName("AddAnnotation")]
        [DnnAuthorize]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AddAnnotationText(AnnotationTextItem text)
        {
            try
            {
                if (text == null || string.IsNullOrWhiteSpace(text.AnnotationText))
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Annotation text cannot be empty.");

                var ctl = new CustomAnnotationController();
                var colAnnotations = ctl.GetCustomAnnotationsByUser(UserInfo.UserID) ?? new List<CustomAnnotation>();
                if (colAnnotations.Any(a => a.AnnotationText.Trim().ToLower() == text.AnnotationText.Trim().ToLower()))
                    return Request.CreateResponse(HttpStatusCode.OK);

                var customAnnotation = new CustomAnnotation { AnnotationText = text.AnnotationText.Trim(), UserID = UserInfo.UserID };
                ctl.CreateCustomAnnotation(customAnnotation);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error adding annotation: " + ex.Message);
            }
        }

        [HttpPost]
        [ActionName("DeleteAnnotation")]
        [DnnAuthorize]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteAnnotationText(AnnotationIdItem annotationId)
        {
            try
            {
                if (annotationId == null || annotationId.AnnotationId <= 0)
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid annotation ID.");

                var ctl = new CustomAnnotationController();
                ctl.DeleteCustomAnnotation(annotationId.AnnotationId);
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error deleting annotation: " + ex.Message);
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
                var annotations = ctl.GetCustomAnnotationsByUser(UserInfo.UserID) ?? new List<CustomAnnotation>();
                return annotations.Select(nl => new NameValueItem { Text = nl.AnnotationText, Value = nl.AnnotationID.ToString() })
                                 .OrderBy(n => n.Text)
                                 .ToList();
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                throw new HttpResponseException(Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Error retrieving annotations: " + ex.Message));
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

        private bool IsApproved(string encryptedText, int judgeId, string adminJudgeRole)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedText) || string.IsNullOrEmpty(adminJudgeRole))
                    return false;

                var judgeInfo = UserController.GetUserById(PortalSettings.PortalId, judgeId);
                if (judgeInfo == null)
                    return false;

                var encrypt = new Encryptor();
                int tempJudgeId;
                if (!Int32.TryParse(encrypt.QueryStringDecode(encryptedText, adminJudgeRole), out tempJudgeId))
                    return false;

                return tempJudgeId == judgeId;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return false;
            }
        }
    }
}