using System;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Text.RegularExpressions;

namespace uConcur.Tests.Web.Helpers {
    public  class TestDatabase {
        private readonly FileInfo _sdfFile;
        private readonly string _connectionString;

        public TestDatabase(string sdfPath) {
            _sdfFile = new FileInfo(sdfPath);
            _connectionString = $"Data Source={_sdfFile.FullName}";
        }

        public void Create(string sqlPath) {
            var cachedEmptyPath = _sdfFile.FullName + ".empty";
            if (File.Exists(cachedEmptyPath)) {
                File.Copy(cachedEmptyPath, _sdfFile.FullName);
                return;
            }

            using (var engine = new SqlCeEngine(_connectionString)) {
                engine.CreateDatabase();
            }

            ExcuteScriptFromFile(sqlPath);
            _sdfFile.CopyTo(cachedEmptyPath);
        }

        public void Recreate(string sqlPath) {
            if (_sdfFile.Exists)
                Drop();

            Create(sqlPath);
        }

        public void Drop() {
            _sdfFile.Delete();
        }

        private void ExcuteScriptFromFile(string sqlPath) {
            var scripts = Regex.Split(File.ReadAllText(sqlPath), @"^\s*GO\s*$", RegexOptions.Multiline);
            using (var connection = new SqlCeConnection(_connectionString)) {
                connection.Open();
                using (var command = connection.CreateCommand()) {
                    command.CommandType = CommandType.Text;
                    foreach (var script in scripts) {
                        if (string.IsNullOrWhiteSpace(script))
                            continue;
                        try {
                            command.CommandText = script;
                            command.ExecuteNonQuery();
                        }
                        catch (Exception ex) {
                            throw new Exception($"{ex.Message}{Environment.NewLine}SQL: {script}", ex);
                        }
                    }
                }
            }
        }
    }
}
