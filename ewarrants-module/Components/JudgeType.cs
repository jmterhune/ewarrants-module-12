using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_judge_types")]
    //setup the primary key for table
    [PrimaryKey("JudgeTypeID", AutoIncrement = true)]
    //configure caching using PetaPoco
    [Cacheable("judgetypes", CacheItemPriority.Default, 20)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    [Scope("ModuleID")]
    public class JudgeType
    {
        public int JudgeTypeID { get; set; }
        public int ModuleID { get; set; }
        public string JudgeTypeName { get; set; }
    }
}
