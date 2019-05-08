/*
//
Nuget package manager で System.Data.Sqlite.Core をインストール
参照追加
System.IO.Compressio
System.IO.Compressio.FileSystem
//
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SQLite;
using System.Xml;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualBasic.FileIO;

namespace Database {
    public class Sqlite {
        public static string DbPath { get; private set; }

        private Dictionary<string, DataTable> schemas;
        public DataTable GetTableClone(string name) {
            name = name.ToLower();
            if (schemas == null)
                schemas = new Dictionary<string, DataTable>();
            DataTable tables;
            using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                conn.Open();
                tables = conn.GetSchema("Tables");
                conn.Close();
            }
            List<string> list = new List<string>();
            foreach (DataRow r in tables.Rows)
                list.Add(r["TABLE_NAME"].ToString().ToLower());
            if (list.Contains(name)) {
                if (!schemas.ContainsKey(name)) {
                    using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                        conn.Open();
                        DataTable schema = conn.GetSchema("Columns", new string[4] { null, null, name, null });
                        schemas.Add(name, schema);
                        conn.Close();
                    }
                }
                DataTable table = new DataTable();
                foreach (DataRow r in schemas[name].Rows) {
                    string columnname = r["COLUMN_NAME"].ToString();
                    Type type = r["DATA_TYPE"].GetType();
                    table.Columns.Add(columnname, type);
                }
                table.PrimaryKey = schemas[name].PrimaryKey;
                return table.Clone();
            }
            return null;
        }

        public Sqlite(string dbpath) {
            DbPath = dbpath;
            Initialize();
            //bool change = ChangeTable();
        }
        public void ChangeDirectory(string dbpath) {
            DbPath = dbpath;
            if (!File.Exists(dbpath))
                Initialize();
        }
        private bool Initialize() {
            bool exist = File.Exists(DbPath);
            using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    string[] queries = new string[] {
                        "CREATE TABLE IF NOT EXISTS `Taxonomy` (`id` INTEGER PRIMARY KEY AUTOINCREMENT,`name` text DEFAULT NULL,`label` text DEFAULT NULL,`lang` text DEFAULT NULL,`label_to` text DEFAULT NULL,`namespace` text DEFAULT NULL,`date` text DEFAULT NULL,`filename` text DEFAULT NULL, url text DEFAULT NULL,`year` integer DEFAULT NULL, `edinet` integer DEFAULT NULL, `archive` text DEFAULT NULL );",
                        "CREATE INDEX IF NOT EXISTS `TaxonomyName` on Taxonomy(`name`)",
                        //"CREATE INDEX IF NOT EXISTS `taxonomy_namelang` ON `Taxonomy` ( `name`, `lang`, `label_to` )",
                        "CREATE TABLE IF NOT EXISTS `FormCodes` ( `id` text NOT NULL, `OrdinanceCode` text DEFAULT NULL, `FormCode` text DEFAULT NULL, `FormNumber` text DEFAULT NULL, `Name` text DEFAULT NULL, `DocType` text DEFAULT NULL, `Disclose` text DEFAULT NULL, `Remarks` text DEFAULT NULL, PRIMARY KEY(`id`) )",
                        "CREATE TABLE IF NOT EXISTS `Disclosures` ( `seqNumber` integer NOT NULL, `docID` text NOT NULL, `edinetCode` text DEFAULT NULL, `secCode` text DEFAULT NULL, `JCN` text DEFAULT NULL, `filerName` text DEFAULT NULL, `fundCode` text DEFAULT NULL, `ordinanceCode` text DEFAULT NULL, `formCode` text DEFAULT NULL, `docTypeCode` text DEFAULT NULL, `periodStart` text DEFAULT NULL, `periodEnd` text DEFAULT NULL, `submitDateTime` text DEFAULT NULL, `docDescription` text DEFAULT NULL, `issuerEdinetCode` text DEFAULT NULL, `subjectEdinetCode` text DEFAULT NULL, `subsidiaryEdinetCode` text DEFAULT NULL, `currentReportReason` text DEFAULT NULL, `parentDocID` text DEFAULT NULL, `opeDateTime` text DEFAULT NULL, `withdrawalStatus` text DEFAULT NULL, `docInfoEditStatus` text DEFAULT NULL, `disclosureStatus` text DEFAULT NULL, `xbrlFlag` text DEFAULT NULL, `pdfFlag` text DEFAULT NULL, `attachDocFlag` text DEFAULT NULL, `englishDocFlag` text DEFAULT NULL, `id` integer primary key,`xbrl` text DEFAULT NULL,`pdf` text DEFAULT NULL,`attach` text DEFAULT NULL,`english` text DEFAULT NULL , `date` text, `status` text DEFAULT NULL, `code` integer DEFAULT NULL )",
                        //"CREATE INDEX IF NOT EXISTS `DisclosuresNumber` on Disclosures(`seqNumber`)",
                        "CREATE INDEX IF NOT EXISTS `DisclosuresDocID` on Disclosures(`docID`)",
                        "CREATE INDEX IF NOT EXISTS `DisclosuresCode` on Disclosures(`secCode`)",
                        "CREATE INDEX IF NOT EXISTS `DisclosuresDocType` on Disclosures(`docTypeCode`)",
                        //"CREATE INDEX IF NOT EXISTS `DisclosuresDate` on Disclosures(`submitDateTime`)",
                        //"CREATE INDEX IF NOT EXISTS `DisclosuresXbrl` on Disclosures(`xbrlFlag`)",
                        "CREATE TABLE IF NOT EXISTS `Metadata` ( `title` text DEFAULT NULL, `date` text NOT NULL primary key, `type` text DEFAULT NULL, `count` integer DEFAULT NULL, `processDateTime` text DEFAULT NULL, `status` text DEFAULT NULL, `message` text DEFAULT NULL,`access` text DEFAULT NULL);",
                        //"CREATE INDEX IF NOT EXISTS `MetadataDate` on Metadata(`date`)",
                        "CREATE INDEX IF NOT EXISTS `DisclosuresDate` on Disclosures(`date`)"
                    };
                    foreach (string query in queries) {
                        try {
                            command.CommandText = query;
                            command.Connection.Open();
                            command.ExecuteNonQuery();

                        } catch (Exception ex) {
                            Console.WriteLine(ex.Message);
                        } finally {
                            command.Connection.Close();
                        }
                    }
                }
            }
            return exist;
        }


        public void Vacuum() {
            //string dbpath = Path.Combine(setting.Directory, "edinet.db");
            //InvokeProgressLabel(0, 0, "データベースVACUUM実行中");
            using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand(conn)) {
                    command.CommandText = "VACUUM;";
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    command.Connection.Close();
                }
            }
        }

        //public object[] GetMetadata(DateTime target) {
        //    using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
        //        using (SQLiteCommand command = new SQLiteCommand()) {
        //            command.Connection = conn;
        //            //command.CommandText = string.Format("select max(datetime(processDateTime)) from Metadata where date = '{0:yyyy-MM-dd}';", target);
        //            command.CommandText = string.Format("select processDateTime, count from Metadata where `status` = '200' and `date` = '{0:yyyy-MM-dd}';", target);
        //            command.Connection.Open();
        //            using (SQLiteDataReader reader = command.ExecuteReader()) {
        //                if (reader.HasRows) {
        //                    reader.Read();
        //                    object[] value = new object[2];
        //                    value[0] = DateTime.Parse(reader.GetString(0));
        //                    value[1] = reader.GetInt32(1);
        //                    return value;
        //                }
        //            }
        //            command.Connection.Close();
        //        }
        //    }
        //    return null;
        //}

        public Edinet.Json.Metadata ReadMetadata(DateTime target) {
            Edinet.Json.Metadata metadata = null; ;
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand(conn)) {
                    command.CommandText = string.Format("select `title`, `date`, `type`, `count`, `processDateTime`, `status`, `message` from Metadata where `status` = '200' and `date` = '{0:yyyy-MM-dd}';", target);
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            metadata = new Edinet.Json.Metadata();
                            reader.Read();
                            metadata.Title = reader.GetString(0);
                            if (!reader.IsDBNull(1) & !reader.IsDBNull(2)) {
                                metadata.Parameter = new Edinet.Json.Parameter() {
                                    Date = reader.GetString(1),
                                    Type = reader.GetString(2)
                                };
                            }
                            if (!reader.IsDBNull(3)) {
                                metadata.Resultset = new Edinet.Json.Resultset() {
                                    Count = reader.GetInt32(3)
                                };
                            }
                            metadata.ProcessDateTime = reader.GetString(4);
                            metadata.Status = reader.GetString(5);
                            metadata.Message = reader.GetString(6);
                        }
                    }
                    command.Connection.Close();
                }
            }
            return metadata;
        }
        public DataTable ReadDisclosure(DateTime target) {
            DataTable table = GetTableClone("disclosures");
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand(conn)) {
                    command.CommandText = string.Format("select * from disclosures where date(`date`) = '{0:yyyy-MM-dd}';", target);
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                DataRow r = table.NewRow();
                                for (int i = 0; i < table.Columns.Count; i++) {
                                    if (reader.IsDBNull(i))
                                        r[i] = DBNull.Value;
                                    else
                                        r[i] = reader.GetValue(i);
                                }
                                table.Rows.Add(r);
                            }
                        }
                    }
                    command.Connection.Close();
                }
            }
            return table;
        }

        //public Nullable<DateTime> GetMetadataProcessDateTime(DateTime target) {
        //    using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
        //        using (SQLiteCommand command = new SQLiteCommand()) {
        //            command.Connection = conn;
        //            //command.CommandText = string.Format("select max(datetime(processDateTime)) from Metadata where date = '{0:yyyy-MM-dd}';", target);
        //            command.CommandText = string.Format("select processDateTime from Metadata where `status` = '200' and `date` = '{0:yyyy-MM-dd}';", target);
        //            command.Connection.Open();
        //            using (SQLiteDataReader reader = command.ExecuteReader()) {
        //                if (reader.HasRows) {
        //                    reader.Read();
        //                    if (!reader.IsDBNull(0))
        //                        return DateTime.Parse(reader.GetString(0));
        //                }
        //            }
        //            command.Connection.Close();
        //        }
        //    }
        //    return null;
        //}
        public List<DateTime> MetadataList() {
            List<DateTime> list = new List<DateTime>();
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    command.CommandText = "select `date` from Metadata where `status` = '200' and strftime('%Y-%m-%d', date(`date`)) <> strftime('%Y-%m-%d', date(processDateTime));";
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                list.Add(DateTime.Parse(reader.GetString(0)));
                            }
                        }
                    }
                    command.Connection.Close();
                }
            }
            return list;
        }

        public Dictionary<DateTime, int> GetFinalMetalist() {
            Dictionary<DateTime, int> dic = new Dictionary<DateTime, int>();
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    command.CommandText = "select `date`, count  from Metadata where `status` = '200' and date(`date`) != date(processDateTime) group by `date`;";
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                dic.Add(DateTime.Parse(reader.GetString(0)), (int)reader.GetInt32(1));
                            }
                        }
                    }
                    command.Connection.Close();
                }
            }
            return dic;
        }

        //public int GetMaxSecNumber(DateTime target) {
        //    using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
        //        using (SQLiteCommand command = new SQLiteCommand()) {
        //            command.Connection = conn;
        //            command.CommandText = string.Format("select max(`seqNumber`) from Disclosures where date(`submitDateTime`) = '{0:yyyy-MM-dd}';", target);
        //            command.Connection.Open();
        //            using (SQLiteDataReader reader = command.ExecuteReader()) {
        //                if (reader.HasRows) {
        //                    reader.Read();
        //                    if (!reader.IsDBNull(0))
        //                        return reader.GetInt32(0);
        //                }
        //            }
        //            command.Connection.Close();
        //        }
        //    }
        //    return 0;
        //}

        //public void ReadSchema(string tablename, out DataTable table) {
        //    table = new DataTable();
        //    using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
        //        using (SQLiteCommand command = new SQLiteCommand(conn)) {
        //            command.CommandText = string.Format("select * from {0} limit 1;", tablename);
        //            command.Connection.Open();
        //            using (SQLiteDataReader reader = command.ExecuteReader()) {
        //                DataTable schema = reader.GetSchemaTable();
        //                List<DataColumn> keys = new List<DataColumn>();
        //                for (int i = 0; i < schema.Rows.Count; i++) {
        //                    DataRow r = schema.Rows[i];
        //                    table.Columns.Add(r["ColumnName"].ToString(), Type.GetType(r["DataType"].ToString()));
        //                    //if ((bool)r["IsUnique"])
        //                    table.Columns[i].Unique = (bool)r["IsUnique"];
        //                    table.Columns[i].AllowDBNull = (bool)r["AllowDBNull"];
        //                    if ((bool)r["IsKey"])
        //                        keys.Add(table.Columns[i]);
        //                }
        //                table.PrimaryKey = keys.ToArray();
        //            }
        //            command.Connection.Close();
        //        }
        //    }
        //}
        //public void ReadQuery(string query, out DataTable table) {
        //    table = new DataTable();
        //    using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
        //        using (SQLiteCommand command = new SQLiteCommand(conn)) {
        //            command.CommandText = query;
        //            command.Connection.Open();
        //            using (SQLiteDataReader reader = command.ExecuteReader()) {
        //                DataTable schema = reader.GetSchemaTable();
        //                List<DataColumn> keys = new List<DataColumn>();
        //                for (int i = 0; i < schema.Rows.Count; i++) {
        //                    DataRow r = schema.Rows[i];
        //                    table.Columns.Add(r["ColumnName"].ToString(), Type.GetType(r["DataType"].ToString()));
        //                    //if ((bool)r["IsUnique"])
        //                    table.Columns[i].Unique = (bool)r["IsUnique"];
        //                    table.Columns[i].AllowDBNull = (bool)r["AllowDBNull"];
        //                    if ((bool)r["IsKey"])
        //                        keys.Add(table.Columns[i]);
        //                }
        //                table.PrimaryKey = keys.ToArray();
        //                if (reader.HasRows) {
        //                    while (reader.Read()) {
        //                        DataRow r = table.NewRow();
        //                        for (int i = 0; i < table.Columns.Count; i++) {
        //                            if (reader.IsDBNull(i))
        //                                r[i] = DBNull.Value;
        //                            else
        //                                r[i] = reader.GetValue(i);
        //                        }
        //                        table.Rows.Add(r);
        //                    }
        //                }
        //            }
        //            command.Connection.Close();
        //        }
        //    }
        //}
        public DataTable ReadQuery(string query) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand(conn)) {
                    DataTable table = new DataTable();
                    command.CommandText = query;
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        DataTable schema = reader.GetSchemaTable();
                        List<DataColumn> keys = new List<DataColumn>();
                        for (int i = 0; i < schema.Rows.Count; i++) {
                            DataRow r = schema.Rows[i];
                            table.Columns.Add(r["ColumnName"].ToString(), Type.GetType(r["DataType"].ToString()));
                            //if ((bool)r["IsUnique"])
                            table.Columns[i].Unique = (bool)r["IsUnique"];
                            table.Columns[i].AllowDBNull = (bool)r["AllowDBNull"];
                            if ((bool)r["IsKey"])
                                keys.Add(table.Columns[i]);
                        }
                        table.PrimaryKey = keys.ToArray();
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                DataRow r = table.NewRow();
                                for (int i = 0; i < table.Columns.Count; i++) {
                                    if (reader.IsDBNull(i))
                                        r[i] = DBNull.Value;
                                    else
                                        r[i] = reader.GetValue(i);
                                }
                                table.Rows.Add(r);
                            }
                        }
                    }
                    command.Connection.Close();
                    return table;
                }
            }
        }

        public void UpdateMetadata(Edinet.Json.Metadata metadata) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand(conn)) {
                    StringBuilder sb = new StringBuilder();
                    string[] fieldsMetadata = "title,date,type,count,processDateTime,status,message,access".Split(',');
                    sb.Append("replace into Metadata(");
                    for (int j = 0; j < fieldsMetadata.Length; j++) {
                        if (j > 0)
                            sb.Append(", ");
                        sb.AppendFormat("`{0}`", fieldsMetadata[j]);
                    }
                    sb.Append(") values (");
                    for (int j = 0; j < fieldsMetadata.Length; j++) {
                        if (j > 0)
                            sb.Append(", ");
                        sb.AppendFormat("@{0}", fieldsMetadata[j]);
                    }
                    sb.Append(");");
                    command.CommandText = sb.ToString();
                    command.Parameters.AddWithValue("@title", metadata.Title);
                    command.Parameters.AddWithValue("@date", metadata.Parameter.Date);
                    command.Parameters.AddWithValue("@type", metadata.Parameter.Type);
                    command.Parameters.AddWithValue("@count", metadata.Resultset.Count);
                    command.Parameters.AddWithValue("@processDateTime", metadata.ProcessDateTime);
                    command.Parameters.AddWithValue("@status", metadata.Status);
                    command.Parameters.AddWithValue("@message", metadata.Message);
                    command.Parameters.AddWithValue("@access", DateTime.Now.ToString());

                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    command.Connection.Close();
                }
            }
        }

        public void UpdateDisclosures(DataView dv, Edinet.Json.Metadata metadata) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand(conn)) {
                    StringBuilder sb = new StringBuilder();
                    string[] fieldsMetadata = "title,date,type,count,processDateTime,status,message,access".Split(',');
                    sb.Append("replace into Metadata(");
                    for (int j = 0; j < fieldsMetadata.Length; j++) {
                        if (j > 0)
                            sb.Append(", ");
                        sb.AppendFormat("`{0}`", fieldsMetadata[j]);
                    }
                    sb.Append(") values (");
                    for (int j = 0; j < fieldsMetadata.Length; j++) {
                        if (j > 0)
                            sb.Append(", ");
                        sb.AppendFormat("@{0}", fieldsMetadata[j]);
                    }
                    sb.Append(");");
                    command.CommandText = sb.ToString();
                    command.Parameters.AddWithValue("@title", metadata.Title);
                    command.Parameters.AddWithValue("@date", metadata.Parameter.Date);
                    command.Parameters.AddWithValue("@type", metadata.Parameter.Type);
                    command.Parameters.AddWithValue("@count", metadata.Resultset.Count);
                    command.Parameters.AddWithValue("@processDateTime", metadata.ProcessDateTime);
                    command.Parameters.AddWithValue("@status", metadata.Status);
                    command.Parameters.AddWithValue("@message", metadata.Message);
                    command.Parameters.AddWithValue("@access", DateTime.Now.ToString());

                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    command.Connection.Close();

                    dv.RowFilter = "edit = 'new'";
                    if (dv.Count > 0) {
                        sb.Clear();
                        command.Parameters.Clear();
                        sb.Append("insert into Disclosures(");
                        for (int i = 0; i < dv.Table.Columns.Count; i++) {
                            if (dv.Table.Columns[i].ColumnName != "edit") {
                                if (i > 0)
                                    sb.Append(", ");
                                sb.AppendFormat("`{0}`", dv.Table.Columns[i].ColumnName);
                            }
                        }
                        sb.Append(") values (");
                        for (int i = 0; i < dv.Table.Columns.Count; i++) {
                            if (dv.Table.Columns[i].ColumnName != "edit") {
                                if (i > 0)
                                    sb.Append(", ");
                                sb.AppendFormat("@{0}", dv.Table.Columns[i].ColumnName);
                            }
                        }
                        sb.Append(");");
                        command.CommandText = sb.ToString();
                        for (int i = 0; i < dv.Table.Columns.Count; i++) {
                            if (dv.Table.Columns[i].ColumnName != "edit") {
                                command.Parameters.AddWithValue("@" + dv.Table.Columns[i].ColumnName, null);
                            }
                        }
                        command.Connection.Open();
                        using (SQLiteTransaction ts = command.Connection.BeginTransaction()) {
                            for (int j = 0; j < dv.Count; j++) {
                                for (int i = 0; i < dv.Table.Columns.Count; i++) {
                                    if (dv.Table.Columns[i].ColumnName != "edit") {
                                        command.Parameters["@" + dv.Table.Columns[i].ColumnName].Value = dv[j][i];
                                    }
                                }
                                command.ExecuteNonQuery();
                            }
                            ts.Commit();
                        }
                        command.Connection.Close();
                    }
                    dv.RowFilter = "edit = 'update'";
                    if (dv.Count > 0) {
                        sb.Clear();
                        command.Parameters.Clear();

                        string[] fields1 = new string[] { "edinetCode", "withdrawalStatus", "docInfoEditStatus", "disclosureStatus", "submitDateTime", "opeDateTime", "status" };
                        sb.Append("update Disclosures set ");
                        for (int i = 0; i < fields1.Length; i++) {
                            if (i > 0)
                                sb.Append(",");
                            sb.AppendFormat(" {0} = @{0}", fields1[i]);
                        }
                        sb.Append(" where id = @id;");
                        command.CommandText = sb.ToString();
                        for (int i = 0; i < fields1.Length; i++)
                            command.Parameters.AddWithValue("@" + fields1[i], null);
                        command.Parameters.Add("@id", DbType.Int64);
                        command.Connection.Open();

                        using (SQLiteTransaction ts = command.Connection.BeginTransaction()) {

                            for (int i = 0; i < dv.Count; i++) {
                                //command.Parameters["@edinetCode"].Value = dv[i]["edinetCode"];
                                command.Parameters["@withdrawalStatus"].Value = dv[i]["withdrawalStatus"];
                                command.Parameters["@docInfoEditStatus"].Value = dv[i]["docInfoEditStatus"];
                                command.Parameters["@disclosureStatus"].Value = dv[i]["disclosureStatus"];
                                command.Parameters["@submitDateTime"].Value = dv[i]["submitDateTime"];
                                command.Parameters["@opeDateTime"].Value = dv[i]["opeDateTime"];
                                command.Parameters["@status"].Value = dv[i]["status"];
                                command.Parameters["@id"].Value = dv[i]["id"];
                                command.ExecuteNonQuery();
                            }
                            ts.Commit();
                        }
                        command.Connection.Close();
                        command.Parameters.Clear();
                    }
                }
            }
        }

        public void UpdateCode() {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand(conn)) {
                    string query = "update disclosures set `code` = substr(secCode, 1, 4);";
                    command.CommandText = query;
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    command.Connection.Close();
                }
            }
        }

        //public DataTable UpdateDisclosures(DateTime target, Edinet.Json.ApiResponse json) {
        //    //jsonがnullの場合は確定した日付の書類一覧
        //    DataTable table = new DataTable();
        //    using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
        //        using (SQLiteCommand command = new SQLiteCommand()) {
        //            command.Connection = conn;
        //            StringBuilder sb = new StringBuilder();
        //            string[] fieldsMetadata = "title,date,type,count,processDateTime,status,message,access".Split(',');
        //            sb.Append("replace into Metadata(");
        //            for (int j = 0; j < fieldsMetadata.Length; j++) {
        //                if (j > 0)
        //                    sb.Append(", ");
        //                sb.AppendFormat("`{0}`", fieldsMetadata[j]);
        //            }
        //            sb.Append(") values (");
        //            for (int j = 0; j < fieldsMetadata.Length; j++) {
        //                if (j > 0)
        //                    sb.Append(", ");
        //                sb.AppendFormat("@{0}", fieldsMetadata[j]);
        //            }
        //            sb.Append(");");
        //            command.CommandText = sb.ToString();
        //            command.Parameters.AddWithValue("@title", json.MetaData.Title);
        //            command.Parameters.AddWithValue("@date", json.MetaData.Parameter.Date);
        //            command.Parameters.AddWithValue("@type", json.MetaData.Parameter.Type);
        //            command.Parameters.AddWithValue("@count", json.MetaData.Resultset.Count);
        //            command.Parameters.AddWithValue("@processDateTime", json.MetaData.ProcessDateTime);
        //            command.Parameters.AddWithValue("@status", json.MetaData.Status);
        //            command.Parameters.AddWithValue("@message", json.MetaData.Message);
        //            command.Parameters.AddWithValue("@access", DateTime.Now.ToString());

        //            command.Connection.Open();
        //            command.ExecuteNonQuery();
        //            command.Connection.Close();
        //            if (json == null || json.MetaData.Resultset.Count == 0)
        //                return null;
        //            string query = string.Format("select * from Disclosures where date(`date`) = '{0:yyyy-MM-dd}';", target);
        //            command.CommandText = query;
        //            command.Connection.Open();
        //            using (SQLiteDataReader reader = command.ExecuteReader()) {
        //                DataTable schema = reader.GetSchemaTable();
        //                List<DataColumn> keys = new List<DataColumn>();
        //                for (int i = 0; i < schema.Rows.Count; i++) {
        //                    DataRow r = schema.Rows[i];
        //                    string columnname = r["ColumnName"].ToString();
        //                    string datatype = r["DataType"].ToString();
        //                    Type type = Type.GetType(datatype);
        //                    table.Columns.Add(columnname, type);
        //                    table.Columns[i].Unique = (bool)r["IsUnique"];
        //                    table.Columns[i].AllowDBNull = (bool)r["AllowDBNull"];
        //                    if ((bool)r["IsKey"])
        //                        keys.Add(table.Columns[i]);
        //                }
        //                table.PrimaryKey = keys.ToArray();
        //                if (reader.HasRows) {
        //                    while (reader.Read()) {
        //                        DataRow r = table.NewRow();
        //                        for (int i = 0; i < table.Columns.Count; i++) {
        //                            if (reader.IsDBNull(i))
        //                                r[i] = DBNull.Value;
        //                            else
        //                                r[i] = reader.GetValue(i);
        //                        }
        //                        table.Rows.Add(r);
        //                    }
        //                }
        //            }
        //            command.Connection.Close();

        //            DataView dv = new DataView(table, "", "id", DataViewRowState.CurrentRows);

        //            string[] fields = new string[] { "id", "xbrl", "pdf", "attach", "english", "date", "status", "code" };

        //            sb.Clear();
        //            command.Parameters.Clear();
        //            sb.Append("insert into Disclosures(");
        //            for (int i = 0; i < Disclosures.Const.FieldName.Count; i++) {
        //                if (i > 0)
        //                    sb.Append(", ");
        //                sb.AppendFormat("`{0}`", Disclosures.Const.FieldName.ElementAt(i).Key);
        //            }
        //            for (int i = 0; i < fields.Length; i++)
        //                sb.AppendFormat(", `{0}`", fields[i]);
        //            sb.Append(") values (");
        //            for (int i = 0; i < Disclosures.Const.FieldName.Count; i++) {
        //                if (i > 0)
        //                    sb.Append(", ");
        //                sb.AppendFormat("@{0}", Disclosures.Const.FieldName.ElementAt(i).Key);
        //            }
        //            for (int i = 0; i < fields.Length; i++)
        //                sb.AppendFormat(", @{0}", fields[i]);
        //            sb.Append(");");
        //            List<int> listUpdate = new List<int>();//idを追加
        //            command.CommandText = sb.ToString();
        //            for (int i = 0; i < Disclosures.Const.FieldName.Count; i++)
        //                command.Parameters.AddWithValue("@" + Disclosures.Const.FieldName.ElementAt(i).Key, null);
        //            command.Parameters.Add("@id", DbType.Int64);
        //            for (int i = 1; i < fields.Length; i++)
        //                command.Parameters.AddWithValue("@" + fields[i], null);
        //            command.Connection.Open();
        //            int count = 0;
        //            using (SQLiteTransaction ts = command.Connection.BeginTransaction()) {

        //                for (int i = 0; i < json.Results.Length; i++) {
        //                    int index = dv.Find(json.Results[i].Id);
        //                    if (index > -1) {
        //                        string status = dv[index]["Status"].ToString();
        //                        if (status == "")
        //                            status = null;
        //                        if (status != json.Results[i].Status) {
        //                            dv[index].BeginEdit();
        //                            dv[index]["status"] = status;
        //                            dv[index].EndEdit();
        //                            listUpdate.Add(json.Results[i].Id);
        //                        }
        //                    } else {
        //                        command.Parameters["@seqNumber"].Value = json.Documents[i].SeqNumber;
        //                        command.Parameters["@docID"].Value = json.Documents[i].DocID;
        //                        command.Parameters["@edinetCode"].Value = json.Documents[i].EdinetCode;
        //                        command.Parameters["@secCode"].Value = json.Documents[i].SecCode;
        //                        command.Parameters["@JCN"].Value = json.Documents[i].Jcn;
        //                        command.Parameters["@filerName"].Value = json.Documents[i].FilerName;
        //                        command.Parameters["@fundCode"].Value = json.Documents[i].FundCode;
        //                        command.Parameters["@ordinanceCode"].Value = json.Documents[i].OrdinanceCode;
        //                        command.Parameters["@formCode"].Value = json.Documents[i].FormCode;
        //                        command.Parameters["@docTypeCode"].Value = json.Documents[i].DocTypeCode;
        //                        command.Parameters["@periodStart"].Value = json.Documents[i].PeriodStart;
        //                        command.Parameters["@periodEnd"].Value = json.Documents[i].PeriodEnd;
        //                        command.Parameters["@submitDateTime"].Value = json.Documents[i].SubmitDateTime;
        //                        command.Parameters["@docDescription"].Value = json.Documents[i].DocDescription;
        //                        command.Parameters["@issuerEdinetCode"].Value = json.Documents[i].IssuerEdinetCode;
        //                        command.Parameters["@subjectEdinetCode"].Value = json.Documents[i].SubjectEdinetCode;
        //                        command.Parameters["@subsidiaryEdinetCode"].Value = json.Documents[i].SubsidiaryEdinetCode;
        //                        command.Parameters["@currentReportReason"].Value = json.Documents[i].CurrentReportReason;
        //                        command.Parameters["@parentDocID"].Value = json.Documents[i].ParentDocID;
        //                        command.Parameters["@opeDateTime"].Value = json.Documents[i].OpeDateTime;
        //                        command.Parameters["@withdrawalStatus"].Value = json.Documents[i].WithdrawalStatus;
        //                        command.Parameters["@docInfoEditStatus"].Value = json.Documents[i].DocInfoEditStatus;
        //                        command.Parameters["@disclosureStatus"].Value = json.Documents[i].DisclosureStatus;
        //                        command.Parameters["@xbrlFlag"].Value = json.Documents[i].XbrlFlag;
        //                        command.Parameters["@pdfFlag"].Value = json.Documents[i].PdfFlag;
        //                        command.Parameters["@attachDocFlag"].Value = json.Documents[i].AttachDocFlag;
        //                        command.Parameters["@englishDocFlag"].Value = json.Documents[i].EnglishDocFlag;
        //                        command.Parameters["@date"].Value = json.MetaData.Parameter.Date;
        //                        command.Parameters["@id"].Value = json.Documents[i].Id;
        //                        command.Parameters["@status"].Value = json.Documents[i].Status;
        //                        if (json.Documents[i].Code > 0)
        //                            command.Parameters["@code"].Value = json.Documents[i].Code;
        //                        else
        //                            command.Parameters["@code"].Value = DBNull.Value;
        //                        DataRowView r = dv.AddNew();
        //                        for (int j = 0; j < dv.Table.Columns.Count; j++) {
        //                            string field = dv.Table.Columns[j].ColumnName;
        //                            if (command.Parameters.Contains("@" + field)) {
        //                                r[field] = command.Parameters["@" + field].Value;
        //                            }
        //                        }
        //                        r.EndEdit();
        //                        try {
        //                            command.ExecuteNonQuery();
        //                            count++;
        //                        } catch (Exception ex) {
        //                            Console.WriteLine("id:" + command.Parameters["@id"].Value);
        //                            Console.WriteLine(ex.Message);
        //                            throw;
        //                        }
        //                    }
        //                }

        //                if (count > 0)
        //                    ts.Commit();
        //            }
        //            command.Connection.Close();


        //            if (listUpdate.Count > 0) {
        //                sb.Clear();
        //                command.Parameters.Clear();

        //                string[] fields1 = new string[] { "edinetCode", "withdrawalStatus", "docInfoEditStatus", "disclosureStatus", "submitDateTime", "opeDateTime", "status" };
        //                sb.Append("update Disclosures set ");
        //                for (int i = 0; i < fields1.Length; i++) {
        //                    if (i > 0)
        //                        sb.Append(",");
        //                    sb.AppendFormat(" {0} = @{0}", fields1[i]);
        //                }
        //                sb.Append(" where id = @id;");
        //                command.CommandText = sb.ToString();
        //                for (int i = 0; i < fields1.Length; i++)
        //                    command.Parameters.AddWithValue("@" + fields1[i], null);
        //                command.Parameters.Add("@id", DbType.Int64);
        //                command.Connection.Open();

        //                using (SQLiteTransaction ts = command.Connection.BeginTransaction()) {

        //                    for (int i = 0; i < json.Documents.Length; i++) {
        //                        if (listUpdate.Contains(json.Documents[i].Id)) {
        //                            command.Parameters["@edinetCode"].Value = json.Documents[i].EdinetCode;
        //                            command.Parameters["@withdrawalStatus"].Value = json.Documents[i].WithdrawalStatus;
        //                            command.Parameters["@docInfoEditStatus"].Value = json.Documents[i].DocInfoEditStatus;
        //                            command.Parameters["@disclosureStatus"].Value = json.Documents[i].DisclosureStatus;
        //                            command.Parameters["@submitDateTime"].Value = json.Documents[i].SubmitDateTime;
        //                            command.Parameters["@opeDateTime"].Value = json.Documents[i].OpeDateTime;
        //                            command.Parameters["@status"].Value = json.Documents[i].Status;
        //                            command.Parameters["@id"].Value = json.Documents[i].Id;
        //                            int index = dv.Find(json.Documents[i].Id);
        //                            dv[index].BeginEdit();
        //                            for (int j = 0; j < command.Parameters.Count; j++) {
        //                                string field = command.Parameters[j].ParameterName.Replace("@", "");
        //                                if (field != "id") {
        //                                    dv[index][field] = command.Parameters[j].Value;
        //                                }
        //                            }
        //                            dv[index].EndEdit();
        //                            command.ExecuteNonQuery();
        //                        }
        //                    }
        //                    ts.Commit();
        //                }
        //                command.Connection.Close();
        //                command.Parameters.Clear();
        //            }

        //        }
        //    }
        //    return table;
        //}


        //public string GetFilename(int id, string field) {
        //    using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
        //        using (SQLiteCommand command = new SQLiteCommand()) {
        //            command.Connection = conn;
        //            command.CommandText = string.Format("select {0} from Disclosures where id = {1};", field, id);
        //            command.Connection.Open();
        //            using (SQLiteDataReader reader = command.ExecuteReader()) {
        //                if (reader.HasRows) {
        //                    reader.Read();
        //                    if (!reader.IsDBNull(0))
        //                        return reader.GetString(0);
        //                }
        //            }
        //            command.Connection.Close();
        //        }
        //    }
        //    return null;
        //}

        public int GetDocumentsCount(int code) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    //command.CommandText = string.Format("select count(*) from Disclosures where `secCode` = {0} order by submitDatetime desc;", code * 10);
                    command.CommandText = string.Format("select count(*) from Disclosures where substr(`secCode`, 1, 4) = '{0}' order by submitDatetime desc;", code);
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            reader.Read();
                            if (!reader.IsDBNull(0))
                                return reader.GetInt32(0);
                        }
                    }
                    command.Connection.Close();
                }
            }
            return 0;
        }

        public DataTable Search(string filtertext) {
            DataTable table = GetTableClone("Disclosures");
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    command.CommandText = $"select * from Disclosures where {filtertext} order by id;";
                    command.Connection.Open();
                    try {
                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                            if (reader.HasRows) {
                                while (reader.Read()) {
                                    DataRow r = table.NewRow();
                                    for (int i = 0; i < table.Columns.Count; i++) {
                                        if (reader.IsDBNull(i))
                                            r[i] = DBNull.Value;
                                        else
                                            r[i] = reader.GetValue(i);
                                    }
                                    table.Rows.Add(r);
                                }
                            }
                        }
                    } catch (Exception ex) {
                        DataRow r = table.NewRow();
                        r["id"] = 0;
                        r["docID"] = "エラー";
                        r["status"] = ex.Message;
                        table.Rows.Add(r);
                    }
                    command.Connection.Close();
                }
            }
            return table;
        }
        public DataTable SearchBrand(int code) {
            DataTable table = GetTableClone("Disclosures");
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    //command.CommandText = string.Format("select * from Disclosures where `secCode` = {0} order by submitDatetime desc;", code * 10);
                    command.CommandText = string.Format("select * from Disclosures where substr(`secCode`, 1, 4) = '{0}' order by submitDatetime desc;", code);
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                DataRow r = table.NewRow();
                                for (int i = 0; i < table.Columns.Count; i++) {
                                    if (reader.IsDBNull(i))
                                        r[i] = DBNull.Value;
                                    else
                                        r[i] = reader.GetValue(i);
                                }
                                table.Rows.Add(r);
                                //DataRow r = table.NewRow();

                                //for (int i = 0; i < reader.FieldCount; i++) {
                                //    Type type = reader.GetFieldType(i);
                                //    string name = reader.GetName(i);
                                //    if (type == typeof(int))
                                //        r[i] = reader.GetInt32(i);
                                //    else if (type == typeof(Int64)) {
                                //        if (reader.IsDBNull(i)) {
                                //            object obj = reader.GetValue(i);
                                //            //System.Int64 value = reader.GetInt64(i);
                                //            if(obj!=null)
                                //            r[i] = obj;
                                //        }
                                //    } else {
                                //        if (reader.IsDBNull(i))
                                //            r[i] = DBNull.Value;
                                //        else
                                //            r[i] = reader.GetString(i);
                                //    }
                                //    //if (name == "docTypeCode" && Edinet.Const.DocTypeCode.ContainsKey(r[i].ToString()))
                                //    //    r["タイプ"] = Edinet.Const.DocTypeCode[r[i].ToString()]; 
                                //}
                                //table.Rows.Add(r);
                            }
                        }
                    }
                    command.Connection.Close();
                }
            }
            return table;
        }

        public DataTable AllDisclosures() {
            DataTable table = GetTableClone("Disclosures");
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    command.CommandText = string.Format("select * from Disclosures order by id;");
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                DataRow r = table.NewRow();
                                for (int i = 0; i < table.Columns.Count; i++) {
                                    if (reader.IsDBNull(i))
                                        r[i] = DBNull.Value;
                                    else
                                        r[i] = reader.GetValue(i);
                                }
                                table.Rows.Add(r);
                            }
                        }
                    }
                    command.Connection.Close();
                }
            }
            return table;
        }

        //public void SearchBrand(int code, ref DataTable table) {
        //    using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
        //        using (SQLiteCommand command = new SQLiteCommand()) {
        //            command.Connection = conn;
        //            command.CommandText = string.Format("select * from Disclosures where `secCode` = {0} order by submitDatetime desc;", code * 10);
        //            command.Connection.Open();
        //            using (SQLiteDataReader reader = command.ExecuteReader()) {
        //                if (reader.HasRows) {
        //                    while (reader.Read()) {
        //                        DataRow r = table.NewRow();

        //                        for (int i = 0; i < reader.FieldCount; i++) {
        //                            Type type = reader.GetFieldType(i);
        //                            string name = reader.GetName(i);
        //                            if (type == typeof(int))
        //                                r[i] = reader.GetInt32(i);
        //                            else if (type == typeof(Int64))
        //                                r[i] = reader.GetInt64(i);
        //                            else {
        //                                if (reader.IsDBNull(i))
        //                                    r[i] = DBNull.Value;
        //                                else
        //                                    r[i] = reader.GetString(i);
        //                            }
        //                            if (name == "docTypeCode" && Edinet.Const.DocTypeCode.ContainsKey(r[i].ToString()))
        //                                r["タイプ"] = Edinet.Const.DocTypeCode[r[i].ToString()];
        //                        }
        //                        table.Rows.Add(r);
        //                    }
        //                }
        //            }
        //            command.Connection.Close();
        //        }
        //    }
        //}


        public void UpdateFilenameOfDisclosure(Dictionary<int, string>[] dic) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand(conn)) {
                    string[] fields = new string[] { "xbrl", "pdf", "attach", "english" };
                    for (int i = 0; i < 4; i++) {
                        if (dic[i] != null && dic[i].Count > 0) {
                            command.Parameters.Clear();
                            command.CommandText = $"update Disclosures set {fields[i]} = @{fields[i]} where id = @id;";
                            command.Parameters.AddWithValue("@" + fields[i], null);
                            command.Parameters.AddWithValue("@id", null);
                            command.Connection.Open();
                            using (SQLiteTransaction ts = command.Connection.BeginTransaction()) {
                                foreach (var kv in dic[i]) {
                                    command.Parameters["@id"].Value = kv.Key;
                                    command.Parameters["@" + fields[i]].Value = kv.Value;
                                    command.ExecuteNonQuery();
                                }
                                ts.Commit();
                                command.Connection.Close();
                            }
                        }
                    }
                }
            }
        }
        public async System.Threading.Tasks.Task UpdateFilenameOfDisclosureAsync(int id, string field, string value) {
            await System.Threading.Tasks.Task.Run(() => {
                using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                    using (SQLiteCommand command = new SQLiteCommand()) {
                        command.Connection = conn;
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("update Disclosures set {0} = @{0} where id = {1};", field, id);
                        command.CommandText = sb.ToString();
                        command.Parameters.AddWithValue("@" + field, value);
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                        command.Connection.Close();
                    }
                }
            });
        }

        public void UpdateFilenameOfDisclosure(int id, string field, string value) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("update Disclosures set {0} = @{0} where id = {1};", field, id);
                    command.CommandText = sb.ToString();
                    command.Parameters.AddWithValue("@" + field, value);
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    command.Connection.Close();
                }
            }
        }

        public void UpdateFilenameOfDisclosure(string docid, string field, int type) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("update Disclosures set {0} = @{0} where docID = {1};", field, docid);
                    command.CommandText = sb.ToString();
                    command.Parameters.AddWithValue("@" + field, $"{docid}_{type}");
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    command.Connection.Close();
                }
            }
        }
#pragma warning disable IDE0051
        private void InsertToTable(string tablename, string[] fields, List<string[]> list, bool replace = false) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0} into {1}(", replace ? "replace" : "insert", tablename);
                    for (int j = 0; j < fields.Length; j++) {
                        if (j > 0)
                            sb.Append(", ");
                        sb.AppendFormat("`{0}`", fields[j]);
                    }
                    sb.Append(") values (");
                    for (int j = 0; j < fields.Length; j++) {
                        if (j > 0)
                            sb.Append(", ");
                        sb.AppendFormat("@{0}", fields[j]);
                    }
                    sb.Append(");");
                    command.CommandText = sb.ToString();
                    SQLiteTransaction ts = null;
                    command.CommandText = sb.ToString();
                    for (int j = 0; j < fields.Length; j++)
                        command.Parameters.AddWithValue("@" + fields[j], null);
                    command.Connection.Open();
                    ts = command.Connection.BeginTransaction();

                    foreach (string[] values in list) {
                        for (int j = 0; j < fields.Length; j++) {
                            command.Parameters[j].Value = values[j];
                        }
                        command.ExecuteNonQuery();
                    }
                    ts.Commit();
                    ts.Dispose();
                    command.Connection.Close();
                }
            }
        }

        private void InsertToTable(string tablename, Dictionary<string, string[]> dic, bool replace = false) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    StringBuilder sb = new StringBuilder();
                    sb.AppendFormat("{0} into {1}(", replace ? "replace" : "insert", tablename);
                    for (int i = 0; i < dic.Keys.Count; i++) {
                        if (i > 0)
                            sb.Append(", ");
                        sb.AppendFormat("`{0}`", dic.ElementAt(i).Key);
                    }
                    sb.Append(") values (");
                    for (int i = 0; i < dic.Keys.Count; i++) {
                        if (i > 0)
                            sb.Append(", ");
                        sb.AppendFormat("@{0}", dic.ElementAt(i).Key);
                    }
                    sb.Append(");");
                    command.CommandText = sb.ToString();
                    SQLiteTransaction ts = null;
                    command.CommandText = sb.ToString();
                    for (int i = 0; i < dic.Keys.Count; i++)
                        command.Parameters.AddWithValue("@" + dic.ElementAt(i).Key, null);
                    command.Connection.Open();
                    ts = command.Connection.BeginTransaction();

                    for (int i = 0; i < dic.ElementAt(0).Value.Length; i++) {
                        foreach (var kv in dic) {
                            command.Parameters[kv.Key].Value = kv.Value[i];
                        }
                        command.ExecuteNonQuery();
                    }
                    ts.Commit();
                    ts.Dispose();
                    command.Connection.Close();
                }
            }
        }
#pragma warning restore IDE0051

        //public void UpdateAPIresult(Disclosures.Edinet apiresult) {

        //    string[] fields = "title,date,type,count,processDateTime,status,message,access".Split(',');
        //    //StringBuilder sb = new StringBuilder();
        //    List<string[]> list = new List<string[]>() {
        //        new string[]{
        //            apiresult.ListResult.Json.Root.MetaData.Title,
        //            apiresult.ListResult.Json.Root.MetaData.Parameter.Date,
        //            apiresult.ListResult.Json.Root.MetaData.Parameter.Type,
        //            apiresult.ListResult.Json.Root.MetaData.Resultset.Count.ToString(),
        //            apiresult.ListResult.Json.Root.MetaData.ProcessDateTime,
        //            apiresult.ListResult.Json.Root.MetaData.Status,
        //            apiresult.ListResult.Json.Root.MetaData.Message,
        //            DateTime.Now.ToString()} };
        //    InsertToTable("Metadata", fields, list);

        //    //Dictionary<string, string[]> dic = new Dictionary<string, string[]>();
        //    for (int i = 0; i < Disclosures.Const.FieldName.Keys.Count; i++) {
        //        //string key = Disclosures.Const.FieldName.Keys.ElementAt(i);
        //        List<string> values = new List<string>();
        //        for (int j = 0; j < apiresult.ListResult.Json.Root.Results.Length; j++) {
        //            values.Add(apiresult.ListResult.Json.Root.Results[j].SeqNumber.ToString());
        //        }
        //    }
        //}

        public static string SearchTaxonomy(string key) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    command.CommandText = string.Format("select label from Taxonomy where `name` = Replace(label_to, 'label_','') and lang = 'ja' and `name` = '{0}';", key);
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            reader.Read();
                            return reader.IsDBNull(0) ? null : reader.GetString(0);
                        } else
                            return null;
                    }
                }
            }
        }

        public static void LoadFormCodes(out Dictionary<string, string> dic, string filepath) {
            dic = new Dictionary<string, string>();
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    command.CommandText = "select id, Name from FormCodes;";
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                string key = reader.GetString(0);
                                string value = reader.GetString(1);
                                dic.Add(key, value);
                            }
                        }
                    }
                    command.Connection.Close();
                    if (dic.Count == 0) {
                        string[] lines = File.ReadAllLines(filepath);
                        StringBuilder sb = new StringBuilder();
                        string[] fields = "id,OrdinanceCode,FormCode,FormNumber,Name,DocType,Disclose,Remarks".Split(',');
                        sb.Append("replace into FormCodes(");
                        for (int j = 0; j < fields.Length; j++) {
                            if (j > 0)
                                sb.Append(", ");
                            sb.AppendFormat("`{0}`", fields[j]);
                        }
                        sb.Append(") values (");
                        for (int j = 0; j < fields.Length; j++) {
                            if (j > 0)
                                sb.Append(", ");
                            sb.AppendFormat("@{0}", fields[j]);
                        }
                        sb.Append(");");
                        SQLiteTransaction ts = null;
                        command.CommandText = sb.ToString();
                        for (int j = 0; j < fields.Length; j++)
                            command.Parameters.AddWithValue("@" + fields[j], null);
                        command.Connection.Open();
                        ts = command.Connection.BeginTransaction();

                        foreach (string line in lines) {
                            string[] cols = line.Split(',');
                            if (cols[0] != "府令コード") {
                                string id = cols[0] + "-" + cols[1];
                                command.Parameters[0].Value = id;
                                for (int j = 1; j < fields.Length; j++) {
                                    command.Parameters[j].Value = cols[j - 1];
                                }
                                command.ExecuteNonQuery();
                                dic.Add(id, cols[3]);
                            }
                        }
                        ts.Commit();
                        ts.Dispose();
                        command.Connection.Close();
                    }

                }
            }
        }

        public string Read(ZipArchiveEntry entry) {
            Encoding enc = Encoding.UTF8;
            if (entry.Name.EndsWith(".txt", false, System.Globalization.CultureInfo.CurrentCulture)
                | entry.Name.EndsWith(".csv", false, System.Globalization.CultureInfo.CurrentCulture))
                enc = Encoding.GetEncoding("shift_jis");
            using (Stream stream = entry.Open()) {
                using (StreamReader reader = new StreamReader(stream, enc)) {
                    return reader.ReadToEnd();
                }

            }
        }


        public void ImportTaxonomy(string url, string source, string archivename = null) {
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;
                    string[] path = url.Split('/');
                    int pathlength = path.Length;
                    //string fn = path[path.Length - 1];
                    string fn = Path.GetFileName(url);

                    //ファイルが存在すれば項目削除
                    command.CommandText = string.Format("select count(*) from taxonomy where filename = '{0}';", fn);
                    int count = 0;
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            reader.Read();
                            count = reader.GetInt32(0);
                        }
                    }
                    command.Connection.Close();
                    if (count > 0) {
                        command.CommandText = string.Format("delete from taxonomy where filename = '{0}';", fn);
                        command.Connection.Open();
                        command.ExecuteNonQuery();
                        command.Connection.Close();
                    }

                    string[] fields = "name,label,lang,label_to,namespace,date,filename,url,year,edinet,archive".Split(',');
                    StringBuilder sb = new StringBuilder();
                    sb.Append("insert into Taxonomy(");
                    for (int i = 0; i < fields.Length; i++) {
                        if (i > 0)
                            sb.Append(", ");
                        sb.AppendFormat("`{0}`", fields[i]);
                    }
                    sb.Append(") values (");
                    for (int i = 0; i < fields.Length; i++) {
                        if (i > 0)
                            sb.Append(", ");
                        sb.AppendFormat("@{0}", fields[i]);
                    }
                    sb.Append(");");
                    command.CommandText = sb.ToString();
                    for (int i = 0; i < fields.Length; i++)
                        command.Parameters.AddWithValue("@" + fields[i], null);

                    XmlDocument document = new XmlDocument();
                    document.LoadXml(source.Replace("<gen:arc", "<link:labelArc")
                                     .Replace("<label:label", "<link:label").Replace("</label:label>", "</link:label>"));
                    XmlNodeList locs = document.GetElementsByTagName("link:loc");
                    XmlNodeList labels = document.GetElementsByTagName("link:label");
                    XmlNodeList arcs = document.GetElementsByTagName("link:labelArc");
                    if (locs.Count == 0)
                        locs = document.GetElementsByTagName("loc");
                    if (labels.Count == 0)
                        labels = document.GetElementsByTagName("label");
                    if (arcs.Count == 0)
                        arcs = document.GetElementsByTagName("labelArc");
                    if (arcs.Count == 0)
                        arcs = document.GetElementsByTagName("link:definitionArc");
                    if (locs.Count == 0 | labels.Count == 0 | arcs.Count == 0) {
                        return;
                    }
                    Dictionary<string, string> dicArc = new Dictionary<string, string>();
                    Dictionary<string, XmlNode> dicLoc = new Dictionary<string, XmlNode>();
                    Dictionary<string, string[]> dicLabel = new Dictionary<string, string[]>();

                    for (int i = 0; i < locs.Count; i++) {
                        dicLoc.Add(locs[i].Attributes["xlink:label"].InnerText, locs[i]);
                    }
                    for (int i = 0; i < labels.Count; i++) {
                        dicLabel.Add(labels[i].Attributes["xlink:label"].InnerText
                                     , new string[] { labels[i].InnerText, labels[i].Attributes["xml:lang"].InnerText });
                    }
                    command.Parameters["@url"].Value = url;
                    command.Parameters["@filename"].Value = fn;
                    if (url.Contains("edinet"))
                        command.Parameters["@edinet"].Value = 1;
                    else
                        command.Parameters["@edinet"].Value = 0;
                    if (DateTime.TryParse(path[pathlength - 2], out DateTime dfile)) {
                        command.Parameters["@date"].Value = path[pathlength - 2];
                        command.Parameters["@year"].Value = dfile.Year;
                    } else if (DateTime.TryParse(path[pathlength - 3], out DateTime dfile2)) {
                        command.Parameters["@date"].Value = path[pathlength - 3];
                        command.Parameters["@year"].Value = dfile2.Year;
                    } else {

                    }
                    command.Parameters["@archive"].Value = archivename;
                    command.Connection.Open();
                    using (var ts = command.Connection.BeginTransaction()) {


                        for (int i = 0; i < arcs.Count; i++) {
                            //Console.WriteLine("{0}\t{1}\t{2}\t{3}", arcs[i].Attributes["xlink:to"].InnerText
                            //                  , arcs[i].Attributes["xlink:from"].InnerText
                            //                  , dicLabel[arcs[i].Attributes["xlink:to"].InnerText][1]
                            //                  , dicLabel[arcs[i].Attributes["xlink:to"].InnerText][0]);
                            string name = arcs[i].Attributes["xlink:from"].InnerText;
                            command.Parameters["@name"].Value = name;
                            string label = dicLabel[arcs[i].Attributes["xlink:to"].InnerText][0];
                            command.Parameters["@label"].Value = label;
                            string lang = dicLabel[arcs[i].Attributes["xlink:to"].InnerText][1];
                            command.Parameters["@lang"].Value = lang;
                            string label_to = arcs[i].Attributes["xlink:to"].InnerText;
                            command.Parameters["@label_to"].Value = label_to;
                            string xsd = dicLoc[arcs[i].Attributes["xlink:from"].InnerText].Attributes["xlink:href"].InnerText;
                            string pattern = "/(.*?)_\\d{4}-";
                            Match match = Regex.Match(xsd, pattern);
                            string prefix = null;
                            if (match.Success)
                                prefix = match.Groups[1].Value;
                            else {
                                string[] ss = xsd.Split('#');
                                prefix = ss[1].Split('_')[0];
                            }
                            command.Parameters["@namespace"].Value = prefix;
                            command.ExecuteNonQuery();
                            //where `name` = Replace(label_to, 'label_','')
                            if (lang == "ja" & name == label_to.Replace("label_", "")) {
                                //DicTaxonomy.Add(prefix + ":" + name, label);
                                //if (!DicTaxonomy.ContainsKey(name))
                                //    DicTaxonomy.Add(name, label);
                            }
                        }
                        ts.Commit();
                    }
                    command.Connection.Close();
                    //Console.WriteLine(entry.LastWriteTime);
                }
            }
        }
        public void LoadTaxonomy(out Dictionary<string, string> dicTaxonomy, out List<string> listXml) {
            dicTaxonomy = new Dictionary<string, string>();
            listXml = new List<string>();
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;

                    //command.CommandText = "select concat(namespace ,':', `name`) as element, label from taxonomy where `name` = Replace(label_to, 'label_','') and lang = 'ja';";
                    command.CommandText = "select distinct filename from Taxonomy;";
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                string filename = reader.GetString(0);
                                listXml.Add(filename);
                            }
                        }
                    }
                    command.Connection.Close();
                    command.CommandText = "select `name` as element, label from Taxonomy where `name` = Replace(label_to, 'label_','') and lang = 'ja';";
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                string element = reader.GetString(0);
                                string jp = reader.IsDBNull(1) ? null : reader.GetString(1);
                                if (!dicTaxonomy.ContainsKey(element))
                                    dicTaxonomy.Add(element, jp);
                            }
                        }
                    }
                    command.Connection.Close();
                }
            }
        }


        public void SaveTaxonomy(Dictionary<string, string> dicTaxonomy, string url, XmlNodeList arcs, Dictionary<string, XmlNode> dicLoc, Dictionary<string, string[]> dicLabel) {
            string[] path = url.Split('/');
            int pathlength = path.Length;
            string fn = path[pathlength - 1];
            using (var conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    command.Connection = conn;

                    string[] fields = "name,label,lang,label_to,namespace,date,filename,url,year,edinet".Split(',');
                    StringBuilder sb = new StringBuilder();
                    sb.Append("insert into Taxonomy(");
                    for (int i = 0; i < fields.Length; i++) {
                        if (i > 0)
                            sb.Append(", ");
                        sb.AppendFormat("`{0}`", fields[i]);
                    }
                    sb.Append(") values (");
                    for (int i = 0; i < fields.Length; i++) {
                        if (i > 0)
                            sb.Append(", ");
                        sb.AppendFormat("@{0}", fields[i]);
                    }
                    sb.Append(");");
                    command.CommandText = sb.ToString();
                    for (int i = 0; i < fields.Length; i++)
                        command.Parameters.AddWithValue("@" + fields[i], null);


                    command.Parameters["@url"].Value = url;
                    command.Parameters["@filename"].Value = fn;
                    if (url.Contains("edinet"))
                        command.Parameters["@edinet"].Value = 1;
                    else
                        command.Parameters["@edinet"].Value = 0;
                    if (DateTime.TryParse(path[pathlength - 2], out DateTime dfile)) {
                        command.Parameters["@date"].Value = path[pathlength - 2];
                        command.Parameters["@year"].Value = dfile.Year;
                    } else if (DateTime.TryParse(path[pathlength - 3], out DateTime dfile2)) {
                        command.Parameters["@date"].Value = path[pathlength - 3];
                        command.Parameters["@year"].Value = dfile2.Year;
                    } else {

                    }
                    command.Connection.Open();
                    using (var ts = command.Connection.BeginTransaction()) {


                        for (int i = 0; i < arcs.Count; i++) {
                            string name = arcs[i].Attributes["xlink:from"].InnerText;
                            command.Parameters["@name"].Value = name;
                            string label = dicLabel[arcs[i].Attributes["xlink:to"].InnerText][0];
                            command.Parameters["@label"].Value = label;
                            string lang = dicLabel[arcs[i].Attributes["xlink:to"].InnerText][1];
                            command.Parameters["@lang"].Value = lang;
                            string label_to = arcs[i].Attributes["xlink:to"].InnerText;
                            command.Parameters["@label_to"].Value = label_to;
                            string xsd = dicLoc[arcs[i].Attributes["xlink:from"].InnerText].Attributes["xlink:href"].InnerText;
                            string pattern = "/(.*?)_\\d{4}-";
                            Match match = Regex.Match(xsd, pattern);
                            string prefix = null;
                            if (match.Success)
                                prefix = match.Groups[1].Value;
                            else {
                                string[] ss = xsd.Split('#');
                                prefix = ss[1].Split('_')[0];
                            }
                            command.Parameters["@namespace"].Value = prefix;
                            command.ExecuteNonQuery();
                            if (lang == "ja" & name == label_to.Replace("label_", "")) {
                                if (!dicTaxonomy.ContainsKey(name))
                                    dicTaxonomy.Add(name, label);
                            }
                        }
                        ts.Commit();
                        command.Connection.Close();

                    }
                }
            }
        }

        /*
        EDINETコードとファンドコードをデータベースにインポートする場合
        Microsoft.VisualBasic参照追加が必要（文字列をダブルクォーテーションで包むCSVを読み込むため）
        */
        public delegate void Delegate(int value);
        public bool UpdateEdinetCodelist(string zip, Delegate delegateMethod, out Dictionary<string, int> dic) {
            dic = null;
            using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    //id ednet 15   docid 8  code 6 
                    //code,brand,market,title,id,xbrl,date
                    command.Connection = conn;
                    command.CommandText = "CREATE TABLE IF NOT EXISTS edinet_code (`code_edinet` text NOT NULL,`syubetu` text DEFAULT NULL,`listed` text DEFAULT NULL,`renketu` text DEFAULT NULL,`sihonkin` integer DEFAULT NULL,`kessan` text DEFAULT NULL,`brand` text DEFAULT NULL,`brand_en` text DEFAULT NULL,`kana` text DEFAULT NULL,`address` text DEFAULT NULL,`indust` text DEFAULT NULL,`code5` integer DEFAULT NULL,`teisyutu` integer DEFAULT NULL,PRIMARY KEY (`code_edinet`) );";
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    command.Connection.Close();
                    command.CommandText = "CREATE TABLE IF NOT EXISTS edinet_fund (`code_fund` text DEFAULT NULL,`code_brand` integer DEFAULT NULL,`name` text DEFAULT NULL,`kana` text DEFAULT NULL,`kubun` text DEFAULT NULL,`teikibin1` text DEFAULT NULL,`teikibin2` text DEFAULT NULL,`code_edinet` text NOT NULL,`hakkousya` text DEFAULT NULL,PRIMARY KEY (`code_fund`) );";
                    command.Connection.Open();
                    command.ExecuteNonQuery();
                    command.Connection.Close();
                    using (ZipArchive archive = ZipFile.OpenRead(zip)) {
                        using (var stream = archive.Entries[0].Open()) {
                            using (TextFieldParser parser = new TextFieldParser(stream, Encoding.GetEncoding("shift_jis"))) {
                                parser.TextFieldType = FieldType.Delimited;

                                parser.SetDelimiters(",");
                                string[] fields = null;
                                int i = 0;
                                List<string[]> list = new List<string[]>();
                                SQLiteTransaction ts = null;
                                while (!parser.EndOfData) {
                                    string[] cols = parser.ReadFields();
                                    if (i == 1) {
                                        StringBuilder sb = new StringBuilder();
                                        if (cols[0] == "ＥＤＩＮＥＴコード") {
                                            fields = "code_edinet,syubetu,listed,renketu,sihonkin,kessan,brand,brand_en,kana,address,indust,code5,teisyutu".Split(',');
                                            sb.Append("replace into edinet_code(");
                                            for (int j = 0; j < fields.Length; j++) {
                                                if (j > 0)
                                                    sb.Append(", ");
                                                sb.AppendFormat("`{0}`", fields[j]);
                                            }
                                            sb.Append(") values (");
                                            for (int j = 0; j < fields.Length; j++) {
                                                if (j > 0)
                                                    sb.Append(", ");
                                                sb.AppendFormat("@{0}", fields[j]);
                                            }
                                            sb.Append(");");
                                        } else if (cols[0] == "ファンドコード") {
                                            fields = "code_fund,code_brand,name,kana,kubun,teikibin1,teikibin2,code_edinet,hakkousya".Split(',');
                                            sb.Append("replace into edinet_fund(");
                                            for (int j = 0; j < fields.Length; j++) {
                                                if (j > 0)
                                                    sb.Append(", ");
                                                sb.AppendFormat("`{0}`", fields[j]);
                                            }
                                            sb.Append(") values (");
                                            for (int j = 0; j < fields.Length; j++) {
                                                if (j > 0)
                                                    sb.Append(", ");
                                                sb.AppendFormat("@{0}", fields[j]);
                                            }
                                            sb.Append(");");
                                        } else {
                                            return false;
                                        }
                                        command.CommandText = sb.ToString();
                                        //command.Connection.Close();
                                        for (int j = 0; j < fields.Length; j++)
                                            command.Parameters.AddWithValue("@" + fields[j], null);
                                    }
                                    if (i > 1) {
                                        list.Add(cols);
                                    }

                                    i++;
                                }

                                dic = new Dictionary<string, int>();
                                command.Connection.Open();
                                ts = command.Connection.BeginTransaction();

                                delegateMethod(0);
                                i = 0;
                                foreach (string[] cols in list) {
                                    string edinet = null;
                                    int code = 0;
                                    for (int j = 0; j < cols.Length; j++) {
                                        command.Parameters[j].Value = cols[j].Trim() == "" ? null : cols[j];
                                        if (fields[j] == "code_edinet")
                                            edinet = cols[j];
                                        if (fields[j] == "code5" & int.TryParse(cols[j], out code)) {
                                            if (code > 0)
                                                dic.Add(edinet, code);
                                        }
                                    }
                                    command.ExecuteNonQuery();
                                    i++;
                                    delegateMethod((int)(i / list.Count * 100));
                                }
                                ts.Commit();
                                ts.Dispose();
                                command.Connection.Close();
                                command.CommandText = "UPDATE Disclosures SET code = (SELECT edinet_code.code5 FROM edinet_code WHERE Disclosures.edinetCode = edinet_code.code_edinet) WHERE EXISTS (SELECT edinet_code.code5 FROM edinet_code WHERE Disclosures.edinetCode = edinet_code.code_edinet);";
                                command.Connection.Open();
                                command.ExecuteNonQuery();
                                command.Connection.Close();

                            }

                        }
                    }
                }
            }
            return true;
        }

        public void ReadEdinetCodelist(out Dictionary<string, int> dic) {
            dic = new Dictionary<string, int>();
            using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0}", DbPath))) {
                using (SQLiteCommand command = new SQLiteCommand()) {
                    //id ednet 15   docid 8  code 6 
                    //code,brand,market,title,id,xbrl,date
                    command.Connection = conn;
                    command.CommandText = "SELECT count(*) FROM sqlite_master WHERE type='table' AND name='edinet_code';";
                    int count = -1;
                    command.Connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader()) {
                        if (reader.HasRows) {
                            reader.Read();
                            if (!reader.IsDBNull(0))
                                count = reader.GetInt32(0);
                        }
                    }
                    command.Connection.Close();
                    if (count > 0) {
                        command.CommandText = "select code_edinet, code5 from edinet_code where code5 > 0;";
                        command.Connection.Open();
                        using (SQLiteDataReader reader = command.ExecuteReader()) {
                            if (reader.HasRows) {
                                while (reader.Read()) {
                                    dic.Add(reader.GetString(0), reader.GetInt32(1));
                                }
                            }
                        }
                        command.Connection.Close();
                    }
                }

            }
        }
    }
}

