using System;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_files")]
    //setup the primary key for table
    [PrimaryKey("FileID", AutoIncrement = true)]
    public class File
    {
        public int FileID { get; set; }
        public int UploadedBy { get; set; }
        public DateTime UploadedDate { get; set; }
        public bool Persist { get; set; }
        //public int DocTypeID { get; set; }
        public FileType FileType { get; set; }
        public string Path { get; set; }
        public byte[] Bytes { get; set; }
        public byte[] OldBytes { get; set; }
        [IgnoreColumn]
        public string Extension
        {
            get
            {
                switch (FileType)
                {
                    case FileType.tiff:
                        return "tif";
                    case FileType.pdf:
                        return "pdf";
                    case FileType.nonwarrant:
                        return "jpg";
                    default:
                        break;
                }
                return "padf";
            }
        }

    }
    public enum FileType
    {
        tiff = 1,
        pdf = 2,
        nonwarrant = 0,
    }
}
