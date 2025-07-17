using System.Collections.Generic;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
    class CountyController
    {
        public void CreateCounty(County t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<County>();
                rep.Insert(t);
            }
        }

        public void DeleteCounty(int countyId, int moduleId)
        {
            var t = GetCounty(countyId, moduleId);
            DeleteCounty(t);
        }

        public void DeleteCounty(County t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<County>();
                rep.Delete(t);
            }
        }

        public IEnumerable<County> GetCounties(int moduleId)
        {
            IEnumerable<County> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<County>();
                t = rep.Get(moduleId);
            }
            return t;
        }

        public County GetCounty(int countyId, int moduleId)
        {
            County t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<County>();
                t = rep.GetById(countyId, moduleId);
            }
            return t;
        }

        public void UpdateCounty(County t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<County>();
                rep.Update(t);
            }
        }
        public IEnumerable<County> GetCountiesByAgency(int agencyId)
        {
            IEnumerable<County> counties;
            using (IDataContext ctx = DataContext.Instance())
            {
                counties = ctx.ExecuteQuery<County>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_counties_by_agency", agencyId);
            }
            return counties;
        }

    }
}
