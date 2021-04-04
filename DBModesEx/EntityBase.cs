using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FileManage.DBModels
{
    public abstract class EntityBase
    {
        public abstract object getId();
        public abstract string getIdName();
        public abstract string getTableName();
    }
}
