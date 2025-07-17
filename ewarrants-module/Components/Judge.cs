using System;
using System.Collections.Generic;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Portals;
using System.Linq;

namespace tjc.Modules.eWarrants.Components
{
    [TableName("tjc_warrant_judges")]
    //setup the primary key for table
    [PrimaryKey("JudgeID", AutoIncrement = false)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    [Scope("ModuleID")]
   public class Judge
    {
        public int JudgeID { get; set; }
        public int ModuleID { get; set; }
        public int SignatureFileID { get; set; }
        public int InitialFileID { get; set; }
        public string ApprovalKey { get; set; }
        public bool  TouchEnabledViewer { get; set; }
        public DateTime DayStart { get; set; }
        public DateTime DayEnd { get; set; }
        [IgnoreColumn]
        public bool HasSignature { get { return SignatureFileID > 0; } }
        [IgnoreColumn]
        public bool HasInitial { get { return InitialFileID > 0; } }
        [IgnoreColumn]
        public UserInfo User
        {
            get
            {
                UserInfo user = UserController.GetUserById(PortalSettings.Current.PortalId, JudgeID);
                if (user != null)
                {
                    return user;
                }
                else
                {
                    return new UserInfo();
                }
            }

        }
        [IgnoreColumn]
        public string JudgeName { get { return User.DisplayName; } }
        [IgnoreColumn]
        public Atalasoft.Annotate.AnnotationImage Initial
        {
            get
            {
                return GetImage(InitialFileID);
            }
        }
        [IgnoreColumn]
        public Atalasoft.Annotate.AnnotationImage Signature
        {
            get
            {
                return GetImage(SignatureFileID);
            }
        }
        [IgnoreColumn]
        public IEnumerable<County> Counties
        {
            get
            {
                var ctl = new JudgeController();
                return ctl.GetCountiesByJudge(JudgeID);
            }
        }
        [IgnoreColumn]
        public IEnumerable<JudgeType> JudgeTypes
        {
            get
            {
                var ctl = new JudgeController();
                return ctl.GetJudgeTypesByJudge(JudgeID);
            }
        }
        [IgnoreColumn]
        public string CountyList
        {
            get
            {
                string[] counties = Counties.Select(c => c.CountyName).ToArray();
                return string.Join(", ", counties);
            }
        }
        [IgnoreColumn]
        public string JudgeTypeList
        {
            get
            {
                string[] judgeTypes = JudgeTypes.Select(jt => jt.JudgeTypeName).ToArray();
                return string.Join(", ", judgeTypes);
            }
        }
        private Atalasoft.Annotate.AnnotationImage GetImage(int fileId)
        {
            var ctl = new FileController();
            var encypt = new Encryptor();
            File initial = ctl.GetFile(fileId);
            if (initial != null)
            {
                Byte[] buffer = encypt.DecryptStream(initial.Bytes);
                return new Atalasoft.Annotate.AnnotationImage(new System.IO.MemoryStream(buffer));
            }
            else
            {
                return new Atalasoft.Annotate.AnnotationImage();
            }
        }
    }

}
