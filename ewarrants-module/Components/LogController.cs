using System.Collections.Generic;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
    class LogController
    {
        public void CreateLog(Log t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Log>();
                rep.Insert(t);
            }
        }

        public void DeleteLog(int logId, int moduleId)
        {
            var t = GetLog(logId, moduleId);
            DeleteLog(t);
        }

        public void DeleteLog(Log t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Log>();
                rep.Delete(t);
            }
        }

        public IEnumerable<Log> GetLogs(int moduleId)
        {
            IEnumerable<Log> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Log>();
                t = rep.Get(moduleId);
            }
            return t;
        }

        public Log GetLog(int logId, int moduleId)
        {
            Log t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Log>();
                t = rep.GetById(logId, moduleId);
            }
            return t;
        }

        public void UpdateLog(Log t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Log>();
                rep.Update(t);
            }
        }

    }
}
