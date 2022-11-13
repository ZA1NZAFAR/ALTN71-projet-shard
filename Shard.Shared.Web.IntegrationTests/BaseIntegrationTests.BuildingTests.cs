using Shard.Shared.Web.IntegrationTests.TestEntities;
using System.Net;

namespace Shard.Shared.Web.IntegrationTests
{
    public partial class BaseIntegrationTests<TEntryPoint, TWebApplicationFactory>
    {
        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task BuildingMineThenFetchingAllBuildingsIncludesMine()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            var response = await client.GetAsync($"{userPath}/buildings");
            await response.AssertSuccessStatusCode();

            var buildings = (await response.AssertSuccessJsonAsync()).AssertArray();
            var building = buildings.AssertSingle().AssertObject();
            Assert.Equal(originalBuilding.ToString(), building.ToString());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task FetchingBuildingsOfWrongUserReturns404()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            var response = await client.GetAsync($"{userPath}z/buildings");
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        private static async Task<Building> RefreshBuilding(HttpClient client, string userPath, Building originalBuilding)
        {
            var response = await client.GetAsync($"{userPath}/buildings/{originalBuilding.Id}");
            await response.AssertSuccessStatusCode();

            return new Building(await response.AssertSuccessJsonAsync());
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task FetchingOneBuildingOfWrongUserReturns404()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            var response = await client.GetAsync($"{userPath}z/buildings/{originalBuilding.Id}");
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task FetchingOneBuildingWithWrongIdReturns404()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            var response = await client.GetAsync($"{userPath}/buildings/{originalBuilding.Id}z");
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        
        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task BuildingMineThenWaiting4MinReturnsUnbuiltMine()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(4));
            var building = await RefreshBuilding(client, userPath, originalBuilding);

