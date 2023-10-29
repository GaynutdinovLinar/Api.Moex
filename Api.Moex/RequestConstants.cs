namespace Api.Moex
{
    public static class RequestConstants
    {
        /// <summary>
        /// ISS URI
        /// </summary>
        public const string IIS_URI = @"https://iss.moex.com/iss";

        /// <summary>
        /// Символ объединяющий параметры в URI
        /// </summary>
        public const string PARAMS_UNION_SYMBOL = "&";

        /// <summary>
        /// Символ объединяющий пути в URI
        /// </summary>
        public const string PATH_UNION_SYMBOL = "/";

        /// <summary>
        /// Наименование Json элемента, хранящий список колонок
        /// </summary>
        internal const string JSON_COLUMNS_TABLE_NAME = "columns";

        /// <summary>
        /// Наименование Json элемента, хранящий данные
        /// </summary>
        internal const string JSON_DATA_TABLE_NAME = "data";
    }
}
