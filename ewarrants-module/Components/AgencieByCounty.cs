using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_agencies_by_county")]
    class AgencyCounty
    {
        public int AgencyID { get; set; }
        public int CountyID { get; set; }
    }
}
