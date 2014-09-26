using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HonkTech
{
    class Program
    {

        static void Main(string[] args)
        {

            string DataBase = "Onyx";
            string ConnectionString = "Data Source=(local);Initial Catalog=Onyx;Persist Security Info=True;Trusted_Connection=true;MultipleActiveResultSets=True";
            string DirectoryPath = "";

            SqlConnectionStringBuilder CSBuilder = new SqlConnectionStringBuilder(ConnectionString);

            String InitialCatalog = CSBuilder.InitialCatalog;
            CSBuilder.InitialCatalog = "master";

            DropDatabase(CSBuilder.ConnectionString, DataBase);
            CreateDatabase(CSBuilder.ConnectionString, DataBase);

            CSBuilder.InitialCatalog = InitialCatalog;
            CreateTables(CSBuilder.ConnectionString, DirectoryPath, DataBase);
            CreateViews(CSBuilder.ConnectionString, DirectoryPath, DataBase);
            LoadDefaultData(CSBuilder.ConnectionString, DirectoryPath, DataBase);

        }

        static void DropDatabase(string ConnectionString, string Database)
        {

            String DBCountCommand = string.Format("select count(*) from sys.databases where name = '{0}'", Database);

            if (Convert.ToInt32(ExecuteScalar(ConnectionString, DBCountCommand)) > 0)
            {
                Console.WriteLine(string.Format("Database ({0}) exists.", Database));
                Console.WriteLine("Dropping the database....");
                ExecuteStatement(ConnectionString, string.Format("ALTER DATABASE {0} SET SINGLE_USER WITH ROLLBACK IMMEDIATE", Database));
                ExecuteStatement(ConnectionString, string.Format("DROP DATABASE {0}", Database));
                Console.WriteLine("Dropped.");
            }

        }
        static void CreateDatabase(string ConnectionString, string Database)
        {

            Console.Write(string.Format("Creating database ({0})....", Database));
            try
            {
                ExecuteStatement(ConnectionString, string.Format("create database {0}", Database));                
            }
            catch (SqlException e)
            {
                Console.WriteLine(e.ToString());
                throw;
            }
            Console.WriteLine("Database Created.");

        }
        static void CreateTables(string ConnectionString, string DirectoryPath, string Database)
        {

            foreach (String TableFile in Directory.GetFiles(@"Tables\", "*.sql"))
            {

                Console.WriteLine(string.Format("Creating table {0}...", TableFile.Split('.').First()));
                try
                {
                    string CreateTableCommand = File.ReadAllText(TableFile);
                    ExecuteStatement(ConnectionString, CreateTableCommand);
                }
                catch (Exception)
                {
                    Console.WriteLine(string.Format("Error with table create command : {0}", TableFile));
                    throw;
                }

            }

        }
        static void CreateViews(string ConnectionString, string DirectoryPath, string Database)
        {

            foreach (String ViewFile in Directory.GetFiles(@"Views\", "*.sql"))
            {

                Console.WriteLine(string.Format("Creating View {0}...", ViewFile.Split('.').First()));
                try
                {
                    string CreateViewCommand = File.ReadAllText(ViewFile);
                    ExecuteStatement(ConnectionString, CreateViewCommand);
                }
                catch (Exception)
                {
                    Console.WriteLine(string.Format("Error with view create command : {0}", ViewFile));
                    throw;
                }

            }

        }
        static void LoadDefaultData(string ConnectionString, string DirectoryPath, string Databsae)
        {

            Console.WriteLine(String.Format("Starting bulk copy with {0} degrees of parallelism", 1));

            foreach (String BCPName in Directory.GetFiles(@"DefaultData\", "*.bcp"))
            {

                try
                {

                    string FilePath = Path.GetFullPath(BCPName);
                    string TableName = BCPName.Split('.').First();

                    if (TableName.Contains("_.bcp")) TableName = TableName.Replace("_", "");
                        Console.WriteLine("Starting bulk copy for " + TableName);
                    
                    TableName = TableName.Replace("DefaultData\\", "");

                    String BulkLoadCommand = String.Format("BULK INSERT {0}.dbo.{1} from '{2}' with (FIRSTROW = 2, KEEPIDENTITY)", Databsae, TableName, FilePath);
                    //String BulkLoadCommand = String.Format("BULK INSERT {0}.dbo.{1} from '{2}' with (DATAFILETYPE='widechar', KEEPIDENTITY)", Databsae, TableName, FilePath);
                    ExecuteStatement(ConnectionString, BulkLoadCommand);
                    
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Done Loading: " + TableName);
                    Console.ResetColor();

                }

                catch (Exception ex)
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERROR : Unable to load " + BCPName);
                    Console.WriteLine(ex);
                    Console.ResetColor();

                }

            }

        }
        static void ExecuteStatement(string ConnectionString, string Statement)
        {

            using (SqlConnection Connection = new SqlConnection(ConnectionString))
            {

                Connection.Open();
                using (SqlCommand Command = new SqlCommand(Statement, Connection))
                {

                    Command.CommandTimeout = 1200; // two minutes.
                    Command.ExecuteNonQuery();

                }

            }

        }
        static object ExecuteScalar(string ConnectionString, string Statement)
        {
            using (SqlConnection Connection = new SqlConnection(ConnectionString))
            {
                Connection.Open();
                using (SqlCommand Command = new SqlCommand(Statement, Connection))
                {
                    Command.CommandTimeout = 1200; // two minutes.
                    return Command.ExecuteScalar();
                }
            }
        }

    }

}
