using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_agency_user")]
    class AgencyUserListItem
    {
        public int AgencyID { get; set; }
        public int UserID { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DisplayName { get; set; }
        public bool IsAdmin { get; set; }
        public int ModuleID { get; set; }
        public string AgencyName { get; set; }
    }
}
