using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseManager
{
    internal class MappingFileManager
    {
        public string BeginningOfFile { get; set; }
        public string EndOfFile { get; set; }

        public MappingFileManager()
        {
            Assembly.LoadFile("MappingFileBuilders\\beginningOfFile.txt");
            //BeginningOfFile = 
            EndOfFile = $"    }}{Environment.NewLine}}};";
        }
        void AddOneToMany()
        {

        }

        void AddManyToMany()
        {

        }

        
    }
}
