using System.Linq;
using System.Text;

namespace Api.Moex.Helpers
{
    public class RequestBuilder
    {
        private readonly StringBuilder _mainUrlPartBuilder;
        private readonly StringBuilder _paramsUrlPartBuilder;

        public RequestBuilder(string mainUrlPart = "", string paramsUrlPart = "")
        {
            _mainUrlPartBuilder = new StringBuilder(mainUrlPart);
            _paramsUrlPartBuilder = new StringBuilder(paramsUrlPart);
        }

        public string GetUrl(string fileType) => _mainUrlPartBuilder + fileType + _paramsUrlPartBuilder.ToString().Skip(1);

        public void AddParameters(string parameter) => _paramsUrlPartBuilder.Append(RequestConstants.PARAMS_UNION_SYMBOL + parameter);

        public void AddPath(string path) => _mainUrlPartBuilder.Append(RequestConstants.PATH_UNION_SYMBOL + path);
    }
}
