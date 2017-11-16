using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
namespace TMS
{
    public class ObjectToTable
    {
        /// <summary>
        /// 第一个string为string为自定义,第二个class中的name
        /// </summary>
        public Dictionary<Type, Dictionary<string, string>> Transfer { get; set; }
        public ObjectToTable()
        {
            Transfer = new Dictionary<Type, Dictionary<string, string>>();
        }
        public DataTable Create<T>(IList<T> model)
        {
            Type type = typeof(T);
            Dictionary<string, string> dic = Transfer[type];
            DataTable dt = new DataTable();
            foreach (var x in dic.Keys)
            {
                dt.Columns.Add(x);
            }
            foreach (var x in model)
            {
                DataRow dr = dt.NewRow();
                foreach (var y in dic.Keys)
                {
                    dr[y] = type.GetProperty(dic[y]).GetValue(x);
                }
                dt.Rows.Add(dr);
            }
            return dt;
        }
    }
}
