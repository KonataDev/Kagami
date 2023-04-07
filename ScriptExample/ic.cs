var ic = string(string str) => {
  var queryStr = str.TrimStart(' ');
  if (string.IsNullOrEmpty(queryStr)) return "查询错误, 参数为空";

  var redirectMsg = string.Empty;

  try {
    followRedirect:

	// request api
	var bytes = Wget("https://www.semiee.com/bdxx-api/chip/v3/" +
		$"rich/search?&model={queryStr}").Result;

    // results
    var json = JSON.Parse(Encoding.UTF8.GetString(bytes));
    if (json.code != 0) return "查询错误, 未知错误。代码1";

    // no result
    if (json.total == 0) {
      bytes = Wget("https://www.semiee.com/bdxx-api/chip/v3/" +
        $"rich/search/recommend?model={queryStr}").Result;

      json = JSON.Parse(Encoding.UTF8.GetString(bytes));
      if (json.code != 0) return "查询错误, 未知错误。代码2"; {
        json = JSON.Parse(Encoding.UTF8.GetString(bytes));

        // follow the result
        queryStr = json.result.content;
        redirectMsg = $"未搜索到结果, 使用相似型号 '{json.result.content}' 进行搜索。\n";
        goto followRedirect;
      }
    }

    var total = 0;
    var result = "";
    if (json.total > 5) {
      total = 5;
      result = $"{redirectMsg}" +
        $"搜索结果数量太多 ({json.total}+), 仅显示前 5 条结果。请点击下方链接查看完整列表。\n" +
        $"https://www.semiee.com/search?searchModel={queryStr}\n\n";
    } else {
      total = (int) json.total;
      result = $"{redirectMsg}" +
        $"半导小芯查询返回 {total} 条结果\n\n";
    }

    // generate result
    for (var i = 0; i < total; ++i) {
      result += $"{json.result[i].name} [{json.result[i].brand_name}]\n";
      result += $"{json.result[i].descri}\n";

      try {
        bytes = Wget($"https://www.semiee.com/bdxx-api/chip/detail/{json.result[i].id}").Result;
        var pdfjson = JSON.Parse(Encoding.UTF8.GetString(bytes));
        if (pdfjson.code != 0) throw new Exception("catchme");

        result += pdfjson.result.dsFile.path.Replace(" ", "%20");
      } catch {
        result += $"https://www.semiee.com/{json.result[i].id}.html";
      }

      result += "\n\n";
    }

    return result.TrimEnd('\n');
  }
  catch (Exception e) {
    return "内部错误, 可能是服务器不可用。";
  }
};