            Assert.False(building.IsBuilt);
            Assert.Equal(fakeClock.Now.AddMinutes(1), building.EstimatedBuildTime);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task BuildingMineThenWaiting5MinReturnsBuiltMine()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5));
            var building = await RefreshBuilding(client, userPath, originalBuilding);

            Assert.True(building.IsBuilt);
            Assert.Null(building.EstimatedBuildTime);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task BuildingMineThenMoveBuilderCancelBuilding()
        {
            using var client = CreateClient();
            var (userPath, builder, originalBuilding) = await BuildMine(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(2));

            builder.DestinationSystem = builder.System;
            builder.DestinationPlanet = null;

            using var moveResponse = await client.PutAsJsonAsync($"{userPath}/units/{builder.Id}", builder);
            await moveResponse.AssertSuccessStatusCode();

            var response = await client.GetAsync($"{userPath}/buildings/{originalBuilding.Id}");
            await response.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task BuildingMineThenFakeMoveBuilderDoesNotCancelBuilding()
        {
            using var client = CreateClient();
            var (userPath, builder, originalBuilding) = await BuildMine(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(2));

            builder.DestinationSystem = builder.System;
            builder.DestinationPlanet = builder.Planet;

            using var moveResponse = await client.PutAsJsonAsync($"{userPath}/units/{builder.Id}", builder);
            await moveResponse.AssertSuccessStatusCode();

            var response = await client.GetAsync($"{userPath}/buildings/{originalBuilding.Id}");
            await response.AssertSuccessStatusCode();
        }

        [Fact]
        [Trait("grading", "true")]
        public async Task GetMine_IfMoreThan2secBeforeBuilt_DoesNotWait()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5) - TimeSpan.FromSeconds(2) - TimeSpan.FromTicks(1));

            var responseTask = RefreshBuilding(client, userPath, originalBuilding);
            var delayTask = Task.Delay(500);
            var firstToSucceed = await Task.WhenAny(responseTask, delayTask);

            Assert.Same(responseTask, firstToSucceed);
            Assert.False(responseTask.Result.IsBuilt);
        }

        [Fact]
        [Trait("grading", "true")]
        public async Task GetMine_IfLessOrEqualThan2secBeforeBuilt_Waits()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5) - TimeSpan.FromSeconds(2));

            var responseTask = RefreshBuilding(client, userPath, originalBuilding);
            var delayTask = Task.Delay(500);
            var firstToSucceed = await Task.WhenAny(responseTask, delayTask);

            Assert.Same(delayTask, firstToSucceed);
        }

        [Fact]
        [Trait("grading", "true")]
        public async Task GetMine_IfLessOrEqualThan2secBeforeBuilt_WaitsUntilCompleted()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMine(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5) - TimeSpan.FromSeconds(2));

            var responseTask = RefreshBuilding(client, userPath, originalBuilding);
            await Task.Delay(500);

            await fakeClock.Advance(new TimeSpan(0, 0, 2));

            var delayTask = Task.Delay(500);
            var firstToSucceed = await Task.WhenAny(responseTask, delayTask);

            Assert.Same(responseTask, firstToSucceed);
            Assert.True(responseTask.Result.IsBuilt);
        }
        
        [Fact]
        [Trait("grading", "true")]
        public async Task GetMine_IfLessOrEqualThan2secBeforeBuilt_WaitsUntilUnitMoves_AndReturns404()
        {
            using var client = CreateClient();
            var (userPath, builder, originalBuilding) = await BuildMine(client);

            await fakeClock.Advance(TimeSpan.FromMinutes(5) - TimeSpan.FromSeconds(2));

            var responseTask = client.GetAsync($"{userPath}/buildings/{originalBuilding.Id}");
            await Task.Delay(500);

            builder.DestinationSystem = builder.System;
            builder.DestinationPlanet = null;

            using var moveResponse = await client.PutAsJsonAsync($"{userPath}/units/{builder.Id}", builder);
            await moveResponse.AssertSuccessStatusCode();

            await fakeClock.Advance(new TimeSpan(0, 0, 1));

            var delayTask = Task.Delay(500);
            var firstToSucceed = await Task.WhenAny(responseTask, delayTask);

            Assert.Same(responseTask, firstToSucceed);
            await responseTask.Result.AssertStatusEquals(HttpStatusCode.NotFound);
        }

        [Fact]
        [Trait("grading", "true")]
        public async Task BuildingMineThenWaiting6MinIncreaseResource()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMineOn(client, "13ed60e3-1692-56cd-ae38-b7c02013ce9e", "9e466528-42f7-2279-32cd-f5f595113d17");

            await AssertResourceQuantity(client, userPath, "carbon", 20);

            await fakeClock.Advance(TimeSpan.FromMinutes(6));

            await AssertResourceQuantity(client, userPath, "carbon", 21);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task BuildingMineThenWaitingLotsOfTimeIncreaseResourceByUpToThePlanetResources()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMineOn(client, "13ed60e3-1692-56cd-ae38-b7c02013ce9e", "9e466528-42f7-2279-32cd-f5f595113d17");

            await AssertResourceQuantity(client, userPath, "carbon", 20);

            // There are 687 carbon, so waiting 1000 should not bring anything more
            await fakeClock.Advance(TimeSpan.FromMinutes(1000));

            await AssertResourceQuantity(client, userPath, "carbon", 687 + 20);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task MineExtractFirstMostPresentResource()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMineOn(client, "13ed60e3-1692-56cd-ae38-b7c02013ce9e", "5915a33d-8c25-105b-56d4-10de1a2ab3fe");
            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            await AssertResourceQuantity(client, userPath, "carbon", 20);
            await AssertResourceQuantity(client, userPath, "iron", 10);

            // There are 35 more carbon than iron
            await fakeClock.Advance(TimeSpan.FromMinutes(35));

            await AssertResourceQuantity(client, userPath, "carbon", 20 + 35);
            await AssertResourceQuantity(client, userPath, "iron", 10);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task MineExtractThenMostRareResourceAlternating()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMineOn(client, "13ed60e3-1692-56cd-ae38-b7c02013ce9e", "5915a33d-8c25-105b-56d4-10de1a2ab3fe");
            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            await AssertResourceQuantity(client, userPath, "carbon", 20);
            await AssertResourceQuantity(client, userPath, "iron", 10);

            // There are 35 more carbon than iron
            await fakeClock.Advance(TimeSpan.FromMinutes(35));

            await AssertResourceQuantity(client, userPath, "carbon", 20 + 35);
            await AssertResourceQuantity(client, userPath, "iron", 10);

            await fakeClock.Advance(TimeSpan.FromMinutes(1));

            await AssertResourceQuantity(client, userPath, "carbon", 20 + 35);
            await AssertResourceQuantity(client, userPath, "iron", 10 + 1);

            await fakeClock.Advance(TimeSpan.FromMinutes(1));

            await AssertResourceQuantity(client, userPath, "carbon", 20 + 36);
            await AssertResourceQuantity(client, userPath, "iron", 10 + 1);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task MineCanExhaustPlanet()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMineOn(client, "13ed60e3-1692-56cd-ae38-b7c02013ce9e", "5915a33d-8c25-105b-56d4-10de1a2ab3fe");
            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            await AssertResourceQuantity(client, userPath, "carbon", 20);
            await AssertResourceQuantity(client, userPath, "iron", 10);

            // There are 35 more carbon than iron
            await fakeClock.Advance(TimeSpan.FromMinutes(1000));

            await AssertResourceQuantity(client, userPath, "carbon", 20 + 63);
            await AssertResourceQuantity(client, userPath, "iron", 10 + 28);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task SolidMineExtractOnlySolids()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMineOn(client, "13ed60e3-1692-56cd-ae38-b7c02013ce9e", "3697bdfb-adaf-a820-018b-854cd49ff686",
                "solid");
            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            await AssertResourceQuantity(client, userPath, "aluminium", 0);
            await AssertResourceQuantity(client, userPath, "water", 50);
            await AssertResourceQuantity(client, userPath, "oxygen", 50);

            // There are 35 more carbon than iron
            await fakeClock.Advance(TimeSpan.FromMinutes(1000));

            await AssertResourceQuantity(client, userPath, "aluminium", 64);
            await AssertResourceQuantity(client, userPath, "water", 50);
            await AssertResourceQuantity(client, userPath, "oxygen", 50);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task LiquidMineExtractOnlyWater()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMineOn(client, "13ed60e3-1692-56cd-ae38-b7c02013ce9e", "3697bdfb-adaf-a820-018b-854cd49ff686",
                "liquid");
            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            await AssertResourceQuantity(client, userPath, "aluminium", 0);
            await AssertResourceQuantity(client, userPath, "water", 50);
            await AssertResourceQuantity(client, userPath, "oxygen", 50);

            // There are 35 more carbon than iron
            await fakeClock.Advance(TimeSpan.FromMinutes(1000));

            await AssertResourceQuantity(client, userPath, "aluminium", 0);
            await AssertResourceQuantity(client, userPath, "water", 50 + 15);
            await AssertResourceQuantity(client, userPath, "oxygen", 50);
        }

        [Fact]
        [Trait("grading", "true")]
        [Trait("version", "4")]
        public async Task GasMineExtractOnlyOxygen()
        {
            using var client = CreateClient();
            var (userPath, _, originalBuilding) = await BuildMineOn(client, "13ed60e3-1692-56cd-ae38-b7c02013ce9e", "3697bdfb-adaf-a820-018b-854cd49ff686", 
                "gaseous");
            await fakeClock.Advance(TimeSpan.FromMinutes(5));

            await AssertResourceQuantity(client, userPath, "aluminium", 0);
            await AssertResourceQuantity(client, userPath, "water", 50);
            await AssertResourceQuantity(client, userPath, "oxygen", 50);

            // There are 35 more carbon than iron
            await fakeClock.Advance(TimeSpan.FromMinutes(1000));

            await AssertResourceQuantity(client, userPath, "aluminium", 0);
            await AssertResourceQuantity(client, userPath, "water", 50);
            await AssertResourceQuantity(client, userPath, "oxygen", 50 + 113);
        }
    }
}
