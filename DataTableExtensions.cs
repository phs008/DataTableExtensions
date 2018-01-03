using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NLog;

namespace DTExtension
{
    /// <summary>
    /// [Author : 박흥식]
    /// BulkInsert 를 편하게 하기 위하여 Data 기반으로 Property를 동적으로 읽어와서 자동으로 Table Column 과 rows 데이터를 생성해주는 ExtensionClass
    /// </summary>
    public static class DataTableExtensions
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// [Author : 박흥식]
        /// IList data 기반으로 DataTable 을 자동 생성해 주는 함수
        /// data의 Property Name 기반으로 table 의 Column 과 rows 데이터를 자동으로 생성해준다.
        /// </summary>
        /// <typeparam name="T">data 의 Type</typeparam>
        /// <param name="data">T 타입의 data</param>
        /// <returns>System.Data.DataTable</returns>
        public static DataTable ToDataTable<T>(this IList<T> data)
        {
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            int propertyCnt = 0;
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                if (prop.PropertyType.IsGenericType || prop.Attributes.OfType<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>().Any())
                    continue;

                table.Columns.Add(prop.Name, prop.PropertyType);
                ++propertyCnt;
            }
            object[] values = new object[propertyCnt];

            foreach (T item in data)
            {
                for (int i = 0; i < props.Count; i++)
                {
                    if (props[i].PropertyType.IsGenericType || props[i].Attributes.OfType<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>().Any())
                        continue;
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        if (table.Columns[j].ColumnName == props[i].Name)
                        {
                            values[j] = props[i].GetValue(item);
                            break;
                        }
                    }
                }
                table.Rows.Add(values);
            }
            return table;
        }
        /// <summary>
        /// [Author : 박흥식]
        /// SqlBulkCopy 사용시 columnname 에 따라 Mapping 처리를 하드코딩 해야 하는데 
        /// data 기반으로 Property name 을 읽어 columnMapping Collection 을 생성해준다.
        /// </summary>
        /// <typeparam name="T">data 의 Type</typeparam>
        /// <param name="data">T 타입의 data</param>
        /// <returns>ICollection<SqlBulkCopyColumnMapping> columnMappingCollection</SqlBulkCopyColumnMapping></returns>
        public static ICollection<SqlBulkCopyColumnMapping> ToColumnMapping<T>(this IList<T> data)
        {
            ICollection<SqlBulkCopyColumnMapping> ColumnMappings = new List<SqlBulkCopyColumnMapping>();
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            int propertyCnt = 0;
            for (int i = 0; i < props.Count; i++)
            {
                PropertyDescriptor prop = props[i];
                if (prop.PropertyType.IsGenericType || prop.Attributes.OfType<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>().Any())
                    continue;
                SqlBulkCopyColumnMapping tupleData = new SqlBulkCopyColumnMapping(prop.Name, prop.Name.ToLower());
                ColumnMappings.Add(tupleData);
                propertyCnt++;
            }
            return ColumnMappings;
        }
    }
}
