using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
    class JudgeTypeController
    {
        public void CreateJudgeType(JudgeType t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<JudgeType>();
                rep.Insert(t);
            }
        }

        public void DeleteJudgeType(int judgeTypeId, int moduleId)
        {
            var t = GetJudgeType(judgeTypeId, moduleId);
            DeleteJudgeType(t);
        }

        public void DeleteJudgeType(JudgeType t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<JudgeType>();
                rep.Delete(t);
            }
        }
        public JudgeType GetJudgeTypeByName(int moduleId, string typeName)
        {
            JudgeType t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<JudgeType>();
                t = rep.Find("Where ModuleId=@0 And JudgeTypeName like @1", moduleId, typeName).FirstOrDefault();
            }
            return t;
        }
        public IEnumerable<JudgeType> GetJudgeTypes(int moduleId)
        {
            IEnumerable<JudgeType> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<JudgeType>();
                t = rep.Get(moduleId);
            }
            return t;
        }

        public JudgeType GetJudgeType(int judgeTypeId, int moduleId)
        {
            JudgeType t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<JudgeType>();
                t = rep.GetById(judgeTypeId, moduleId);
            }
            return t;
        }

        public void UpdateJudgeType(JudgeType t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<JudgeType>();
                rep.Update(t);
            }
        }
    }
}
