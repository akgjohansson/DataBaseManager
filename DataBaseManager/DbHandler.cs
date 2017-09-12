using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UserInput;

namespace DataBaseManager
{
    public class DbHandler
    {
        string Constr { get; set; }

        public DbHandler(string _Constr)
        {
            Constr = _Constr;
        }

        /// <summary>
        /// Creates a new table in the database. N.B. do not include key columns in argument list!
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns>True if success, false if not (without any effect on database)</returns>
        public bool CreateTable(string parent, string tableName, string[] columns, SqlDbType[] dataType , string[] additionalColumnsOptions)
        {
            return CreateTableMethod(parent, tableName, columns, dataType, additionalColumnsOptions);
        }
        /// <summary>
        /// Creates a new table in the database. N.B. do not include key columns in argument list!
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="tableName"></param>
        /// <param name="columns"></param>
        /// <returns>True if success, false if not (without any effect on database)</returns>
        public bool CreateTable(string parent , string tableName , string[] columns , SqlDbType[] dataType)
        {
            string[] additionalOptions = new string[dataType.Length];
            for (int i = 0; i < dataType.Length; i++)
            {
                additionalOptions[i] = "not null";
            }
            return CreateTableMethod(parent, tableName, columns, dataType , new string[dataType.Length]);
        }

        
        private bool CreateTableMethod(string parent , string tableName , string[] columns , SqlDbType[] datatypes , string[] additionalColumnOptions)
        {
            if (columns.Length != datatypes.Length)
            {
                Console.WriteLine("Columns and datatypes arrays do not have the same length!");
                return false;
            }
            if (!IsTableExisting(parent))
            {
                Console.WriteLine($"Table {parent} does not exist!");
                return false;
            }

            string foreignKey = $"FK_{parent}_{tableName}";
            string columnsText = $"Id int not null identity primary key, {parent}Id int not null";
            for (int i = 0; i < columns.Length; i++)
            {
                columnsText += $",{columns[i]} {datatypes[i]} {additionalColumnOptions[i]}";
            }
            

            string command = $"create table {tableName} ({columnsText},constraint {foreignKey} foreign key ({parent}Id) references {parent}(Id))";
            using (SqlConnection con = new SqlConnection(Constr))
            using (SqlCommand com = new SqlCommand(command, con))
            {
                con.Open();
                com.ExecuteNonQuery();
            }
            return true;

        }

        public void CreateProduct()
        {

        }

        public void RemoveProduct()
        {

        }

        public void ResetDatabase(string filePath)
        {

        }

        public void RemoveTable(string table , bool deleteThisTableToo)
        {
            string[] children = FindForeignTables(table);
            if (children.Length != 0)
            {
                Console.WriteLine($"Removing table {table} will also remove these tables:");
                
                Console.WriteLine();
                children.ToList().ForEach(x => PrintTree(x));
                if (GetInputFromUser.GetString("Proceed? ('proceed'/[n]): ").ToLower() == "proceed")
                {
                    RemoveRecursively(table);
                }
            }
            else if (deleteThisTableToo)
            {
                DeleteThisTable(table);
            }
        }

        private void RemoveRecursively(string table)
        {
            string[] children = FindForeignTables(table);
            foreach (string child in children)
            {
                RemoveRecursively(child);
                DeleteThisTable(child);
            }
        }

        private void DeleteThisTable(string table)
        {
            using (SqlConnection con = new SqlConnection(Constr))
            {
                con.Open();
                string[] foreignKeys = GetForeignKeys(table, con);
                if (foreignKeys.Length != 0)
                {
                    foreach (string key in foreignKeys)
                    {
                        string command = $"alter table {table} drop constraint {key};";

                        using (SqlCommand com = new SqlCommand(command, con))
                        {
                            com.ExecuteNonQuery();
                        }
                    }
                }

                using (SqlCommand com = new SqlCommand($"drop table {table}" , con))
                {
                    com.ExecuteNonQuery();
                }

            }
        }
        /// <summary>
        /// Gets the foreign key(s) from a table and returns them as a string array. If table doesn not exist, an empty array is returned
        /// </summary>
        /// <param name="table"></param>
        /// <param name="con"></param>
        /// <returns></returns>
        private string[] GetForeignKeys(string table, SqlConnection con)
        {
            List<string> output = new List<string>();
            if (IsTableExisting(table))
            {
                string command = $"select sys.foreign_keys.name from sys.foreign_keys" +
                    $" inner join sys.tables on sys.foreign_keys.referenced_object_id = sys.tables.object_id" +
                    $" where tables.name = '{table}'; ";
                using (SqlCommand com = new SqlCommand(command, con))
                {
                    SqlDataReader reader = com.ExecuteReader();

                    while (reader.Read())
                    {
                        output.Add(reader.GetString(0));
                    }
                    reader.Close();
                }
            }
            return output.ToArray();
        }

        private void PrintTree(string parent)
        {
            List<List<string>> tree = ListTreeStructure(parent);
            foreach (List<string> branch in tree)
            {
                bool first = true;
                foreach (string item in branch)
                {
                    if (!first)
                        Console.WriteLine(" - ");
                    else
                        first = false;
                    Console.Write(item);
                }
                Console.Write(Environment.NewLine);
            }
        }

        public void AddEntry()
        {
            
        }

        public void ManageEntries()
        {

        }

