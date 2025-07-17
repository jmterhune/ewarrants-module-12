using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_judges_by_judge_type")]
   public class JudgeJudgeType_xref
    {
        public int JudgeID { get; set; }
        public int JudgeTypeID { get; set; }
    }
}
