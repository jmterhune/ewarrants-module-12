using DotNetNuke.ComponentModel.DataAnnotations;
using System;

namespace tjc.Modules.eWarrants.ClerkInterface
{
    [TableName("vw_portal_warrants_all")]
    //setup the primary key for table
    [PrimaryKey("WarrantID", AutoIncrement = false)]
    public class PortalWarrant
    {
        public int WarrantID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public DateTime? ReviewedByAgencyDate { get; set; }

        public string Title { get; set; }
        public string Defendant { get; set; }
        public int WarrantType { get; set; }
        public string WarrantTypeName { get; set; }
        public string ReviewedJudgeName { get; set; }
        public string AssignedJudgeName { get; set; }
        public string AgencyName { get; set; }
        public string LeoName { get; set; }
        public string LeoEmail { get; set; }
        public string StateAttorney { get; set; }
        public string CountyName { get; set; }
        public int FileType { get; set; }
        public string FileTypeName { get; set; }
        public int PortalStatus { get; set; }
        public bool IsJuvenile { get; set; }
    }
    [TableName("vw_signed_warrants")]
    //setup the primary key for table
    [PrimaryKey("WarrantID", AutoIncrement = false)]
    public class SignedWarrant
    {
        public int WarrantID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ReviewedDate { get; set; }
        public DateTime? ReviewedByAgencyDate { get; set; }
        public string Title { get; set; }
        public string Defendant { get; set; }
        public int WarrantType { get; set; }
        public string WarrantTypeName { get; set; }
        public string ReviewedJudgeName { get; set; }
        public string AssignedJudgeName { get; set; }
        public string AgencyName { get; set; }
        public int FileID { get; set; }
        public string LeoName { get; set; }
        public string LeoEmail { get; set; }
        public string StateAttorney { get; set; }
        public string CountyName { get; set; }
        public int FileType { get; set; }
        public string FileTypeName { get; set; }
        public int PortalStatus { get; set; }
        public bool IsJuvenile { get; set; }
    }
    public class WarrantFile
    {
        public byte[] Bytes { get; set; }
    }
}