        /// <summary>
        /// Lists the tree structure from the top
        /// </summary>
        public List<List<string>> ListTreeStructure()
        {
            List<List<string>> output = new List<List<string>>
            {
                new List<string>{"Products"}
            };
            ListTreeStructureMethod(output[0][0],output, new List<string> { "Products" });
            return output;
        }

        /// <summary>
        /// Lists the tree structure from the given table
        /// </summary>
        /// <param name="parent"></param>
        public List<List<string>> ListTreeStructure(string parent)
        {
            List<List<string>> output = new List<List<string>>
            {
                new List<string>{parent}
            };
            ListTreeStructureMethod(parent , output , new List<string> { parent });
            return output;
        }

        public List<string> ListAllTables()
        {
            List<string> printedCategories = new List<string>();
            List<List<string>> categories = this.ListTreeStructure();

            foreach (List<string> category in categories)
            {
                foreach (string item in category)
                {
                    if (!printedCategories.Contains(item))
                    {
                        printedCategories.Add(item);
                    }
                }

            }
            return printedCategories;
        }

        public bool CreateForeignKeyConstraint(string parent , string child)
        {
            if (IsTableExisting(parent) && IsTableExisting(child))
            {

                return true;
            }
            else
            {
                return false;
            }
            
        }

        private void ListTreeStructureMethod(string parent , List<List<string>> treeList, List<string> ancestorTableTree)
        {
            string[] children = FindForeignTables(parent);
            foreach (string tableName in children)
            {
                
                
                List<string> thisChildsAncestors = new List<string>();
                ancestorTableTree.ForEach(x => thisChildsAncestors.Add(x));
                thisChildsAncestors.Add(tableName);
                ListTreeStructureMethod(tableName, treeList , thisChildsAncestors);
                

                
            }
            if (children.Length == 0)
            {
                treeList.Add(new List<string>());
                int lastRow = treeList.Count - 1;
                ancestorTableTree.ForEach(x => treeList[lastRow].Add(x));
                //treeList[lastRow].Add(parent);
                
            }
        }

        public string[] ListColumnsFromTable(string table)
        {
            if (this.IsTableExisting(table))
            {
                List<string> output = new List<string>();
                using (SqlConnection con = new SqlConnection(Constr)) {
                    con.Open();
                    int objectId = GetObjectId(con,table);
                    string command = $"select name from sys.all_columns where sys.all_columns.object_id = {objectId}";
                    using (SqlCommand com = new SqlCommand(command, con))
                    {
                        SqlDataReader reader = com.ExecuteReader();
                        while (reader.Read())
                        {
                            output.Add(reader.GetString(0));
                        }
                        reader.Close();
                    }
                    
                }
                return output.ToArray();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Finds all children of a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns>an array of strings with the name of all children. If no children exist, length==0</returns>
        private string[] FindForeignTables(string table)
        {
            List<string> children = new List<string>();

            using (SqlConnection con = new SqlConnection(Constr)) {
                con.Open();
                int objectId = GetObjectId(con, table);
                string command = $"select sys.tables.name from sys.foreign_keys " +
                    $"inner join sys.tables on sys.tables.object_id = sys.foreign_keys.parent_object_id" +
                    $" where sys.foreign_keys.referenced_object_id = {objectId}";
                using (SqlCommand com = new SqlCommand(command, con))
                {
                    SqlDataReader reader = com.ExecuteReader();
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            children.Add(reader.GetString(i));

                        }
                    }
                    reader.Close();
                }
            }
            return children.ToArray();
        }

        /// <summary>
        /// Finds the parent of a table
        /// </summary>
        /// <param name="table"></param>
        /// <returns>Name of table if parent exist, otherwise ""</returns>
        private string FindParentTable(string table)
        {
            using (SqlConnection connection = new SqlConnection(Constr))
            {
                connection.Open();
                int objectId = GetObjectId(connection, table);
                string command = $"select sys.tables.name from sys.tables " +
                    $"inner join sys.foreign_keys on sys.tables.object_id = sys.foreign_keys.parent_object_id " +
                    $"where sys.foreign_keys.referenced_object_id = { objectId} ";
                using (SqlCommand com = new SqlCommand(command, connection))
                {
                    SqlDataReader reader = com.ExecuteReader();
                    if (reader.Read())
                    {
                        string output = reader.GetString(0);
                        reader.Close();
                        return output;
                    }
                    else
                    {
                        reader.Close();
                        return "";
                    }
                }
            }
        }

        /// <summary>
        /// Finds out if a table exists or not
        /// </summary>
        /// <param name="table"></param>
        /// <returns></returns>
        public bool IsTableExisting(string table)
        {
            string command = $"select name from sys.tables";
            using (SqlConnection con = new SqlConnection(Constr))
            using (SqlCommand com = new SqlCommand(command, con))
            {
                con.Open();
                SqlDataReader reader = com.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.GetString(0).ToLower() == table.ToLower())
                        return true;
                }
                reader.Close();
            }
            return false;
        }

        /// <summary>
        /// Returns the object_id of the table. If table does not exist, the return is 0
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private int GetObjectId(SqlConnection connection , string table)
        {
            if (IsTableExisting(table))
            {
                string command = $"select object_id from sys.tables where name = '{table}'";
                using (SqlCommand com = new SqlCommand(command, connection))
                {
                    SqlDataReader reader = com.ExecuteReader();
                    reader.Read();
                    int output = reader.GetInt32(0);
                    reader.Close();
                    return output;
                }
            }
            else
            {
                return 0;
            }
        }
    }
}
