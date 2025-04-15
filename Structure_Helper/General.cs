using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Structure_Helper;
public class General
{
    public static DataTable ConvertToDataTable<T>(IList<T> data) // Hàm chuyển danh sách IList<T> thành DataTable
    {
        PropertyDescriptorCollection properties =
           TypeDescriptor.GetProperties(typeof(T)); // Lấy tập hợp các thuộc tính của kiểu T
        DataTable table = new DataTable(); // Tạo một bảng dữ liệu mới (DataTable)
        foreach (PropertyDescriptor prop in properties) // Duyệt qua từng thuộc tính của T
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); // Thêm cột vào bảng với tên và kiểu dữ liệu của thuộc tính
        foreach (T item in data) // Duyệt qua từng phần tử trong danh sách data
        {
            DataRow row = table.NewRow(); // Tạo một hàng mới trong bảng
            foreach (PropertyDescriptor prop in properties) // Duyệt qua từng thuộc tính
                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value; // Gán giá trị của thuộc tính vào cột tương ứng, nếu null thì gán DBNull
            table.Rows.Add(row); // Thêm hàng vào bảng
        }
        return table; // Trả về bảng đã tạo
    }
    public static DataTable ConvertToDataTable1<T>(T data) // Hàm chuyển một đối tượng T thành DataTable
    {
        PropertyDescriptorCollection properties =
           TypeDescriptor.GetProperties(typeof(T)); // Lấy tập hợp các thuộc tính của kiểu T
        DataTable table = new DataTable(); // Tạo một bảng dữ liệu mới
        foreach (PropertyDescriptor prop in properties) // Duyệt qua từng thuộc tính của T
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); // Thêm cột vào bảng với tên và kiểu dữ liệu của thuộc tính
        DataRow row = table.NewRow(); // Tạo một hàng mới trong bảng
        foreach (PropertyDescriptor prop in properties) // Duyệt qua từng thuộc tính
            row[prop.Name] = prop.GetValue(data) ?? DBNull.Value; // Gán giá trị của thuộc tính từ đối tượng data vào cột tương ứng, nếu null thì gán DBNull
        table.Rows.Add(row); // Thêm hàng vào bảng

        return table; // Trả về bảng đã tạo
    }
    public static DataTable ToDataTable<T>(List<T> iList) // Hàm chuyển danh sách List<T> thành DataTable
    {
        DataTable dataTable = new DataTable(); // Tạo một bảng dữ liệu mới
        PropertyDescriptorCollection propertyDescriptorCollection = // Lấy tập hợp các thuộc tính của kiểu T
            TypeDescriptor.GetProperties(typeof(T));
        for (int i = 0; i < propertyDescriptorCollection.Count; i++) // Duyệt qua từng thuộc tính bằng vòng lặp for
        {
            PropertyDescriptor propertyDescriptor = propertyDescriptorCollection[i]; // Lấy thuộc tính thứ i
            Type type = propertyDescriptor.PropertyType; // Lấy kiểu dữ liệu của thuộc tính

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>)) // Kiểm tra nếu kiểu dữ liệu là Nullable (có thể null)
                type = Nullable.GetUnderlyingType(type); // Lấy kiểu dữ liệu cơ bản của Nullable (ví dụ: int? -> int)

            dataTable.Columns.Add(propertyDescriptor.Name, type); // Thêm cột vào bảng với tên và kiểu dữ liệu của thuộc tính
        }
        object[] values = new object[propertyDescriptorCollection.Count]; // Tạo mảng để chứa giá trị của các thuộc tính
        foreach (T iListItem in iList) // Duyệt qua từng phần tử trong danh sách iList
        {
            for (int i = 0; i < values.Length; i++) // Duyệt qua từng thuộc tính
            {
                values[i] = propertyDescriptorCollection[i].GetValue(iListItem); // Lấy giá trị của thuộc tính và lưu vào mảng values
            }
            dataTable.Rows.Add(values); // Thêm hàng vào bảng với các giá trị từ mảng values
        }
        return dataTable; // Trả về bảng đã tạo
    }
    public static List<T> ConvertToListEnableNull<T>(DataTable dt) // Hàm chuyển DataTable thành danh sách List<T>
    {
        var columnNames = dt.Columns.Cast<DataColumn>() // Lấy danh sách tên cột từ DataTable
                .Select(c => c.ColumnName) // Chuyển mỗi cột thành tên của nó
                .ToList(); // Chuyển kết quả thành danh sách
        var properties = typeof(T).GetProperties(); // Lấy tập hợp các thuộc tính của kiểu T
        return dt.AsEnumerable().Select(row => // Duyệt qua từng hàng trong DataTable và chuyển thành đối tượng T
        {
            var objT = Activator.CreateInstance<T>(); // Tạo một đối tượng mới của kiểu T
            foreach (var pro in properties) // Duyệt qua từng thuộc tính của T
            {
                if (columnNames.Contains(pro.Name)) // Kiểm tra nếu tên cột trong DataTable khớp với tên thuộc tính
                {
                    PropertyInfo pI = objT.GetType().GetProperty(pro.Name); // Lấy thông tin thuộc tính của đối tượng
                    if (row[pro.Name] != DBNull.Value) // Kiểm tra nếu giá trị trong cột không phải là null
                    {
                        pro.SetValue(objT, row[pro.Name] = Convert.ChangeType(row[pro.Name], Nullable.GetUnderlyingType(pI.PropertyType) ?? pI.PropertyType)); // Chuyển đổi giá trị từ cột và gán vào thuộc tính của đối tượng
                    }
                    else // Nếu giá trị là null
                    {
                        pro.SetValue(objT, null); // Gán giá trị null cho thuộc tính của đối tượng
                    }
                }
            }
            return objT; // Trả về đối tượng T đã được gán giá trị
        }).ToList(); // Chuyển kết quả thành danh sách List<T>
    }
}
