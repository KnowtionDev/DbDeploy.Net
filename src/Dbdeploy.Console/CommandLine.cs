using System;
using System.IO;
using Net.Sf.Dbdeploy.Configuration;
using Net.Sf.Dbdeploy.Database;
using Net.Sf.Dbdeploy.Exceptions;

namespace Net.Sf.Dbdeploy
{
    public class CommandLine
    {
        public static void Main(string[] args)
        {
            try
            {
                IConfiguration config = new ConfigurationFile();
                DbmsFactory factory = new DbmsFactory(config.DbType, config.DbConnectionString);
                DatabaseSchemaVersionManager databaseSchemaVersion = new DatabaseSchemaVersionManager(factory, config.DbDeltaSet, config.CurrentDbVersion, config.TableName);


                var dir = new DirectoryInfo(".");
                var syntax = factory.CreateDbmsSyntax();
                //var output = Console.Out;

                using (var output = new StreamWriter(@".\output.txt", false, System.Text.Encoding.UTF8))
                {
                    var deployer = new ToPrintStreamDeployer(databaseSchemaVersion, dir, output, syntax, config.UseTransaction, null);
                    deployer.DoDeploy(Int64.MaxValue);
                }
            }
            catch (DbDeployException ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(1);
            }

            catch (Exception ex)
            {
                Console.Error.WriteLine("Failed to apply changes: " + ex);
                Console.Error.WriteLine(ex.StackTrace);
                Environment.Exit(2);
            }
        }
    }
}