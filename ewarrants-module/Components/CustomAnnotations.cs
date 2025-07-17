using DotNetNuke.ComponentModel.DataAnnotations;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_annotations")]
    //setup the primary key for table
    [PrimaryKey("AnnotationID", AutoIncrement = true)]
    //configure caching using PetaPoco
    class CustomAnnotation
    {
        public int AnnotationID { get; set; }
        public string AnnotationText { get; set; }
        public int UserID { get; set; }
    }
}
