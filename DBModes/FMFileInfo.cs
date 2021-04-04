using System;

namespace FileManage.DBModels
{
    public partial class FMFileInfo
    {
        public Guid FMFileInfoId { get; set; }
        public string Path{get;set;}
        public string Name{get;set;}
        
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }
    }
}
