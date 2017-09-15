using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DataBaseManager
{
    public class ClassFileManager
    {
        public string Path { get; set; }
        public string BuildPath { get; set; }
        public string BeginningOfFile { get; set; }
        public string EndOfFile { get; set; }
        public string FilePrefix { get; set; } = "class";

        public ClassFileManager(string path)
        {
            Path = path;
            BuildPath = $"{Path}\\ClassFileBuilders";
            BeginningOfFile = File.ReadAllText($"{BuildPath}\\beginningOfFile.txt");
            EndOfFile = $"    }}{Environment.NewLine}}}";
        }


        public void Make()
        {
            string[] allClasses = GetAllClasses();
            foreach (string className in allClasses)
            {
                string firstPart = GetFirstPart(className);
                string secondPart = GetSecondPart(className);
                File.WriteAllText($"{Path}\\{FilePrefix}{className}.cs" , firstPart+secondPart);
                AddFileToProject(className);
            }
        }

        public void AddFileToProject(string className)
        {
            string pathToCsprojFile = $"{Path}\\DataBaseManager.csproj";
            string[] csprojLines = File.ReadAllLines(pathToCsprojFile);
            string[] stripped = Validator.Selections.ReturnMatches(csprojLines, "<Compile Include=", true);
            string newLine = $"    <Compile Include=\"{className}.cs\" />";

            Regex r = new Regex($"\"{className}.cs\"");
            var match = stripped.Where(x => r.IsMatch(x)).ToArray();

            if (match.Length == 0)
            {
                Regex r2 = new Regex("<Compile Include=");
                var firstLineOfClasses = Array.IndexOf(csprojLines, stripped[0])+1;
                List<string> csprojList = new List<string>();
                for (int i = 0; i < csprojLines.Length; i++)
                {
                    csprojList.Add(csprojLines[i]);
                    if (i == firstLineOfClasses)
                        csprojList.Add(newLine);
                }
                File.WriteAllText(pathToCsprojFile, "");
                File.WriteAllLines(pathToCsprojFile, csprojList.ToArray());
            }

        }

        public void CreateClass(string className, string[] properties , Type[] types , bool overwrite = true)
        {
            string firstPart = "";
            string secondPart;
            string[] allClasses = GetAllClasses();

            if (!overwrite && allClasses.Contains(className))
            {
                firstPart = GetFirstPart(className);
                secondPart = GetSecondPart(className);
            }
            else
            {
                firstPart += BeginningOfFile;
                firstPart += Properties(properties , types);
                secondPart = EndOfFile;
            }

            SaveBothParts(className, firstPart, secondPart);
            
        }

        public void AddOneToMany(string className, string foreignClassName)
        {
            string firstPart1 = GetFirstPart(className);
            firstPart1 += $"        public virtual {foreignClassName} {foreignClassName} {{ get; set; }}";
            SaveFirstPart(className, firstPart1);

            string firstPart2 = GetFirstPart(foreignClassName);
            firstPart2 += $"        public virtual ICollection<{className}> {className} {{ get; set; }}";
            SaveFirstPart(foreignClassName, firstPart2);
        }

        public void AddManyToOne(string className, string foreignClassName)
        {
            string firstPart1 = GetFirstPart(className);
            firstPart1 += $"        public virtual ICollection<{foreignClassName}> {foreignClassName} {{ get; set; }}";
            SaveFirstPart(className, firstPart1);

            string firstPart2 = GetFirstPart(foreignClassName);
            firstPart2 += $"        public virtual {className} {className} {{ get; set; }}";
            SaveFirstPart(foreignClassName, firstPart2);
        }

        public void AddManyToMany(string className, string foreignClassName)
        {
            string firstPart1 = GetFirstPart(className);
            firstPart1 += $"        public virtual ICollection<{foreignClassName}> {foreignClassName} {{ get; set; }}";
            SaveFirstPart(className, firstPart1);

            string firstPart2 = GetFirstPart(foreignClassName);
            firstPart2 += $"        public virtual ICollection<{className}> {className} {{ get; set; }}";
            SaveFirstPart(foreignClassName, firstPart2);
        }

        private void SaveBothParts(string className, string firstPart, string secondPart)
        {
            SaveFirstPart(className, firstPart);
            SaveSecondPart(className, firstPart);
        }

        private void SaveSecondPart(string className, string firstPart)
        {
            File.WriteAllText($"{BuildPath}\\{FilePrefix}{className}.end.txt", firstPart);
        }

        private void SaveFirstPart(string className, string firstPart)
        {
            File.WriteAllText($"{BuildPath}\\{FilePrefix}{className}.beginning.txt", firstPart);
        }

        private string GetSecondPart(string className)
        {
            string filePath = $"{Path}\\{FilePrefix}{className}.end.txt";
            if (File.Exists(filePath))
            {
                return System.IO.File.ReadAllText(filePath);
            }
            else
            {
                return null;
            }
        }

        private string GetFirstPart(string className)
        {
            string filePath = $"{Path}\\{FilePrefix}{className}.beginning.txt";
            if (File.Exists(filePath))
            {
                return System.IO.File.ReadAllText(filePath);
            }
            else
            {
                return null;
            }
        }

        private string[] GetAllClasses()
        {
            string[] allFiles = System.IO.Directory.GetFiles(Path);
            for (int i = 0; i < allFiles.Length; i++)
            {
                string[] splitFilePath = allFiles[i].Split('\\');
                allFiles[i] = splitFilePath[splitFilePath.Length - 1];
            }
            string[] files = Validator.Selections.ReturnMatches(allFiles, $"^{FilePrefix}+", false);
            string[] classes = new string[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                classes[i] = files[i].Split('.')[0].Substring(FilePrefix.Length);
            }
            return Validator.Selections.RemoveDuplicates(classes);
        }

        private string Properties(string[] properties, Type[] types)
        {
            string output = $"        public virtual Guid Id {{ get; set; }}";
            for (int i=0; i < properties.Length;i++)
            {
                output += $"        public virtual {types[i]} {properties[i]} {{ get; set; }}";
            }
            return output;
        }
    }
}
