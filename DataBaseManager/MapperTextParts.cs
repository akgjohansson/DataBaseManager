using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseManager
{
    public class MapperTextParts
    {

        public string EndOfMapper { get; set; }
        public MapperTextParts()
        {
            EndOfMapper = $"            }});{Environment.NewLine}";
        }
        public string BeginningOfMapper(string className) {
            return $"            _modelMapper.Class<{className}>(e =>            {{{Environment.NewLine}";
        }

        

        public string Id(string className) {
            return $"                e.Id(p => p.{className}Id, p => p.Generator(Generators.GuidComb));{Environment.NewLine}";
        }
        
        public string Properties(string[] properties)
        {
            string output = "";
            foreach (string property in properties)
            {
                output += $"                e.Property(p => p.{property});{Environment.NewLine}";
            }
            return output;
        }

        public string OneToMany(string tableName, string foreignTableName)
        {
            return $"                e.Set(p => p.{foreignTableName}, p =>{Environment.NewLine}" +
                $"                {{{Environment.NewLine}" +
                $"                    p.Inverse(true);{Environment.NewLine}" +
                $"                    p.Cascade(Cascade.All);{Environment.NewLine}" +
                $"                    p.Key(k => k.Column(col => col.Name(\"{tableName}Id\")));{Environment.NewLine}" +
                $"                }}, p => p.OneToMany());{Environment.NewLine}";
        }

        public string ManyToOne(string tableName, string foreignTableName)
        {
            return $"                e.ManyToOne(p => p.{foreignTableName}, mapper =>{Environment.NewLine}" +
                $"               {{{Environment.NewLine}" +
                $"                   mapper.Column(\"{foreignTableName}Id\");{Environment.NewLine}" +
                $"                   mapper.NotNullable(true);{Environment.NewLine}" +
                $"                   mapper.Cascade(Cascade.None);{Environment.NewLine}" +
                $"               }});";
        }
        public string ManyToMany(string tableName, string foreignTableName , bool firstToMention)
        {
            string inverseText = "";
            if (firstToMention)
                inverseText = $"                    collectionMapping.Inverse(true);{Environment.NewLine}";

            return $"                e.Set( x => x.Customers  , collectionMapping =>{Environment.NewLine}" +
                $"                {{{Environment.NewLine}" +
                $"                    collectionMapping.Table(\"{tableName}{foreignTableName}\");{Environment.NewLine}" +
                $"                    collectionMapping.Cascade(Cascade.None);{Environment.NewLine}" +
                $"                    collectionMapping.Key(keyMap => keyMap.Column(\"{tableName}Id\"));{Environment.NewLine}" +
                $"                }}, map => map.ManyToMany(p => {{{Environment.NewLine}" +
                $"                    p.Column(\"{foreignTableName}Id\");{Environment.NewLine}" +
                $"                    p.ForeignKey(\"FK_{foreignTableName}{tableName}_{foreignTableName}\");" +
                $"                    p.Class(typeof({foreignTableName}));{Environment.NewLine}" +
                $"                }}));{Environment.NewLine}";
        }

    }
}
