using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_counties")]
    //setup the primary key for table
    [PrimaryKey("CountyID", AutoIncrement = true)]
    //configure caching using PetaPoco
    [Cacheable("counties", CacheItemPriority.Default, 20)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    [Scope("ModuleID")]
    public class County
    {
        ///<summary>
        /// The ID for the County
        ///</summary>
        public int CountyID { get; set; }
        ///<summary>
        /// A string with the name of the County Name
        ///</summary>
        public string CountyName { get; set; }

        ///<summary>
        /// The ModuleId of where the object was created and gets displayed
        ///</summary>
        public int ModuleID { get; set; }

        [IgnoreColumn]
        public string CountyCode { get { return CountyName.Substring(0, 3).ToUpper(); } }

    }
}
