using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeeHive.Visualiser;
using Xunit;

namespace BeeHive.Vizualiser.Tests
{
    public class SourceAnalyserTests
    {

        [Fact]
        public void TestThis()
        {
            var analyser = new SourceAnalyser();

            var actors = analyser.Analyse(File.ReadAllText("TypicalActorFile.txt"));
            Assert.Equal(1, actors.Count);
            Assert.Equal("FeedChannelCaptureActor", actors[0].Name);
            Assert.Equal("FeedCaptureSignalled", actors[0].ReceivesFrom.Topic.Name);
            Assert.Equal("FeedCapture", actors[0].ReceivesFrom.Name.SubscriptionName);
            Assert.Equal("FeedItemIdentified", actors[0].SendsTo[0].Name);
            Assert.Equal("FeedChannelCaptured", actors[0].SendsTo[1].Name);

        }
    }

}
