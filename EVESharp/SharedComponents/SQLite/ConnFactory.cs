/*
 * ---------------------------------------
 * User: duketwo
 * Date: 03.07.2018
 * Time: 12:30
 * ---------------------------------------
 */

/*
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyHook.IPC;
using ServiceStack;
using ServiceStack.OrmLite;
using SharedComponents.EVE;
using SharedComponents.EVE.DatabaseSchemas;

namespace SharedComponents.SQLite
{
    public class ConnFactory
    {
        public static readonly ConnFactory Instance = new ConnFactory();

        internal OrmLiteConnectionFactory Factory { get; private set; }

        static ConnFactory()
        {
            OrmLiteConfig.DialectProvider = SqliteDialect.Provider;
            using (var wc = WriteConn.Open())
            {

                // delete a table

                //if (wc.DB.TableExists<AbyssStatEntry>())
                //{
                //    wc.DB.DropTable<AbyssStatEntry>();
                //}

                // rename a table
                if (wc.DB.TableExists("StatisticsEntryCSV") && !wc.DB.TableExists("StatisticsEntry"))
                {
                    Console.WriteLine("Altering db table StatisticsEntryCSV name to StatisticsEntry.");
                    wc.DB.ExecuteSql("ALTER TABLE StatisticsEntryCSV RENAME TO StatisticsEntry");
                }

                //create tables
                wc.DB.CreateTableIfNotExists<StatisticsEntry>();
                wc.DB.CreateTableIfNotExists<AbyssStatEntry>();
                wc.DB.CreateTableIfNotExists<CachedWebsiteEntry>();

                if (wc.DB.TableExists<StatisticsEntry>())
                {
                    // delete columns
                    if (wc.DB.ColumnExists("StatisticsEntry", "Test"))
                    {

                    }

                    // add missing columns
                    if (!wc.DB.ColumnExists("StatisticsEntry", "Test"))
                    {

                    }
                }
                //if (wc.DB.TableExists<AbyssStatEntry>())
                //{
                //    // Check if the column exists, and if not, add the column
                //    if (!wc.DB.ColumnExists<AbyssStatEntry>(x => x.AStarErrors))
                //    {
                //        // Add the missing column 'AStarErrors' of type 'int'
                //        wc.DB.AddColumn<AbyssStatEntry>(c => c.AStarErrors);
                //    }
                //}
            }
        }


        private ConnFactory()
        {
            var connString = ConnectionString;
            Console.WriteLine($"connString: {connString}");
            Factory = new OrmLiteConnectionFactory(connString, SqliteDialect.Provider);
        }

        private string ConnectionString
        {
            get
            {
                var _dbFileName = Path.Combine(Utility.Util.AssemblyPath, "EVESharpSettings", "DB.SQLite");
                SQLiteConnectionStringBuilder builder = new SQLiteConnectionStringBuilder();
                builder.DataSource = _dbFileName;
                builder.Pooling = true;
                builder.SyncMode = SynchronizationModes.Normal;
                builder.FailIfMissing = false;
                builder.Version = 3;
                builder.JournalMode = SQLiteJournalModeEnum.Wal;
                return builder.ToString();
            }
        }
    }

}

*/