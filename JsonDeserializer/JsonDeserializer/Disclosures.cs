using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;
using System.IO;

namespace Edinet {

    class JsonContent {
        public DataTable Table { get; private set; }
        public Json.Metadata Metadata { get; private set; }
        public Exception Exception { get; private set; }
        public JsonContent(DataTable table, Json.Metadata metadata) {
            Table = table;
            Metadata = metadata;
        }
        public JsonContent(Exception exception) {
            Exception = exception;
        }
    }
    class Disclosures {

        private readonly RequestDocument apiDocument;
        public Disclosures(string version = "v1") {
            apiDocument = new RequestDocument(version);
        }

        public async Task<JsonContent> ApiRequest(DateTime target, RequestDocument.RequestType requestType) {
            if (target.Date > DateTime.Now | target.Date < DateTime.Now.Date.AddYears(-5))
                return null;
            HttpResponse response = await apiDocument.Request(target, requestType);
            if (response.Exception != null)
                return new JsonContent(response.Exception);
            else
                return JsonToTable(apiDocument.Json);

        }
        public JsonContent ReadJsonfile(string filepath) {
            using (Stream stream = File.OpenRead(filepath)) {
                JsonDeserializer json = new JsonDeserializer(stream);
                if (json.Response == null)
                    return null;
                else
                    return JsonToTable(json);
            }
        }
        public JsonContent JsonToTable(JsonDeserializer json) {
            DataTable table = new DataTable();
            if (json.Response.Documents != null) {
                for (int i = 0; i < json.Response.Documents.Length; i++) {
                    PropertyInfo[] properties = json.Response.Documents[i].GetType().GetProperties();
                    List<object> list = new List<object>();
                    if (i == 0) {
                        table = new DataTable();
                        foreach (PropertyInfo property in properties) {
                            Type type = property.PropertyType;
                            if (type.FullName.Contains("Nullable")) {
                                if (type.FullName.Contains("Int32"))
                                    type = typeof(int);
                                else if (type.FullName.Contains("DateTime"))
                                    type = typeof(DateTime);
                                else {

                                }
                            }
                            DataColumn column = new DataColumn(property.Name, type);
                            table.Columns.Add(column);
                        }
                        table.Rows.Clear();
                    }
                    foreach (PropertyInfo property in properties) {
                        list.Add(property.GetValue(json.Response.Documents[i], null));
                    }
                    table.Rows.Add(list.ToArray());
                }
            }
            return new JsonContent(table, json.Response.MetaData);
        }

        public void Dispose() {
            apiDocument.Dispose();
        }
    }
}
