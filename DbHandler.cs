using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


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
        public bool CreateTable(string parent , string tableName , string[] columns , string[] datatypes)
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
            string columnsText = $"Id int not null identity primary key, {parent}Id int";
            for (int i = 0; i < columns.Length; i++)
            {
                columnsText += $",{columns[i]} {datatypes[i]}";
            }
            

            string command = $"create table {tableName} ({columnsText},constraint {foreignKey} foreign key ({parent}Id) references {parent}({parent}Id))";
            using (SqlConnection con = new SqlConnection(Constr))
            using (SqlCommand com = new SqlCommand(command, con))
            {
                com.ExecuteNonQuery();
            }
            return true;

        }

        

        public void ResetDatabase(string filePath)
        {

        }

        public void RemoveTable(string table)
        {
            string[] children = FindForeignTables(table);
            if (children.Length != 0)
            {
                GetInputFromUser.GetString() Console.WriteLine();
                children.ToList().ForEach(x => PrintTree(x));
            }
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
                [0] = new List<string>()
            };
            ListTreeStructureMethod("Products" , output);
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
                [0] = new List<string>()
            };
            ListTreeStructureMethod(parent , output);
            return output;
        }

        private void ListTreeStructureMethod(string parent , List<List<string>> treeList)
        {
            string[] children = FindForeignTables(parent);
            foreach (string tableName in children)
            {
                int lastRow = treeList.Count-1;
                treeList[lastRow].Add(tableName);
                ListTreeStructureMethod(tableName, treeList);

                if (treeList[lastRow].Count != 0)
                    treeList.Add(new List<string>());
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
                int objectId = GetObjectId(con, table);
                string command = $"select sys.tables.name from sys.foreign_keys " +
                    $"inner join sys.tables on sys.tables.object_id = sys.foreign_keys.referenced_object_id" +
                    $"where sys.foreign_keys.parent_object_id = {objectId}";
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
        private bool IsTableExisting(string table)
        {
            string command = $"select * from sys.tables";
            using (SqlConnection con = new SqlConnection(Constr))
            using (SqlCommand com = new SqlCommand(command, con))
            {
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
