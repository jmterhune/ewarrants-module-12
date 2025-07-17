using System.Collections.Generic;
using System.Linq;
using DotNetNuke.Data;

namespace tjc.Modules.eWarrants.Components
{
    class AttachmentController
    {
        public void CreateAttachment(Attachment t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Attachment>();
                rep.Insert(t);
            }
        }

        public void DeleteAttachment(int attachmentId)
        {
            var t = GetAttachment(attachmentId);
            DeleteAttachment(t);
        }

        public void DeleteAttachment(Attachment t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Attachment>();
                rep.Delete(t);
            }
        }
        public IEnumerable<Attachment> GetOrphanedAttachments()
        {
            IEnumerable<Attachment> attachments;
            using (IDataContext ctx = DataContext.Instance())
            {
                attachments = ctx.ExecuteQuery<Attachment>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_orphaned_attachments");
            }
            return attachments;
        }

        public void CleanupAttachments()
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_cleanup_attachments");
            }
        }
        public IEnumerable<Attachment> GetAttachments()
        {
            IEnumerable<Attachment> t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Attachment>();
                t = rep.Get();
            }
            return t;
        }

        public Attachment GetAttachment(int attachmentId)
        {
            Attachment t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Attachment>();
                t = rep.GetById(attachmentId);
            }
            return t;
        }

        public void UpdateAttachment(Attachment t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<Attachment>();
                rep.Update(t);
            }
        }

        #region Attachment by Warrant xref
        public IEnumerable<Attachment> GetAttachmentsByWarrant(int warrantId)
        {
            IEnumerable<Attachment> attachments;
            using (IDataContext ctx = DataContext.Instance())
            {
                attachments = ctx.ExecuteQuery<Attachment>(System.Data.CommandType.StoredProcedure, "tjc_warrant_get_attachments_by_Warrant", warrantId);
            }
            return attachments;
        }

        public AttachmentWarrantXref GetAttachmentWarrantXref(int fileId, int warrantId)
        {
            AttachmentWarrantXref t;
            using (IDataContext ctx = DataContext.Instance())
            {
                var rep = ctx.GetRepository<AttachmentWarrantXref>();
                t = rep.Find("Where FileID = @0 And WarrantID = @1", fileId, warrantId).FirstOrDefault();
            }
            return t;
        }

        public void CreateAttachmentWarrantXref(AttachmentWarrantXref t)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_create_attachment_warrant_xref", t.WarrantID, t.FileID);
            }
        }
        public void DeleteAttachmentWarrantXref(int warrantId,int fileId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_delete_attachment_warrant_xref", warrantId,fileId);
            }
        }
        public void DeleteAttachmentsByWarrant(int warrantId)
        {
            using (IDataContext ctx = DataContext.Instance())
            {
                ctx.Execute(System.Data.CommandType.StoredProcedure, "tjc_warrant_delete_attachments_by_warrant", warrantId);
            }
        }
        #endregion

    }
}
