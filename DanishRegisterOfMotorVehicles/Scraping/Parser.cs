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


//            // sæt rod node: body -> .h-tab-content-inner
//            var root = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='h-tab-content-inner']");
//
//            if (root == null)
//                return null;
//
//            // nu har(burde) vi en liste der består af -> h2 #text div[id='id*']...
//            // de tre første noder er ikke nøvendige, 
//            // et h tag, en tekst node og et div med en form: h2 #text div
//            var rows = root.ChildNodes.Where(x => x.Name == "div").ToList();
//
//            foreach (var row in rows)
//            {
//                list.AddRange(GetFirstRowEntities(row));
//            }

            var vehicleNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='notrequired keyvalue singleDouble']");
            var lineNodes = htmlDoc.DocumentNode.SelectNodes("//div[@class='line']");
            var entities = new List<Entity>();
            foreach (var lineNode in lineNodes)
            {
                try
                {
                    entities.Add(new Entity()
                    {
                        Label = lineNode.SelectSingleNode("//label/text()").InnerText,
                        Value = lineNode.SelectSingleNode("//span/text()").InnerText
                    });
                }
                catch
                {
                    continue;
                }

            }

            return entities;
        }

        private IEnumerable<HtmlNode> GetUnits(HtmlNode node)
        {
            return node.ChildNodes.Where(x => x.GetAttributeValue("class", "").Contains("unit"));
        }

        private IEnumerable<HtmlNode> GetLines(HtmlNode node)
        {
            return node.ChildNodes.Where(x => x.GetAttributeValue("class", "").Contains("line"));
        }

        private HtmlNode GetLine(HtmlNode node)
        {
            return GetLines(node).FirstOrDefault();
        }

        private string GetCategory(HtmlNode node)
        {
            return GetPrettyString(node.SelectSingleNode("h3").InnerText);
        }

        private IEnumerable<Entity> GetFirstRowEntities(HtmlNode row)
        {
            var line = GetLine(row);

            var units = GetUnits(line).ToList();

            var unitsSpecial =
                GetUnits(line.ChildNodes.First(x => x.Name == "div")).ToList();

            if (units == null || units.Count() != 2)
                throw new Exception("Found an unexpected number of units. Expected 2 saw " + units.Count());

            if (unitsSpecial == null || unitsSpecial.Count() != 2)
                throw new Exception("Found an unexpected number of special units. Expected 2 saw " + units.Count());

            var unitA = units.First();
            var unitB = units.Last();

            var unitSpecialA = unitsSpecial.First();
            var unitSpecialB = unitsSpecial.Last();

            var categoryA = GetCategory(unitSpecialA);
            var categoryB = GetCategory(unitSpecialB);

            // result
            var result = new List<Entity>();

            var divsA =
                unitSpecialA.ChildNodes
                    .First(x => x.GetAttributeValue("class", "") == "bluebox").ChildNodes
                    .Where(y => y.GetAttributeValue("class", "") == "notrequired keyvalue singleDouble");

            foreach (var divA in divsA)
            {
                var model = GetModelFromSpecialUnits(divA, categoryA);
                result.Add(model);
            }

            foreach (var lineA in GetLines(unitA))
            {
                var innerUnitsAA = GetUnits(lineA).ToList();
                var modelAA = GetModelFromInnerUnits(innerUnitsAA, categoryA);
                result.Add(modelAA);
            }

            IEnumerable<HtmlNode> divsB =
                unitSpecialB.ChildNodes
                    .First(x => x.GetAttributeValue("class", "") == "bluebox").ChildNodes
                    .Where(y => y.GetAttributeValue("class", "") == "notrequired keyvalue singleDouble")
                    .ToList();

            foreach (var divB in divsB)
            {
                var model = GetModelFromSpecialUnits(divB, categoryB);
                result.Add(model);
            }

            foreach (var lineB in GetLines(unitB))
            {
                var innerUnitsBB = GetUnits(lineB).ToList();
                var modelBB = GetModelFromInnerUnits(innerUnitsBB, categoryB);
                result.Add(modelBB);
            }

            return result;
        }

        private Entity GetModelFromInnerUnits(IEnumerable<HtmlNode> innerUnits, string category = "")
        {
            var label = GetPrettyString(innerUnits.First().InnerText);
            var slug = GetSlug(label);
            var path = GetSlug(category) + "/" + slug;
            var value = innerUnits.Last().SelectSingleNode("span").InnerText;
            var model = new Entity
            {
                Path = path,
                Category = category,
                Slug = slug,
                Label = label,
                Value = value
            };
            return model;
        }

        private Entity GetModelFromSpecialUnits(HtmlNode innerUnits, string category = "")
        {
            var divsBTrimmed = innerUnits.ChildNodes.Where(da => da.Name == "span");

            var label = GetPrettyString(divsBTrimmed.First().InnerText);
            var slug = GetSlug(label);
            var value = GetPrettyString(divsBTrimmed.Last().InnerText);
            var path = GetSlug(category) + "/" + slug;
            var model = new Entity
            {
                Path = path,
                Category = category,
                Slug = slug,
                Label = label,
                Value = value
            };
            return model;
        }

        private string GetSlug(string s)
        {
            var result = GetPrettyString(s);
            result = result.Replace("æ", "ae");
            result = result.Replace("ø", "oe");
            result = result.Replace("å", "aa");
            result = Encoding.ASCII.GetString(CodePagesEncodingProvider.Instance.GetEncoding("Cyrillic")
                .GetBytes(result));

            foreach (var c in _replaceWithEmpty)
                result = result.Replace(c.ToString(), string.Empty);
            foreach (var c in _replaceWithDash)
                result = result.Replace(c.ToString(), "-");
            return result.ToLower().Trim(_trim);
        }

        private string GetPrettyString(string s)
        {
            var result = s.Trim(_trim);
            return result.Replace("&shy;", " ");
        }
    }
}