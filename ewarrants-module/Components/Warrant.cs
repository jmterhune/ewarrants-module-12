using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Caching;
using System.Web.Services.Description;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Users;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_warrants")]
    //setup the primary key for table
    [PrimaryKey("WarrantID", AutoIncrement = true)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    [Scope("ModuleID")]
    public class Warrant
    {

        public int WarrantID { get; set; }
        public int ModuleID { get; set; }
        public int AgencyID { get; set; }
        public int CreatedByUserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public WarrantStatus Status { get; set; }
        public int ReviewedByJudgeUserID { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public int JudgeUserID { get; set; }
        public string CountyName { get; set; }
        public int FileID { get; set; }
        public string Title { get; set; }
        public string Defendant { get; set; }
        public string StateAttorney { get; set; }
        public WarrantType WarrantType { get; set; }
        public int ReviewedByAgencyUserID { get; set; }
        public DateTime? ReviewedByAgencyDate { get; set; }
        public DateTime? NotificationSentDate { get; set; }
        public bool IsMarked { get; set; }
      //  public int PortalStatus { get; set; }
        public string AdministeredOathName { get; set; }
        public string AdministeredOathAgency { get; set; }
        public string AdministeredOathId { get; set; }
        public bool IsJuvenile { get; set; }

        [IgnoreColumn]
        public Agency Agency
        {
            get
            {
                var ctl = new AgencyController();
                return ctl.GetAgency(AgencyID, ModuleID);
            }
        }
        [IgnoreColumn]
        public string StatusToolTip
        {
            get
            {
                switch (Status)
                {
                    case WarrantStatus.New:
                        {
                            return "New";
                        }

                    case WarrantStatus.Rejected:
                        {
                            return "Rejected by " + GetJudgeName(ReviewedByJudgeUserID, ModuleID);
                        }

                    case WarrantStatus.Signed:
                        {
                            return "Signed by " + GetJudgeName(ReviewedByJudgeUserID, ModuleID);
                        }

                    case WarrantStatus.UnderReview:
                        {
                            return "Under Review By " + GetJudgeName(ReviewedByJudgeUserID, ModuleID);
                        }

                    case WarrantStatus.Complete:
                        {
                            int hoursThreshold = 24;
                            var ctlSettings = new SettingController();
                            Setting setting = ctlSettings.GetSettings(ModuleID);
                            if (setting != null) { hoursThreshold = ctlSettings.GetSettings(ModuleID).Hours; }

                            int hoursPast = (int)(DateTime.Now - ReviewedByAgencyDate.GetValueOrDefault()).TotalHours;
                            var ctl = new SettingController();
                            if (hoursPast > hoursThreshold)
                                return "Completed - this warrant is scheduled to be deleted";
                            else
                                return "Completed - Will be automatically deleted in " + (hoursThreshold - hoursPast).ToString() + " hours!";
                        }
                }
                return "";
            }
        }
        private string GetJudgeName(int userId, int moduleId)
        {
            var ctl = new JudgeController();
            Judge objJudge = ctl.GetJudge(userId, moduleId);
            if (objJudge != null)
                return objJudge.JudgeName;
            else
                return "";
        }
    }

    [TableName("tjc_warrants")]
    //setup the primary key for table
    [PrimaryKey("WarrantID", AutoIncrement = false)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    [Scope("ModuleID")]

    public class WarrantListItem : Warrant
    {
        public string JudgeName { get; set; }
        public string CreatedByName { get; set; }
        public string AgencyName { get; set; }
        public int ParentAgencyID { get; set; }
    }
    [TableName("vw_portal_warrants_all")]
    //setup the primary key for table
    [PrimaryKey("WarrantID", AutoIncrement = false)]
    public class PortalWarrant
    {
        public int WarrantID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public string Title { get; set; }
        public string Defendant { get; set; }
        public int WarrantType { get; set; }
        public string WarrantTypeName { get; set; }
        public string ReviewedJudgeName { get; set; }
        public string AssignedJudgeName { get; set; }
        public string AgencyName { get; set; }
        public string LeoName { get; set; }
        public string LeoEmail { get; set; }
        public string StateAttorney { get; set; }
        public string CountyName { get; set; }
        public int FileType { get; set; }
        public string FileTypeName { get; set; }
        public int PortalStatus { get; set; }
        public bool IsJuvenile { get; set; }
    }
    public enum WarrantStatus
    {
        [Display(Name = "N")]
        New = 1,
        [Display(Name = "U")]
        UnderReview = 2,
        [Display(Name = "S")]
        Signed = 3,
        [Display(Name = "R")]
        Rejected = 4,
        [Display(Name = "C")]
        Complete = 5
    }
    public enum WarrantType
    {
        [Display(Name = "Search Warrant")]
        Search = 2,
        [Display(Name = "Arrest Warrant")]
        Arrest = 1,
        [Display(Name = "Juvenile Pickup Order")]
        PickupOrder = 3,
        [Display(Name = "RPO")]
        RPO = 4,
        [Display(Name = "CBS")]
        CBS = 5,
        [Display(Name = "Civil Forfeiture")]
        CivilForfeiture = 0
    }
}
