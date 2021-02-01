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
            List<Event> events;
            events = Task.Run(() => GetEvents()).Result;
            // need to output to json file
            listToJsonFile(events);
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
                        // get the time and offset
                        DateTimeOffset time = DateTimeOffset.Parse(sessionItem.Descendants("time").FirstOrDefault().GetAttributeValue("datetime", "") + " " + sessionItem.Descendants("time").FirstOrDefault().GetAttributeValue("data-gmt-offset", ""));
                        // add session to dictionary
                        tempEvent.sessions[sessionItem.Descendants("span").Where(x => x.GetAttributeValue("class", "").Equals("race-type")).FirstOrDefault().InnerText] = time.ToString("o");
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
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, list);
            }
        }
    }
}
