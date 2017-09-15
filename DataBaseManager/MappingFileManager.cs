using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace DataBaseManager
{
    public class MappingFileManager
    {
        private string BeginningOfFile { get; set; }
        private string EndOfFile { get; set; }
        public string FullPathToHere { get; set; }
        private string FullPathToMFB { get; set; }
        private MapperTextParts MapperText { get; set; }

        public MappingFileManager()
        {
            FullPathToHere = GetPathToHomeDir();
            FullPathToMFB = FullPathToHere + "\\MappingFileBuilders";
            BeginningOfFile = System.IO.File.ReadAllText($"{FullPathToHere}\\MappingFileBuilders\\beginningOfFile.txt");
            EndOfFile = $"    }}{Environment.NewLine}}};";
            MapperText = new MapperTextParts();
        }

        public void NewTables(string tableName, string[] properties , Type[] types)
        {
            string firstPart = "";
            firstPart += MapperText.BeginningOfMapper(tableName);
            firstPart += MapperText.Id(tableName);
            firstPart += MapperText.Properties(properties);

            string secondPart = "";
            secondPart += MapperText.EndOfMapper;
            SaveBothParts(firstPart, secondPart, tableName);
            ClassFileManager cfm = new ClassFileManager(FullPathToHere);
            cfm.CreateClass(tableName, properties , types);
            cfm.Make();
        }

        

        public void AddOneToMany(string tableName , string foreignTableName)
        {
            string secondPart1 = GetSecondPart(tableName);
            secondPart1 = MapperText.OneToMany(tableName, foreignTableName) + secondPart1;

            SaveSecondPart(secondPart1 , tableName);

            string secondPart2 = GetSecondPart(foreignTableName);
            secondPart2 = MapperText.ManyToOne(tableName, foreignTableName) + secondPart2;
            SaveSecondPart(secondPart2, foreignTableName);
        }

        public void AddManyToMany(string tableName , string foreignTableName)
        {
            string secondPart1 = GetSecondPart(tableName);
            secondPart1 = MapperText.ManyToMany(tableName, foreignTableName, true) +  secondPart1;
            SaveSecondPart(secondPart1, tableName);

            string secondPart2 = GetSecondPart(foreignTableName);
            secondPart2 = MapperText.ManyToMany(tableName, foreignTableName, false) + secondPart2;
            SaveSecondPart(secondPart2, foreignTableName);
        }

        private void SaveBothParts(string firstPart, string secondPart , string tableName)
        {
            SaveFirstPart(firstPart, tableName);
            SaveSecondPart(secondPart, tableName);
        }

        private void SaveFirstPart(string firstPart , string tableName)
        {
            System.IO.File.WriteAllText($"{FullPathToMFB}\\table{tableName}.beginning.txt", firstPart);
        }

        public void SaveSecondPart(string secondPart , string tableName)
        {
            System.IO.File.WriteAllText($"{FullPathToMFB}\\table{tableName}.end.txt", secondPart);
        }

        private string GetFirstPart(string tableName)
        {
            string filePath = $"{FullPathToMFB}\\table{tableName}.beginning.txt";
            if (File.Exists(filePath))
            {
                return System.IO.File.ReadAllText(filePath);
            }
            else
            {
                return null;
            }
        }

        private string GetSecondPart(string tableName)
        {
            string filePath = $"{FullPathToMFB}\\table{tableName}.end.txt";
            if (File.Exists(filePath))
            {
                return System.IO.File.ReadAllText(filePath);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///  Removes all tables
        /// </summary>
        public void TabulaRasa()
        {
            string[] fileNames = Validator.Selections.ReturnMatches(Directory.GetFiles(FullPathToMFB),"^table+",true);
            foreach (string file in fileNames)
            {
                File.Delete(file);
            }
            Build();
            Make();
        }
        /// <summary>
        /// Builds the database
        /// </summary>
        private void Make()
        {
            
        }

        public string[] GetAllTables()
        {
            string[] allFiles = System.IO.Directory.GetFiles(FullPathToMFB);
            for (int i = 0; i < allFiles.Length; i++)
            {
                string[] splitFilePath = allFiles[i].Split('\\');
                allFiles[i] = splitFilePath[splitFilePath.Length - 1];
            }
            string[] files = Validator.Selections.ReturnMatches(allFiles, "^table+", false);
            string[] tables = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                tables[i] = files[i].Split('.')[0].Substring(5);
            }
            return Validator.Selections.RemoveDuplicates(tables);
        }

        /// <summary>
        /// Builds the Mapping file
        /// </summary>
        public void Build()
        {
            string[] tables = GetAllTables();
            string output = "";
            output += BeginningOfFile;
            foreach (string table in tables)
            {
                output += GetFirstPart(table);
                output += GetSecondPart(table);
            }
            File.WriteAllText($"{FullPathToMFB}\\mappings.cs",output);
        }

        

        private static string GetPathToHomeDir()
        {
            string fullPathHere = System.IO.Path.GetFullPath(".");
            int safetyCounter = 0;
            while (!System.IO.Directory.Exists($"{fullPathHere}\\DataBaseManager"))
            {
                fullPathHere = System.IO.Path.GetFullPath($"{fullPathHere}\\..");
                if (safetyCounter++ > 10)
                {
                    throw new Exception("Directory not found!");
                }
            }
            safetyCounter = 0;
            while (!System.IO.Directory.Exists($"{fullPathHere}\\MappingFileBuilders"))
            {
                fullPathHere += "\\DataBaseManager";
                if (safetyCounter++ > 10)
                {
                    throw new Exception("Directory MappingFileBuilders does not exist!");
                }
            }

            return fullPathHere;
        }
    }
}
