using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_users_by_agency")]
    class AgencyUser
    {
        public int AgencyID { get; set; }
        public int UserID { get; set; }
        public bool IsAdmin { get; set; }
    }
}
