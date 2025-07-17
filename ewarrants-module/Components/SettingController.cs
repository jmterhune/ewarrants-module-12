using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
    class SettingController
    {
        public void CreateSetting(Setting t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Setting>();
                rep.Insert(t);
            }
        }

        public void DeleteSetting(int moduleId)
        {
            var t = GetSettings(moduleId);
            DeleteSetting(t);
        }

        public void DeleteSetting(Setting t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Setting>();
                rep.Delete(t);
            }
        }

        public Setting GetSettings(int moduleId)
        {
            Setting t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Setting>();
                t = rep.GetById(moduleId);
            }
            return t;
        }
        public Setting GetSetting()
        {
            Setting t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Setting>();
                t = rep.Find("Where DemoMode <> 1").FirstOrDefault();
            }
            return t;
        }

        public void UpdateSettings(Setting t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Setting>();
                rep.Update(t);
            }
        }

    }
}
