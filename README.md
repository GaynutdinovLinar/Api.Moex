# Api.Moex
## Описание
Api.Moex - библиотека, созданная для более удобного взаимодействия с ISS Московской биржи.

Установить ее можно при помощи менеджера NuGet пакетов.
```
Install-Package Api.Moex -Version 2.0.0
```

Ключевым элементом библиотеки является класс **Moex**. При помощи него можно совершать запросы к ISS и получать ответ в виде объекта **MoexResponse** или дочернего от него. MoexResponse хранит в себе строку ответа на запрос.

Согласно документации ISS Московской биржи ответ на запрос можно получить в виде json, csv или xml файла (пример запроса http://iss.moex.com/iss/index.json). В зависимости от вида файла данные нужно считывать по разному.

В библиотеке существует класс **MoexJsonResponse**, являющийся наследником от MoexResponse, поэтому его можно получать в качестве ответа на запрос, возвращающий json файл. Он обладает функционалом упрощающим чтение и получение данных.

Response-классы для csv и xml файлов в библиотеке отсутствуют. Но вы можете написать их сами, унаследовавшись от MoexResponse, и получать в качестве ответов на запрос.

## Примеры использования

Получение ответа на запрос и вывод в консоль.
``` c#
var moex = new Moex();
var response = await moex.GetAsync<MoexResponse>(@"http://iss.moex.com/iss/index.json");
Console.WriteLine(response.Content);
```
Получение таблицы engines из запроса в виде DataTable.
```c#
var moex = new Moex();
using (var response = await moex.GetAsync<MoexJsonResponse>(@"http://iss.moex.com/iss/index.json"))
{
    var enginesDataTable = response["engines"].ToDataTable();
}
```

Заполнение созданной таблицы DataTable. В данном случае будут заполняться только те колонки, что есть в DataTable. При этом наименование таблицы и колонок должны совпадать с их наименованиями в json ответе на запрос.

Если указать тип значение в колонке, то данные будут преобразованы к нему (по умолчанию string).
```C#
var engines = new DataTable("engines");
engines.Columns.Add("id", typeof(int));
engines.Columns.Add("name");

var moex = new Moex();
using (var response = await moex.GetAsync<MoexJsonResponse>(@"http://iss.moex.com/iss/index.json"))
{
    if (response["engines"].TryCompleteDataTable(ref engines))
    {
        Console.WriteLine("Таблица engines заполнилась!");
    }
}
```

Построчное преобразование данных.
```C#
var moex = new Moex();
using (var response = await moex.GetAsync<MoexJsonResponse>(@"http://iss.moex.com/iss/index.json"))
{
    var boards = response["boards"].ConvertDataTo(row => new
    {
        Id = row["id"].GetInt32(),
        BoardId = row["boardid"].GetString(),
        IsTraded = row["is_traded"].GetInt32() == 0 ? false : true,
    }).ToArray();
}
```
