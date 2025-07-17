using System.Collections.Generic;
using System.Linq;
using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_agencies")]
    //setup the primary key for table
    [PrimaryKey("AgencyID", AutoIncrement = true)]
    //configure caching using PetaPoco
    [Cacheable("agencies", CacheItemPriority.Default, 20)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    [Scope("ModuleID")]
    public class Agency
    {
        ///<summary>
        /// The ID for the agency
        ///</summary>
        public int AgencyID { get; set; }
        ///<summary>
        /// A string with the name of the Agency Name
        ///</summary>
        public string AgencyName { get; set; }

        ///<summary>
        /// A string with the email address of the Agency Contact
        ///</summary>
        public string EmailAddress { get; set; }

        ///<summary>
        /// An integer establishes a hierachy of sub agencies
        ///</summary>
        public int ParentAgencyID { get; set; }

        ///<summary>
        /// The ModuleId of where the object was created and gets displayed
        ///</summary>
        public int ModuleID { get; set; }

        ///<summary>
        /// A boolean that establishes the agency as a view only agency
        /// can only view signed warrants
        ///</summary>
        public bool ViewOnly { get; set; }
        [IgnoreColumn]
        public Agency ParentAgency
        {
            get
            {
                var ctl = new AgencyController();
                return ctl.GetAgency(ParentAgencyID, ModuleID);
            }
        }
        [IgnoreColumn]
        public string ParentAgencyName { get { if (ParentAgency != null) { return ParentAgency.AgencyName; } else { return ""; } } }
        [IgnoreColumn]
        public IEnumerable<County> Counties
        {
            get
            {
                var ctl = new CountyController();
                return ctl.GetCountiesByAgency(AgencyID);
            }
        }
        [IgnoreColumn]
        public string CountyList
        {
            get
            {
                string[] counties = Counties.Select(c => c.CountyName).ToArray();
                return string.Join(", ", counties);
            }
        }

    }
}
