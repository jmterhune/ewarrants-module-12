using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Scheduling;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace tjc.Modules.eWarrants.Components
{
    public class ScheduleAttachmentCleanup : SchedulerClient
    {
        public ScheduleAttachmentCleanup(ScheduleHistoryItem oItem) : base()
        {
            this.ScheduleHistoryItem = oItem;
        }

        public override void DoWork()
        {
            try
            {

                // Perform required items for logging
                this.Progressing();
                int count = 0;
                var ctl = new AttachmentController();
                
                IEnumerable<Attachment> attachments = ctl.GetOrphanedAttachments();
                foreach (Attachment attachment in attachments)
                {
                    try
                    {
                        FileInfo f = new FileInfo(attachment.FileName);
                        if (f.Exists)
                        {
                            f.Delete();
                            count += 1;
                            ctl.DeleteAttachment(attachment);
                        }
                    }
                    catch (Exception exc)
                    {
                        Exceptions.LogException(exc);
                    }
                }
                ctl.CleanupAttachments();

                // To log note
                this.ScheduleHistoryItem.AddLogNote(count.ToString() + " Orphaned Attachments Deleted");
                // Show success
                this.ScheduleHistoryItem.Succeeded = true;
            }

            catch (Exception ex)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote("Exception= " + ex.ToString());
                this.Errored(ref ex);
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }
    }

}