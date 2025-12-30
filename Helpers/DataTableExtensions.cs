using System.Data;

namespace WebAPI.Helpers
{
    public static class DataTableExtensions
    {
        public static List<Dictionary<string, object>> ToList(this DataTable table)
        {
            var list = new List<Dictionary<string, object>>();

            foreach (DataRow row in table.Rows)
            {
                var dict = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    dict[col.ColumnName] = row[col];
                }
                list.Add(dict);
            }
            return list;
        }
    }

}
