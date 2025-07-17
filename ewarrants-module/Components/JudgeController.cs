using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
   public class JudgeController
    {
        public void CreateJudge(Judge t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Judge>();
                rep.Insert(t);
            }
        }

        public void DeleteJudge(int judgeId, int moduleId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_update_judge_approval", judgeId, moduleId);
            }
        }

        public void DeleteJudge(Judge t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Judge>();
                rep.Delete(t);
            }
        }

        public IEnumerable<Judge> GetJudges(int moduleId)
        {
            IEnumerable<Judge> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Judge>();
                t = rep.Get(moduleId);
            }
            return t;
        }

        public Judge GetJudge(int judgeId, int moduleId)
        {
            Judge t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Judge>();
                t = rep.GetById(judgeId, moduleId);
            }
            return t;
        }

        public Judge GetJudge(int judgeId)
        {
            Judge t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Judge>();
                t = rep.Find("Where JudgeID = @0", judgeId).FirstOrDefault();
            }
            return t;
        }


        public void UpdateJudge(Judge t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Judge>();
                rep.Update(t);
            }
        }
        public void UpdateJudgeApproval(int judgeId, string judgeApprovalKey)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_update_judge_approval", judgeId, judgeApprovalKey);
                var rep = ctx.GetRepository<Judge>();
            }
        }

        #region Judge Types Xref

        public void CreateJudgeJudgeType(int judgeId, int judgeTypeId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_create_judge_judgetype_xref", judgeId, judgeTypeId);
            }
        }
        public void DeleteJudgeTypesByJudge(int judgeId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_delete_judge_judge_type_xref_by_judge", judgeId);
            }
        }

        public IEnumerable<JudgeType> GetJudgeTypesByJudge(int judgeId)
        {
            IEnumerable<JudgeType> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                t = ctx.ExecuteQuery<JudgeType>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_judge_types_by_judge", judgeId);
            }
            return t;
        }
        public IEnumerable<Judge> GetJudgesByJudgeType(int moduleId, int judgeTypeId)
        {
            IEnumerable<Judge> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                t = ctx.ExecuteQuery<Judge>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_judges_by_judgetype", moduleId, judgeTypeId);
            }
            return t;
        }
        public IEnumerable<Judge> GetJudgesByJudgeTypeByCounty(int moduleId, int countyId, int judgeTypeId)
        {
            IEnumerable<Judge> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                t = ctx.ExecuteQuery<Judge>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_judges_by_judgetype_county", moduleId, countyId, judgeTypeId);
            }
            return t;
        }
        public void AddJudgeJudgeTypes(int judgeId, ListItemCollection judgeTypes)
        {
            DeleteJudgeTypesByJudge(judgeId);
            foreach (ListItem judgeType in judgeTypes)
            {
                if (judgeType.Selected)
                {
                    Int32.TryParse(judgeType.Value, out int judgeTypeId);
                    CreateJudgeJudgeType(judgeId, judgeTypeId);
                }
            }
        }
        #endregion

        #region County Xref
        public void CreateJudgeCounty(int judgeId, int countyId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_create_judge_county_xref", judgeId, countyId);
            }
        }

        public void DeleteCountiesByJudge(int judgeId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_delete_judge_county_xref_by_judge", judgeId);
            }
        }


        public IEnumerable<County> GetCountiesByJudge(int judgeId)
        {
            IEnumerable<County> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                t = ctx.ExecuteQuery<County>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_counties_by_judge", judgeId);
            }
            return t;
        }

        public void AddJudgeCounties(int judgeId, ListItemCollection counties)
        {
            DeleteCountiesByJudge(judgeId);
            foreach (ListItem county in counties)
            {
                if (county.Selected)
                {
                    Int32.TryParse(county.Value, out int countId);
                    CreateJudgeCounty(judgeId, countId);
                }
            }
        }
        #endregion
    }
}
