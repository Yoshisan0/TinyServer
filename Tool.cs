using System.IO;

public class Tool
{
    /// <summary>
    /// パスからContentTypeを取得する
    /// </summary>
    /// <param name="_path">パス</param>
    /// <returns>ContentType</returns>
    public static string GetContentTypeFromPath(string _path)
    {
        string ext = Path.GetExtension(_path).ToLowerInvariant();
        return GetContentTypeFromExt(ext);
    }

    /// <summary>
    /// 拡張子からContentTypeを取得する
    /// </summary>
    /// <param name="_ext">拡張子</param>
    /// <returns>ContentType</returns>
    public static string GetContentTypeFromExt(string _ext)
    {
        string contentType = "application/octet-stream"; 

        switch (_ext)
        {
            case ".html":
            case ".htm":
                {
                    contentType = "text/html";
                }
                break;
            case ".css":
                {
                    contentType = "text/css";
                }
                break;
            case ".js":
                {
                    contentType = "application/javascript";
                }
                break;
            case ".json":
                {
                    contentType = "application/json";
                }
                break;
            case ".png":
                {
                    contentType = "image/png";
                }
                break;
            case ".jpg":
            case ".jpeg":
                {
                    contentType = "image/jpeg";
                }
                break;
            case ".gif":
                {
                    contentType = "image/gif";
                }
                break;
            case ".svg":
                {
                    contentType = "image/svg+xml";
                }
                break;
            case ".txt":
                {
                    contentType = "text/plain";
                }
                break;
            case ".ico":
                {
                    contentType = "image/x-icon";
                }
                break;
            case ".xml":
                {
                    contentType = "application/xml";
                }
                break;
            case ".pdf":
                {
                    contentType = "application/pdf";
                }
                break;
            case ".zip":
                {
                    contentType = "application/zip";
                }
                break;
        }

        return contentType;
    }
}
