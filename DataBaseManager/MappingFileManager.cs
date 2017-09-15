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
        private string FullPathToHere { get; set; }
        private string FullPathToMFB { get; set; }
        private MapperTextParts mapperText { get; set; }

        public MappingFileManager()
        {
            FullPathToHere = GetPathToHomeDir();
            FullPathToMFB = FullPathToHere + "\\MappingFileBuilders";
            BeginningOfFile = System.IO.File.ReadAllText($"{FullPathToHere}\\MappingFileBuilders\\beginningOfFile.txt");
            EndOfFile = $"    }}{Environment.NewLine}}};";
            mapperText = new MapperTextParts();
        }

        public void NewTable(string tableName, string[] properties)
        {
            string firstPart = "";
            firstPart += mapperText.BeginningOfMapper(tableName);
            firstPart += mapperText.Id(tableName);
            firstPart += mapperText.Properties(properties);

            string secondPart = "";
            secondPart += mapperText.EndOfMapper;
            SaveBothParts(firstPart, secondPart, tableName);
        }


        public void AddOneToMany(string tableName , string foreignTableName)
        {
            string secondPart1 = GetSecondPart(tableName);
            secondPart1 = mapperText.OneToMany(tableName, foreignTableName) + secondPart1;

            SaveSecondPart(secondPart1 , tableName);

            string secondPart2 = GetSecondPart(foreignTableName);
            secondPart2 = mapperText.ManyToOne(tableName, foreignTableName) + secondPart2;
            SaveSecondPart(secondPart2, foreignTableName);
        }
        public void AddManyToMany(string tableName , string foreignTableName)
        {
            string secondPart1 = GetSecondPart(tableName);
            secondPart1 = mapperText.ManyToMany(tableName, foreignTableName, true) +  secondPart1;
            SaveSecondPart(secondPart1, tableName);

            string secondPart2 = GetSecondPart(foreignTableName);
            secondPart2 = mapperText.ManyToMany(tableName, foreignTableName, false) + secondPart2;
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
            string[] files = GetAllTables();
            string[] tableNames = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                tableNames[i] = files[i].Substring(6, files[i].Length - 3);
            }
        }

        public string[] GetAllTables()
        {
            string[] allFiles = System.IO.Directory.GetFiles(FullPathToHere);
            string[] files = Validator.Selections.ReturnMatches(allFiles, "^tables*", false);
            string[] tables = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                tables[i] = files[i].Split('.')[0];
            }
            return Validator.Selections.RemoveDuplicates(tables);
        }

        /// <summary>
        /// Builds the Mapping file
        /// </summary>
        internal void Build()
        {

        }

        /// <summary>
        /// Adds newly created class file to project (in the .csproj file)
        /// </summary>
        /// <param name="fileName"></param>
        internal void AddFileToProject(string fileName)
        {
            string pathToCsprojFile = "DataBaseManager\\DataBaseManager.csproj";

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
