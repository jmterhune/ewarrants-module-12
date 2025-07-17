using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
    class CustomAnnotationController
    {
        public void CreateCustomAnnotation(CustomAnnotation t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<CustomAnnotation>();
                rep.Insert(t);
            }
        }

        public void DeleteCustomAnnotation(int customAnnotationId)
        {
            var t = GetCustomAnnotation(customAnnotationId);
            DeleteCustomAnnotation(t);
        }

        public void DeleteCustomAnnotation(CustomAnnotation t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<CustomAnnotation>();
                rep.Delete(t);
            }
        }

        public IEnumerable<CustomAnnotation> GetCustomAnnotations()
        {
            IEnumerable<CustomAnnotation> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<CustomAnnotation>();
                t = rep.Get();
            }
            return t;
        }

        public IEnumerable<CustomAnnotation> GetCustomAnnotationsByUser(int userId)
        {
            IEnumerable<CustomAnnotation> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<CustomAnnotation>();
                t = rep.Get().AsQueryable().Where(ca=>ca.UserID==userId);
            }
            return t;
        }

        public CustomAnnotation GetCustomAnnotation(int customAnnotationId)
        {
            CustomAnnotation t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<CustomAnnotation>();
                t = rep.GetById(customAnnotationId);
            }
            return t;
        }

        public void UpdateCustomAnnotation(CustomAnnotation t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<CustomAnnotation>();
                rep.Update(t);
            }
        }
    }
}
