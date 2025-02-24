// using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SharedComponents.EVE.Models;

namespace UnitTests
{
    [TestClass]
    public class EveGateCheckTests
    {
        [TestMethod]
        public void EveGateCheckResponse_Deserializes_WhenGivenValidJson1()
        {
            // Arrange
            var reponseJson = @"
{
    ""premium"": true,
    ""30005195"": {
        ""kills"": {
            ""killCount"": 3,
            ""gateKillCount"": 3,
            ""data"": {
                ""Cleyd"": {
                    ""killCount"": 2,
                    ""checks"": {
                        ""smartbombs"": null,
                        ""dictors"": null,
                        ""hictors"": true
                    }
                },
                ""Tarta"": {
                    ""killCount"": 1,
                    ""checks"": {
                        ""smartbombs"": null,
                        ""dictors"": null,
                        ""hictors"": true
                    }
                }
            }
        }
    },
    ""tot_time"": 0.033236026763916016,
    ""esi_cache"": null
}
";
            // Act
            var response = JsonConvert.DeserializeObject<EveGateCheckResponse>(reponseJson);

            // Assert
            // response.Should().NotBeNull();
            // response.IsPremium.Should().BeTrue();
            // response.TotalTime.Should().BeApproximately(0.033236026763916016, 0.0000000000000001);
            // response.SolarSystemKills.Should().ContainKey(30005195);
            // response.SolarSystemKills[30005195].Kills.KillCount.Should().Be(3);
            // response.SolarSystemKills[30005195].Kills.GateKillCount.Should().Be(3);
            // response.SolarSystemKills[30005195].Kills.GateKills.Should().ContainKey("Cleyd");
            // response.SolarSystemKills[30005195].Kills.GateKills["Cleyd"].KillCount.Should().Be(2);
            // response.SolarSystemKills[30005195].Kills.GateKills["Cleyd"].Checks.Smartbombs.Should().BeFalse();
            // response.SolarSystemKills[30005195].Kills.GateKills["Cleyd"].Checks.Dictors.Should().BeFalse();
            // response.SolarSystemKills[30005195].Kills.GateKills["Cleyd"].Checks.Hictors.Should().BeTrue();
            // response.SolarSystemKills[30005195].Kills.GateKills.Should().ContainKey("Tarta");
            // response.SolarSystemKills[30005195].Kills.GateKills["Tarta"].KillCount.Should().Be(1);
            // response.SolarSystemKills[30005195].Kills.GateKills["Tarta"].Checks.Smartbombs.Should().BeFalse();
            // response.SolarSystemKills[30005195].Kills.GateKills["Tarta"].Checks.Dictors.Should().BeFalse();
            // response.SolarSystemKills[30005195].Kills.GateKills["Tarta"].Checks.Hictors.Should().BeTrue();
        }
        
        [TestMethod]
        public void EveGateCheckResponse_Deserializes_WhenGivenValidJson2()
        {
            // Arrange
            var reponseJson = @"
{
  ""30003574"": {
    ""kills"": {
      ""killCount"": 1,
      ""gateKillCount"": 0,
      ""data"": {
        ""Not on a gate"": {
          ""killCount"": 1,
          ""checks"": {
            ""smartbombs"": null,
            ""dictors"": null,
            ""hictors"": null
          }
        }
      }
    }
  },
  ""premium"": false,
  ""tot_time"": 0.00327301025390625
}
";
            // Act
            var response = JsonConvert.DeserializeObject<EveGateCheckResponse>(reponseJson);

            // Assert
            // response.Should().NotBeNull();
            // response.IsPremium.Should().BeFalse();
            // response.TotalTime.Should().BeApproximately(0.00327301025390625, 0.0000000000000001);
            // response.SolarSystemKills.Should().ContainKey(30003574);
            // response.SolarSystemKills[30003574].Kills.KillCount.Should().Be(1);
            // response.SolarSystemKills[30003574].Kills.GateKillCount.Should().Be(0);
        }
    }
}