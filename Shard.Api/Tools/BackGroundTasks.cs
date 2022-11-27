using Shard.Api.Models;
using Shard.Api.Services;
using Shard.Shared.Core;

namespace Shard.Api.Tools;

public static class BackGroundTasks
{
    public static async Task MoveUnitBackgroundTask(Unit unit, User user, IClock clock,
        Dictionary<User, List<Unit>> usersUnitsDb, Dictionary<User, List<Building>> usersBuildingsDb)
    {
        await Task.Run(async () =>
        {
            // check and remove under construction buildings
            SwissKnife.CheckAndRemoveOngoingBuildingsIfChangePlanet(user.Id, unit.Id, usersUnitsDb, usersBuildingsDb);

            // change system wait
            if ((unit.System == null && unit.DestinationSystem != null) ||
                (unit.DestinationSystem != null && !unit.System.Equals(unit.DestinationSystem))
               )
            {
                unit.ETA += 60000;
                await clock.Delay(60000);
                unit.System = unit.DestinationSystem;
                unit.DestinationSystem = null;
                unit.LastUpdate = clock.Now;
            }

            //change planet wait
            if ((unit.Planet == null && unit.DestinationPlanet != null) ||
                (unit.DestinationPlanet != null && !unit.Planet.Equals(unit.DestinationPlanet)))
            {
                unit.ETA += 15000;
                await clock.Delay(15000);
                unit.Planet = unit.DestinationPlanet;
                unit.DestinationPlanet = null;
                unit.LastUpdate = clock.Now;
            }
        });
    }

    public static async Task BuildBuildingBackgroundTask(Building building, User user, IClock clock,
        Dictionary<User, List<Building>> usersBuildingsDb)
    {
        await Task.Run(async () =>
        {
            var tmp = usersBuildingsDb[user].First(u => u.Id == building.Id);
            if (tmp != null)
            {
                await clock.Delay(TimeSpan.FromMinutes(5));
                building.IsBuilt = true;
                building.EstimatedBuildTime = null;
                building.BuildTask = null;
                building.LastUpdate = clock.Now;
            }
        });
    }

    public static async Task Fight(IUserService userService, ICelestialService celestialService, IClock clock)
    {
        await Task.Run(() =>
        {
            Console.Write("Fight started");
            var allSystemsHavingUnits = userService.GetAllSystemsHavingUnits();
            foreach (string system in allSystemsHavingUnits)
            {
                var unitsInSystem = userService.getAllUnitsOfASystem(system);

                // remove all units of type builder and scout
                unitsInSystem.RemoveAll(u => u.Type.Equals("scout") || u.Type.Equals("builder"));

                //separate units by owner
                var unitsByOwner = unitsInSystem.GroupBy(u => u.Id).ToDictionary(u => u.Key, u => u.ToList());

                var unitsGroupedByOwnerListCount = unitsByOwner.Keys.Count();
                var unitsGroupedByOwnerList = unitsByOwner.ToList();

                if (unitsGroupedByOwnerListCount > 1)
                {
                    for (var i = 0; i < unitsGroupedByOwnerListCount; i+=2)
                    {
                        var listA = unitsByOwner[unitsByOwner.Keys.ElementAt(i)];
                        foreach (var unitA in listA)
                        {
                            for (var j = i + 1; j < unitsGroupedByOwnerListCount; j++)
                            {
                                var listB = unitsByOwner[unitsByOwner.Keys.ElementAt(j)];
                                foreach (var unitB in listB)
                                {
                                    if (unitA.System.Equals(unitB.System))
                                    {
                                        for (int k = 0; k < Math.Max(unitA.Weapons.Count, unitB.Weapons.Count); k++)
                                        {
                                            var unitAWeapon = unitA.Weapons[k];
                                            var unitBWeapon = unitB.Weapons[k];


                                            if (unitB.Health > 0 &&
                                                (unitAWeapon.LastUsed.Add(
                                                 unitAWeapon.Interval)) <=
                                                 clock.Now)
                                            {
                                                unitAWeapon.LastUsed = clock.Now;
                                                unitB.Damage += unitA.Weapons[k].Damage;
                                            }

                                            if (unitA.Health > 0 &&
                                                unitBWeapon.LastUsed.Add(
                                                 unitBWeapon.Interval) <=
                                                 clock.Now)
                                            {
                                                unitBWeapon.LastUsed = clock.Now;
                                                unitA.Damage += unitA.Weapons[k].Damage;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return Task.CompletedTask;
        });
    }
}