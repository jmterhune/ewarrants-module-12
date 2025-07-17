using System;
using System.Collections.Generic;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_notifications")]
    //setup the primary key for table
    [PrimaryKey("NotificationID", AutoIncrement = true)]
    //configure caching using PetaPoco
    [Cacheable("notifications", CacheItemPriority.Default, 20)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    public class Notification
    {
        public int NotificationID { get; set; }
        public int RequestingJudgeID { get; set; }

        public int CoveringJudgeID { get; set; }

        public DateTime? StartDateTime { get; set; }

        public DateTime? EndDateTime { get; set; }
        public string Message { get; set; }
        [IgnoreColumn]
        public string CoveringJudgeName { get { var objCoverJuge = UserController.Instance.GetUserById(PortalSettings.Current.PortalId, CoveringJudgeID);
               if(objCoverJuge!=null)
                return objCoverJuge.DisplayName;
                return "";
            } }
    }
}
