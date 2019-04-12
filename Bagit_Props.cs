using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BAGIT_FILE_TRANSFER
{

    class Bagit_Props
    {
         public void Bagit_getProp()
        {
        }

         public string HostName { get { return Utility.GetConfigValueByKey("HostName"); } }
        public string SFTPUserName { get { return Utility.GetConfigValueByKey("SFTPUserName"); } }
        public string SFTPPassword { get { return Utility.GetConfigValueByKey("SFTPPassword"); } }
        public string SFTPPort { get { return Utility.GetConfigValueByKey("SFTPPort"); } }
        public string SFTPDirectory { get { return Utility.GetConfigValueByKey("SFTPDirectory"); } }
         public string PythonDirectory { get { return Utility.GetConfigValueByKey("PythonDirectory"); } }
         public string PythonExePath { get { return Utility.GetConfigValueByKey("PythonExePath"); } }
        public string SourceOrganization { get { return Utility.GetConfigValueByKey("SourceOrganization"); } }
        public string BagitDirectory { get { return Utility.GetConfigValueByKey("BagitDirectory"); } }
        public string ProcessDirectory { get { return Utility.GetConfigValueByKey("ProcessDirectory"); } }
        public string ProcessArchiveDirectory { get { return Utility.GetConfigValueByKey("ProcessArchiveDirectory"); } }
        public string SuccessBagMessage { get { return Utility.GetConfigValueByKey("SuccessBagMessage"); } }
        public string Template_bag { get { return Utility.GetConfigValueByKey("Template_bag"); } }
        public string Create_bag { get { return Utility.GetConfigValueByKey("Create_bag"); } }
        public string Validate_bag { get { return Utility.GetConfigValueByKey("Validate_bag"); } }
        public string BagitDirectoryName { get { return Utility.GetConfigValueByKey("BagitDirectoryName"); } }
        public string Profile_Json { get { return Utility.GetConfigValueByKey("Profile_Json"); } }
    }
}
