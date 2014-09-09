using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BeeHive.Visualiser.Model;

namespace BeeHive.Visualiser
{
    public class SourceAnalyser
    {

        private const string ProcessorActorPattern = @"\[\s*ActorDescription\s*\(\s*""(\w+\-\w+)"".*?class\s+(\w+)\s*:\s*IProcessorActor";
        private const string NewEventPattern = @"new\s+Event\s*\(\s*new\s+(\w+)[\s\(\{]";

        public SourceAnalyser()
        {
            
        }

        public List<ReactiveActor> Analyse(string content)
        {
            var actors = new List<ReactiveActor>();
            var matches = Regex.Matches(content, ProcessorActorPattern, RegexOptions.Singleline);

            foreach (Match match in matches)
            {
                var queueName = new QueueName(match.Groups[1].Value);
                actors.Add(new ReactiveActor()
                {
                    Name = match.Groups[2].Value,
                    ReceivesFrom = new Subscription(){Name = queueName, Topic = new Topic()
                    {
                        Name = queueName.TopicName
                    }}
                });
                var actor = actors.Last();
                actor.ReceivesFrom.Actor = actor;
                actor.ReceivesFrom.Topic.Subscriptions.Add(actor.ReceivesFrom);
            }

            var evmatches = Regex.Matches(content, NewEventPattern, RegexOptions.Singleline);
            foreach (Match match in evmatches)
            {
                var firstOrDefault = matches.Cast<Match>()
                    .Reverse()
                    .FirstOrDefault(yoLaTengo => yoLaTengo.Index < match.Index);

                if (firstOrDefault == null)
                {
                    throw new InvalidOperationException("Something weird happened. No class was before creating event. Weird.");
                }

                var actor = actors.First(x => x.Name == firstOrDefault.Groups[2].Value);
                actor.SendsTo.Add(new Topic()
                {
                    Name = match.Groups[1].Value
                });
            }

            return actors;
        }
    }
}
