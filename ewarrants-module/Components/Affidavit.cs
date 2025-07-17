using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tjc.Modules.eWarrants.Components
{
    public class Affidavit
    {
        public Affidavit()
        {
        }
        public Affidavit(string name, string agency, string id)
        {
            this.Name = name;
            this.Agency = agency;
            this.ID = id;
        }
        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }
        private string _agency;
        public string Agency
        {
            get
            {
                return _agency;
            }
            set
            {
                _agency = value;
            }
        }
        private string _id;
        public string ID
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
            }
        }
    }

}