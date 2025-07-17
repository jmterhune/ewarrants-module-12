using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
    class AgencyController
    {
        public void CreateAgency(Agency t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Agency>();
                rep.Insert(t);
            }
        }

        public void DeleteAgency(int agencyId, int moduleId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_delete_agency", agencyId);
            }
        }

        public IEnumerable<Agency> GetAgencies(int moduleId)
        {
            IEnumerable<Agency> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Agency>();
                t = rep.Get(moduleId);
            }
            return t;
        }

        public Agency GetAgency(int agencyId, int moduleId)
        {
            Agency t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Agency>();
                t = rep.GetById(agencyId, moduleId);
            }
            return t;
        }
        public Agency GetAgencyById(int agencyId)
        {
            Agency t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Agency>();
                t = rep.GetById(agencyId);
            }
            return t;
        }



        public void UpdateAgency(Agency t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Agency>();
                rep.Update(t);
            }
        }
        #region Agency By County xref
        public IEnumerable<Agency> GetAgenciesByCounty(int countyId)
        {
            IEnumerable<Agency> agencies;
            using (IDataContext ctx = DataContext.Instance())
            {
                agencies = ctx.ExecuteQuery<Agency>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_agencies_by_county", countyId);
            }
            return agencies;
        }

        public AgencyCounty GetAgencyCountyXref(int agencyId, int countyId)
        {
            AgencyCounty t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgencyCounty>();
                t = rep.Find("Where AgencyID = @0 And CountyID = @1", agencyId, countyId).FirstOrDefault();
            }
            return t;
        }

        public void CreateAgencyCounty(AgencyCounty t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_create_agency_county_xref", t.AgencyID, t.CountyID);
            }
        }

        public void DeleteAgencyCounty(int agencyId, int countyId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_delete_agency_county_xref", agencyId, countyId);
            }
        }

        public void DeleteAgencyCounties(int agencyId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_delete_agency_counties", agencyId);
            }
        }

        #endregion

        #region Agency Users

        public IEnumerable<AgencyUserListItem> GetAgencyUsers(int agencyId)
        {
            IEnumerable<AgencyUserListItem> users;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgencyUserListItem>();
                users = rep.Find("Where AgencyID = @0", agencyId);
            }
            return users;
        }
        public AgencyUserListItem GetAgencyUserByAgency(int userId,int agencyId)
        {
            AgencyUserListItem t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgencyUserListItem>();
                t = rep.Find("Where UserID = @0 And AgencyID = @1", userId, agencyId).FirstOrDefault();
            }
            return t;
        }
        public AgencyUserListItem GetAgencyUser(int userId, int moduleId)
        {
            AgencyUserListItem t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgencyUserListItem>();
                t = rep.Find("Where UserID = @0 And ModuleID = @1", userId, moduleId).FirstOrDefault();
            }
            return t;
        }
        public IEnumerable<AgencyUserListItem> GetUserAgencies(int userId, int moduleId)
        {
            IEnumerable<AgencyUserListItem> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgencyUserListItem>();
                t = rep.Find("Where UserID = @0 And ModuleID = @1", userId, moduleId);
            }
            return t;
        }

        public AgencyUser GetUserAgencyXref(int userId, int agencyId)
        {
            AgencyUser t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AgencyUser>();
                t = rep.Find("Where UserID = @0 And AgencyID = @1", userId, agencyId).FirstOrDefault();
            }
            return t;
        }

        public void CreateAgencyUserXref(AgencyUser t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_create_user_agency_xref", t.UserID, t.AgencyID, t.IsAdmin);
            }
        }

        public void DeleteAgencyUser(int userId, int agencyId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_delete_user_agency_xref", userId, agencyId);
            }
        }
        public void UpdateAgencyUserAdmin(AgencyUser objAgencyUser)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_update_agency_user_admin", objAgencyUser.UserID, objAgencyUser.AgencyID, objAgencyUser.IsAdmin);
            }
        }


        #endregion
    }
}
