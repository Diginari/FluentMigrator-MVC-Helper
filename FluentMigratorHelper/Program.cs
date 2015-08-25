using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FluentMigratorHelper
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", Directory.GetCurrentDirectory() + "/web.config");

            Start:
            var connectionString = GetConnectionString();
            var provider = GetProvider();
            var assembly = GetAssembly();

            Task:
            var task = GetTask();

            var steps = "";
            var version = "";

            if (task.Contains("rollback/"))
            {
                steps = task.Split('/')[1];
                task = task.Split('/')[0];
            }

            if (task.Contains("rollback:toversion/"))
            {
                version = task.Split('/')[1];
                task = task.Split('/')[0];
            }
            
            var arg = @"/conn """ + connectionString + @""" /provider """ + provider + @""" /assembly """ + assembly + @""" /task """ + task + @"""";

            if (version != string.Empty)
            {
                arg = arg + @" /version """ + version + @"""";
            }

            if (steps != string.Empty)
            {
                arg = arg + @" /steps """ + steps + @"""";
            }

            Console.WriteLine("");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(arg);
            Console.ResetColor();
            Console.WriteLine("");

            var proc = new Process();
            proc.StartInfo.FileName = "migrate.exe"; 
            proc.StartInfo.Arguments = arg;
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("");
            Console.WriteLine(proc.StandardOutput.ReadToEnd());
            Console.ResetColor();

            proc.WaitForExit();
            var exitCode = proc.ExitCode;
            proc.Close();

            Console.WriteLine("Would you like to execute another migration? (Y/N)");
            var response = Console.ReadLine() ?? string.Empty;

            if (response.ToLower() == "y")
            {
                Console.WriteLine("");
                Console.WriteLine("Would you like to use the same connection and assembly? (Y/N)");
                var responseTask = Console.ReadLine() ?? string.Empty;
                if (responseTask.ToLower() == "y")
                {
                    goto Task;
                }

                goto Start;
            }

            Environment.Exit(0);
        }

        public static string GetTask()
        {
            Start:
            Console.WriteLine("");
            Console.WriteLine("Please enter the task: ");
            var assembly = Console.ReadLine() ?? string.Empty;

            if (assembly.ToLower() == "quit")
                Environment.Exit(0);

            if (assembly.ToLower() == "?help" || assembly.ToLower() == "--help" || assembly.ToLower() == "?")
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("");
                Console.WriteLine("-  Enter the task");
                Console.WriteLine("-    migrate:up");
                Console.WriteLine("-    migrate (same as up)");
                Console.WriteLine("-    migrate:down");
                Console.WriteLine("-    rollback");
                Console.WriteLine("-    rollback:toversion");
                Console.WriteLine("-    rollback:all");
                Console.WriteLine("-    listmigrations");
                Console.ResetColor();

                goto Start;
            }

            switch (assembly.ToLower())
            {
                case "migrate:up":
                case "migrate":
                case "migrate:down":
                case "rollback:all":
                case "listmigrations":
                    return assembly.ToLower();

                case "rollback":

                    RollBack:
                    Console.WriteLine("");
                    Console.WriteLine("How many steps to rollback? ");
                    var steps = Console.ReadLine();
                    int step = 0;
                    int.TryParse(steps, out step);

                    if (step > 0)
                    {
                        return assembly.ToLower() + "/" + step;
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid step, please try again");
                    Console.ResetColor();

                    goto RollBack;

                case "rollback:toversion":
                    Version:
                    Console.WriteLine("");
                    Console.WriteLine("Which version? ");
                    var verions = Console.ReadLine();
                    int version = 0;
                    int.TryParse(verions, out version);

                    if (version > 0)
                    {
                        return assembly.ToLower() + "/" + version;
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid version, please try again");
                    Console.ResetColor();

                    goto Version;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid task, please try again");
                    Console.WriteLine("");

                    Console.ResetColor();

                    goto Start;
            }
        }

        private static string GetAssembly()
        {
            Start:
            Console.WriteLine("");
            Console.WriteLine("Please enter the assembly name: ");
            var assembly = Console.ReadLine() ?? string.Empty;

            if (assembly.ToLower() == "quit")
                Environment.Exit(0);

            if (assembly.ToLower() == "?help" || assembly.ToLower() == "--help" || assembly.ToLower() == "?")
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("");
                Console.WriteLine("-  Enter the assembly name that contains the migrations, including the .dll extension");
                Console.WriteLine("-  use 'dir' to get a list of assemblies");
                Console.ResetColor();

                goto Start;
            }

            if (assembly.ToLower() == "dir")
            {
                var filePaths = Directory.GetFiles(Directory.GetCurrentDirectory());
                for (var i = 0; i < filePaths.Length; ++i)
                {
                    var path = filePaths[i];
                    if (Path.GetExtension(path) == ".dll")
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGreen;
                        Console.WriteLine("-  " + System.IO.Path.GetFileName(path));
                        Console.ResetColor();
                    }
                }

                goto Start;
            }

            if (!File.Exists(Directory.GetCurrentDirectory() + "/" + assembly) || !assembly.Contains(".dll"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Assembly not found, please use 'dir' to get a list of assemblies");
                Console.ResetColor();

                goto Start;
            }

            return assembly;
        }

        private static string GetProvider()
        {
            Start:
            Console.WriteLine("");
            Console.WriteLine("Please enter the provider name: ");
            var provider = Console.ReadLine() ?? string.Empty;

            if (provider.ToLower() == "quit")
                Environment.Exit(0);

            if (provider.ToLower() == "?help" || provider.ToLower() == "--help" || provider.ToLower() == "?")
            {
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                Console.WriteLine("");
                Console.WriteLine("-  Enter the provider");
                Console.WriteLine("-    SqlServerCe");
                Console.WriteLine("-    SQLite");
                Console.WriteLine("-    SqlServer");
                Console.WriteLine("-    SqlServer2005");
                Console.WriteLine("-    SqlServer2008");
                Console.WriteLine("-    SqlServer2012");
                Console.WriteLine("-    SqlServer2014");
                Console.WriteLine("");
                Console.ResetColor();

                goto Start;
            }

            switch (provider.ToLower())
            {
                case "sqlserverce":
                case "sqlite":
                case "sqlserver":
                case "sqlserver2005":
                case "sqlserver2008":
                case "sqlserver2012":
                case "sqlserver2014":
                    return provider;

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid provider, please try again");
                    Console.WriteLine("");

                    Console.ResetColor();

                    goto Start;
            }
        }

        private static string GetConnectionString()
        {
            Start:

            var connectionStrings = new List<string>();
            foreach (ConnectionStringSettings c in System.Configuration.ConfigurationManager.ConnectionStrings)
            {
                connectionStrings.Add(c.Name);
            }
            
            if (connectionStrings.Count() > 1)
            {
                Console.WriteLine("Multiple connection strings found");
                Console.WriteLine("");

                foreach (var s in connectionStrings)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("-  " + s);
                    Console.ResetColor();
                }

                Console.WriteLine("");
                Console.WriteLine("Please enter your connection string name: ");
                var connectionString = Console.ReadLine() ?? string.Empty;

                if (connectionString.ToLower() == "quit")
                    Environment.Exit(0);

                if (connectionString.ToLower() == "?help" || connectionString.ToLower() == "--help" ||
                    connectionString.ToLower() == "?")
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("");
                    Console.WriteLine("-  Enter the name of the connection string in the config file");
                    Console.WriteLine("");
                    Console.ResetColor();

                    goto Start;
                }

                try
                {
                    connectionString = ConfigurationManager.ConnectionStrings[connectionString].ConnectionString;
                    return connectionString;
                }
                catch (NullReferenceException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid connection string, please try again");
                    Console.WriteLine("");

                    Console.ResetColor();

                    goto Start;
                }
            }
            else if (connectionStrings.Count() == 1)
            {
                return connectionStrings.FirstOrDefault();
            }
            else
            {
                throw new Exception("NO CONNECTIONSTRINGS FOUND");
            }
        }
    }
}