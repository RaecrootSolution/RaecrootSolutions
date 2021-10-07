using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ICSI_Library.Models
{
    public class DocumentReference
    {
        public Int32 ID { get; set; }
        public Int32 SCREEN_ID { get; set; }
        public Int32 REF_ID { get; set; }
        public Int32 DOC_MOD_TRANS_ID { get; set; }
        public string FILE_NAME_TX { get; set; }
        //public Stream FILE_BLB { get; set; }
        public byte[] FILE_BLB { get; set; }
        public string FULL_PATH { get; set; }
        public string FILE_TYPES_TX { get; set; }
        public string DOC_TYPES_TX { get; set; }
        public string UPLOADED_ON { get; set; }
        public bool DELETE_YN { get; set; }
        public bool DOWNLOAD_YN { get; set; }
        public bool VIEW_YN { get; set; }
        public bool isRemoved { get; set; }
        public bool isNew { get; set; }
        public bool isChanged { get; set; }
    }
}
