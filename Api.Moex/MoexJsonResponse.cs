using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using static System.Text.Json.JsonElement;

namespace Api.Moex
{
    /// <summary>
    /// Json ответ на запрос
    /// </summary>
    public class MoexJsonResponse : MoexResponse, IDisposable
    {
        private readonly JsonDocument _jsonDocument;

        public MoexJsonResponse(string response) : base(response)
        {
            _jsonDocument = JsonDocument.Parse(response);
            Root = _jsonDocument.RootElement;
        }

        public JsonTable this[string nameTable] => JsonTables.FirstOrDefault(x => x.Name == nameTable);

        private JsonTable[] _jsonTables;
        /// <summary>
        /// Таблицы из json (имеют "columns" и "data")
        /// </summary>
        public JsonTable[] JsonTables
        {
            get
            {
                if (_jsonTables is null) _jsonTables = GetJsonTables();
                return _jsonTables;
            }
        }

        /// <summary>
        /// Корневой элемент
        /// </summary>
        public JsonElement Root { get; }

        /// <summary>
        /// Получить json таблицы
        /// </summary>
        /// <returns></returns>
        private JsonTable[] GetJsonTables()
        {
            var jsonTables = new List<JsonTable>();

            foreach (var jsonTable in Root.EnumerateObject())
            {
                if (!jsonTable.Value.TryGetProperty(RequestConstants.JSON_COLUMNS_TABLE_NAME, out var columnName)
                    || !jsonTable.Value.TryGetProperty(RequestConstants.JSON_DATA_TABLE_NAME, out var dataElement)) continue;

                int i = 0;
                var jsonColumns = columnName.EnumerateArray()
                    .Select(x => new JsonColumn(i++, x.GetString()))
                    .ToArray();

                jsonTables.Add(new JsonTable(jsonTable.Name, jsonColumns, dataElement.EnumerateArray()));
            }

            return jsonTables.ToArray();
        }

        public override string ToString() => base.ToString();

        public void Dispose()
        {
            _jsonDocument.Dispose();
        }
    }

    /// <summary>
    /// Json таблица
    /// </summary>
    public class JsonTable
    {
        internal JsonTable(string name, JsonColumn[] columns, ArrayEnumerator dataArray)
        {
            Name = name;
            Columns = columns;
            DataArray = dataArray;
        }

        private Dictionary<string, int> _columnIndexByName;
        internal Dictionary<string, int> ColumnIndexByName
        {
            get
            {
                if (_columnIndexByName is null ) _columnIndexByName = Columns.ToDictionary(x => x.Name, x => x.Index);
                return _columnIndexByName;
            }
        }

        private HashSet<int> _columnIndexes;
        internal HashSet<int> ColumnIndexes
        {
            get
            {
                if (_columnIndexes is null) _columnIndexes = new HashSet<int>(Columns.Select(x => x.Index));
                return _columnIndexes;
            }
        }

        /// <summary>
        /// Колонки
        /// </summary>
        public JsonColumn[] Columns { get; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Данные
        /// </summary>
        public ArrayEnumerator DataArray { get; }

        /// <summary>
        /// Данные
        /// </summary>
        public IEnumerable<JsonRow> Rows => DataArray.Select(x => new JsonRow(this, x.EnumerateArray().ToArray()));

        /// <summary>
        /// Преобразовать данные в T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="converter"></param>
        /// <returns></returns>
        public IEnumerable<T> ConvertDataTo<T>(Func<JsonRow, T> converter)
        {
            var result = new List<T>();

            foreach (var row in Rows)
            {
                result.Add(converter(row));
            }

            return result;
        }

        /// <summary>
        /// Преобразовать в DataTable
        /// </summary>
        /// <returns></returns>
        public DataTable ToDataTable()
        {
            var dataTable = new DataTable(Name);

            var jsonColumns = Columns
                .Select(x => new DataColumn(x.Name))
                .ToArray();
            dataTable.Columns.AddRange(jsonColumns);

            foreach (var jsonRow in DataArray)
            {
                var row = dataTable.NewRow();
                dataTable.Rows.Add(row);
                int i = 0;
                foreach (var jsonValue in jsonRow.EnumerateArray()) row[i++] = jsonValue;
            }

            return dataTable;
        }

        /// <summary>
        /// Сопоставляет переданные таблицы и колонки с ответом и заполняет их
        /// </summary>
        /// <param name="dataTables"></param>
        /// <returns>Список заполненых таблиц</returns>
        public bool TryCompleteDataTable(ref DataTable dataTable)
        {
            if (dataTable.TableName != Name) return false;

            var dtColumnNames = new Dictionary<string, DataColumn>(Columns.Length);
            foreach (DataColumn column in dataTable.Columns) dtColumnNames.Add(column.ColumnName, column);

            int i = -1;
            var filtredJsonColumns = new List<(int Index, DataColumn Column)>();

            foreach (var jsonColumn in Columns)
            {
                i++;
                if (!dtColumnNames.TryGetValue(jsonColumn.Name, out var dataColumn)) continue;
                filtredJsonColumns.Add((i, dataColumn));
            }

            foreach (var jsonRow in DataArray)
            {
                var row = dataTable.NewRow();
                dataTable.Rows.Add(row);
                var jsonValues = jsonRow.EnumerateArray().ToArray();

                foreach (var filtredJsonColumn in filtredJsonColumns)
                {
                    if (filtredJsonColumn.Column.DataType == typeof(string)) row[filtredJsonColumn.Column] = jsonValues[filtredJsonColumn.Index];
                    else row[filtredJsonColumn.Column] = jsonValues[filtredJsonColumn.Index].Deserialize(filtredJsonColumn.Column.DataType);
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Json колонка
    /// </summary>
    public class JsonColumn
    {
        internal JsonColumn(int index, string name)
        {
            Index = index;
            Name = name;
        }

        /// <summary>
        /// Индекс колонки в json
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Наименование колонки в json
        /// </summary>
        public string Name { get; }
    }

    /// <summary>
    /// Json строка данных
    /// </summary>
    public class JsonRow
    {
        private JsonTable _jsonTable;

        internal JsonRow(JsonTable jsonTable, JsonElement[] valuesArray)
        {
            _jsonTable = jsonTable;
            ValuesArray = valuesArray;
        }

        /// <summary>
        /// Json колонки
        /// </summary>
        public JsonColumn[] Columns { get; }

        /// <summary>
        /// Значения
        /// </summary>
        public JsonElement[] ValuesArray { get; }

        public JsonElement this[string columnName]
        {
            get
            {
                if (!_jsonTable.ColumnIndexByName.TryGetValue(columnName, out var index)) throw new KeyNotFoundException($"Не удалось найти колонку с наименованием \"{columnName}\"");
                return ValuesArray[index];
            }
        }

        public JsonElement this[int index]
        {
            get
            {
                if (!_jsonTable.ColumnIndexes.Contains(index)) throw new KeyNotFoundException($"Не удалось найти колонку с индексом \"{index}\"");
                return ValuesArray[index];
            }
        }

        public bool ContainsColumn(string name) => _jsonTable.ColumnIndexByName.ContainsKey(name);

        public bool ContainsColumn(int index) => _jsonTable.ColumnIndexes.Contains(index);
    }
}
