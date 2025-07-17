using System.Collections.Generic;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
    class FileController
    {
        public void CreateFile(File t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<File>();
                rep.Insert(t);
            }
        }

        public void DeleteFile(int fileId)
        {
            var t = GetFile(fileId);
            DeleteFile(t);
        }

        public void DeleteFile(File t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<File>();
                rep.Delete(t);
            }
        }

        public IEnumerable<File> GetFiles()
        {
            IEnumerable<File> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<File>();
                t = rep.Get();
            }
            return t;
        }

        public File GetFile(int fileId)
        {
            File t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<File>();
                t = rep.GetById(fileId);
            }
            return t;
        }

        public void UpdateFile(File t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<File>();
                rep.Update(t);
            }
        }
        public void UpdateFileBytes(int fileId,byte[] bytes)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_update_file", fileId, bytes);
            }
        }

    }
}
