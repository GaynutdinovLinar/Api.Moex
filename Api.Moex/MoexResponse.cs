using Api.Moex.Interfaces;

namespace Api.Moex
{
    /// <summary>
    /// Ответ на запрос
    /// </summary>
    /// <param name="Content"></param>
    public class MoexResponse : IMoexResponse
    {
        public MoexResponse(string content)
        {
            Content = content;
        }

        public string Content { get; }

        public override string ToString() => Content;
    }
}
