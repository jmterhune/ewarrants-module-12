using System.Web.Caching;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_attachments")]
    //setup the primary key for table
    [PrimaryKey("FileID", AutoIncrement = true)]
    //configure caching using PetaPoco
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    class Attachment
    {
        public int FileID { get; set; }
        public string FileName { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public int UserID { get; set; }
    }

    [TableName("tjc_warrant_attachments_by_warrant")]
    class AttachmentWarrantXref
    {
        public int FileID { get; set; }
        public int WarrantID { get; set; }

    }
}
