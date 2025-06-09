using Microsoft.Data.SqlClient;

using Microsoft.Data.SqlClient;

namespace EmployeeManagementSystem.Utilities
{
    public class DynamicSqlBuilder
    {
        // Used for WHERE clause
        public static (string WhereClause, List<SqlParameter> Parameters) BuildWhereClause(
            int? employee_id = null,
            string employee_name = null,
            string gender = null,
            string email = null,
            DateOnly? dob = null)
        {
            var filters = new List<string>();
            var parameters = new List<SqlParameter>();

            if (employee_id.HasValue)
            {
                filters.Add("employee_id = @employee_id");
                parameters.Add(new SqlParameter("@employee_id", employee_id.Value));
            }
            if (!string.IsNullOrEmpty(employee_name))
            {
                filters.Add("employee_name = @employee_name");
                parameters.Add(new SqlParameter("@employee_name", employee_name));
            }
            if (!string.IsNullOrEmpty(gender))
            {
                filters.Add("gender = @gender");
                parameters.Add(new SqlParameter("@gender", gender));
            }
            if (!string.IsNullOrEmpty(email))
            {
                filters.Add("email = @email");
                parameters.Add(new SqlParameter("@email", email));
            }
            if (dob.HasValue)
            {
                filters.Add("dob = @dob");
                parameters.Add(new SqlParameter("@dob", dob.Value.ToDateTime(TimeOnly.MinValue)));
            }

            return (filters.Count > 0 ? $"WHERE {string.Join(" AND ", filters)}" : "", parameters);
        }

        // Used for UPDATE statements
        public static (List<string> Clauses, List<SqlParameter> Parameters) BuildDynamicUpdateClauses<T>(
            T model,
            List<string> propertiesToInclude)
        {
            var clauses = new List<string>();
            var parameters = new List<SqlParameter>();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                if (!propertiesToInclude.Contains(prop.Name))
                    continue;

                var value = prop.GetValue(model);
                if (IsDefaultOrNull(value))
                    continue;

                string columnName = prop.Name;
                string paramName = $"@{columnName}";

                clauses.Add($"{columnName} = {paramName}");
                parameters.Add(new SqlParameter(paramName, value));
            }

            return (clauses, parameters);
        }

        // Used for INSERT statements
        public static (List<string> Columns, List<SqlParameter> Parameters) BuildDynamicInsertClauses<T>(
            T model,
            List<string> propertiesToInclude)
        {
            var columns = new List<string>();
            var parameters = new List<SqlParameter>();
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                if (!propertiesToInclude.Contains(prop.Name))
                    continue;

                var value = prop.GetValue(model);
                if (IsDefaultOrNull(value))
                    continue;

                string columnName = prop.Name;
                string paramName = $"@{columnName}";

                columns.Add(columnName);
                parameters.Add(new SqlParameter(paramName, value));
            }

            return (columns, parameters);
        }

        private static bool IsDefaultOrNull(object value)
        {
            if (value == null) return true;

            Type type = value.GetType();

            if (type == typeof(string))
                return string.IsNullOrEmpty((string)value);

            if (type == typeof(DateOnly))
                return (DateOnly)value == default;

            if (Nullable.GetUnderlyingType(type) != null)
                return false; // Already checked for null above

            if (type.IsValueType)
                return value.Equals(Activator.CreateInstance(type));

            return false;
        }
    }
}

