using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace DanishRegisterOfMotorVehicles.Api.Scraping
{
    public class Parser
    {
        private const string HIDDEN_TOKEN_NAME = "dmrFormToken";
        private readonly char[] _replaceWithDash = "/\\ ".ToCharArray();
        private readonly char[] _replaceWithEmpty = ":;,. ()".ToCharArray();

        private readonly char[] _trim = "\r\n\t:;,. ".ToCharArray();


        public string GetAuthenticationToken(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            return htmlDoc.DocumentNode
                .SelectSingleNode("//input[@name='" + HIDDEN_TOKEN_NAME + "']")
                .GetAttributeValue("value", null);
        }

        public List<Entity> ParseHtmlDocToVehicle(string html)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            if (htmlDoc.DocumentNode.InnerText.Contains("Ingen køretøjer fundet"))
                return null;

            var vehicleNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='notrequired keyvalue singleDouble']");
            var lineNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='line']");
            var entities = new List<Entity>();

            foreach(var vehicleNode in vehicleNodes)
            {
                var value = vehicleNode.Descendants().SingleOrDefault(x => x.Attributes["class"]?.Value == "key")?.InnerText;
                var key = vehicleNode.Descendants().SingleOrDefault(x => x.Attributes["class"]?.Value == "value")?.InnerText;

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    continue;

                entities.Add(new Entity()
                {
                    Key = DecodeValue(key),
                    Value = DecodeValue(value)
                });
            }

            foreach (var lineNode in lineNodes)
            {
                var value =lineNode.Descendants("label")?.FirstOrDefault()?.InnerText;
                var key = lineNode.Descendants("span")?.FirstOrDefault()?.InnerText;

                if(string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                    continue;

                entities.Add(new Entity()
                {
                    Key = DecodeValue(value),
                    Value = DecodeValue(key)
                });
            }

            return entities;
        }

        private string DecodeValue(string s)
        {
            var result = GetPrettyString(s);
            //result = result.Replace("æ", "ae");
            //result = result.Replace("ø", "oe");
            //result = result.Replace("å", "aa");
            //result = Encoding.ASCII.GetString(CodePagesEncodingProvider.Instance.GetEncoding("Cyrillic")
            //    .GetBytes(result));
            result = result.Replace(":", "");

            //foreach (var c in _replaceWithEmpty)
            //    result = result.Replace(c.ToString(), "_");
            //foreach (var c in _replaceWithDash)
            //    result = result.Replace(c.ToString(), "-");
            return result.ToLower().Trim(_trim);
        }

        private string GetPrettyString(string s)
        {
            var result = s.Trim(_trim);
            return result.Replace("&shy;", " ");
        }
    }
}