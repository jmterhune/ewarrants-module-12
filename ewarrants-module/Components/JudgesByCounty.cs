using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_judges_by_county")]
    class JudgeCounty_xref
    {
        public int JudgeID { get; set; }
        public int CountyID { get; set; }
    }
}
