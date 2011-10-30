namespace Ringify.Web.Infrastructure
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using System.Xml;

    public static class StorageRequestAnalyzer
    {
        public static string GetRequestedTable(HttpRequest request)
        {
            var path = request.PathInfo;
            if (path.ToUpperInvariant().Contains("/TABLES("))
            {
                var match = Regex.Match(path, @"tables\(['""](\w+)['""]\)", RegexOptions.IgnoreCase);
                if (match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }
            }
            else
            {
                var slashPos = path.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                var parenthesisPos = path.IndexOf("(", StringComparison.OrdinalIgnoreCase);
                parenthesisPos = (parenthesisPos == -1) ? path.Length - 1 : parenthesisPos - 1;

                if (slashPos > -1)
                {
                    return path.Substring(slashPos + 1, parenthesisPos - slashPos);
                }
                else
                {
                    return path;
                }
            }

            return string.Empty;
        }

        public static string GetTableToCreate(HttpRequest request)
        {
            var tableName = string.Empty;
            var buffer = new byte[request.InputStream.Length];

            request.InputStream.Seek(0, SeekOrigin.Begin);
            request.InputStream.Read(buffer, 0, (int)request.InputStream.Length);
            request.InputStream.Seek(0, SeekOrigin.Begin);

            var xml = new XmlDocument();
            xml.LoadXml(ASCIIEncoding.ASCII.GetString(buffer));

            var tableElement = xml.GetElementsByTagName("d:TableName");
            if (tableElement.Count > 0)
            {
                tableName = tableElement[0].InnerText;
            }

            return tableName;
        }

        public static string GetRequestedQueue(HttpRequest request)
        {
            var queueName = request.PathInfo.TrimStart('/');
            var slashPost = queueName.IndexOf('/');

            return slashPost > 0 ? queueName.Remove(slashPost) : queueName;
        }

        public static bool IsListingTables(HttpRequest request)
        {
            if (request.PathInfo.Equals("/Tables", StringComparison.OrdinalIgnoreCase) && request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase) && request.ContentLength <= 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsCreatingTable(HttpRequest request, string tableName)
        {
            if (tableName.Equals("Tables", StringComparison.OrdinalIgnoreCase) && request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }

        public static bool IsCreatingQueue(HttpRequest request)
        {
            if (!request.PathInfo.TrimStart('/').Contains("/") && request.HttpMethod.Equals("PUT", StringComparison.OrdinalIgnoreCase) && request.ContentLength <= 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsDeletingTable(HttpRequest request, string tableName)
        {
            var requestPath = string.Format(CultureInfo.InvariantCulture, "/Tables('{0}')", tableName);
            if (request.PathInfo.Equals(requestPath, StringComparison.OrdinalIgnoreCase) && (request.HttpMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase) || request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase)) && request.ContentLength <= 0)
            {
                return true;
            }

            return false;
        }

        public static bool IsDeletingQueue(HttpRequest request)
        {
            if (!request.PathInfo.TrimStart('/').Contains("/") && request.HttpMethod.Equals("DELETE", StringComparison.OrdinalIgnoreCase) && request.ContentLength <= 0)
            {
                return true;
            }

            return false;
        }
    }
}