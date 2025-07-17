using System;
using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_log")]
    //setup the primary key for table
    [PrimaryKey("LogID", AutoIncrement = true)]
    class Log
    {
        public int LogID { get; set; }
        public int WarrantID { get; set; }
        public DateTime EventTime { get; set; }
        public string EventDescription { get; set; }
        public int UserID { get; set; }
        public string IP { get; set; }

    }
}
