using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_settings")]
    //setup the primary key for table
    [PrimaryKey("ModuleID", AutoIncrement = false)]
    //configure caching using PetaPoco
    [Cacheable("warrantsettings", CacheItemPriority.Default, 20)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    class Setting
    {
        public int ModuleID { get; set; }
        public string NotificationEmail { get; set; }
        public string SenderEmail { get; set; }
        public int DeleteThreshold { get; set; }
        public int SignedThreshold { get; set; }
        public int ClaimedThreshold { get; set; }
        public bool DemoMode { get; set; }
        public int Hours { get; set; }
    }
}
