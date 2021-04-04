using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManage.DBModels
{
    public partial class FMFileInfo : EntityBase
    {
        public override object getId()
        {
            return this.FMFileInfoId;
        }

        public override string getIdName()
        {
            return "fmfileinfoid";
        }

        public override string getTableName()
        {
            return "fmfileinfo";
        }
    }
}
