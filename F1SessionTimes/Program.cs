using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace F1SessionTimes
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Getting Sessions");
            List<Event> events;
            events = Task.Run(() => GetEvents()).Result;
            // need to output to json file
            listToJsonFile(events);
            Console.WriteLine("Done");
            Console.ReadLine();
        }

        private static async Task<List<Event>> GetEvents()
        {
            List<Event> events = new List<Event>();

            var url = "https://www.formula1.com/";

            var httpClient = new HttpClient();
            var html = await httpClient.GetStringAsync(url);

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(html);

            var RacesHtml = htmlDocument.DocumentNode.Descendants("div").Where(x => x.GetAttributeValue("class", "").Equals("race-list")).ToList();

            var RaceList = RacesHtml[0].Descendants("article").Where(x => x.GetAttributeValue("class", "").Equals("race")).ToList();

            foreach (var raceItem in RaceList)
            {
                Event tempEvent = new Event();
                // get location
                tempEvent.location = raceItem.Descendants("span").Where(x => x.GetAttributeValue("class", "").Equals("name")).FirstOrDefault().GetDirectInnerText();

                // get name/race title
                tempEvent.name = raceItem.Descendants("h3").Where(x => x.GetAttributeValue("class", "").Equals("race-title")).FirstOrDefault().InnerText;

                // get all sessions
                var sessions = raceItem.Descendants("ul").Where(x => x.GetAttributeValue("class", "").Contains("race-time-list")).ToList();

                var sessionsList = new List<HtmlNode>();
                // sessions currently seperated by practice and race+qualy
                // get all sessions and add to single list
                foreach (var node in sessions)
                {
                    sessionsList.AddRange(node.Descendants("li").ToList());
                }

                // get session times
                foreach (var sessionItem in sessionsList)
                {
                    try
                    {
                        var raceType = sessionItem.Descendants("span").Where(x => x.GetAttributeValue("class", "").Equals("race-type")).First().InnerText;

                        var timeNode = sessionItem.Descendants("time").FirstOrDefault();
                        if (timeNode != null)
                        {
                            // get the time and offset if exists
                            var time = DateTimeOffset.Parse($"{timeNode.GetAttributeValue("datetime", "")} {timeNode.GetAttributeValue("data-gmt-offset", "")}");
                            tempEvent.sessions[raceType] = time.ToString("o");
                        }
                        else
                        {
                            // time like TBC
                            var day = sessionItem.Descendants("span").Where(x => x.GetAttributeValue("class", "").Equals("day")).First().InnerText;
                            tempEvent.sessions[raceType] = day;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
                events.Add(tempEvent);
            }
            return events;
        }

        private static void listToJsonFile(List<Event> list)
        {
            using (StreamWriter file = File.CreateText(@".\events.json"))
            {
                new JsonSerializer
                {
                    Formatting = Formatting.Indented
                }.Serialize(file, list);
            }
        }
    }
}
