using API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Utils
{
    public class CommonUtils
    {
        public static ContentResult jsonResponse(ResultDTO res)
        {
            ContentResult contentResult = new() { ContentType = "application/json" };

            try
            {
                string jsonContent = JsonSerializer.Serialize(res);
                contentResult.StatusCode = res.StatusCode;
                contentResult.Content = jsonContent;

                return contentResult;
            }
            catch (NotSupportedException)
            {
                throw new NotSupportedException("Not supported");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
