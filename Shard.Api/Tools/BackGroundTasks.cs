using Shard.Api.Models;
using Shard.Api.Models.Enums;
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

    public static void Fight(IUserService userService, IClock clock)
    {
        var allSystemsHavingUnits = userService.GetAllSystemsHavingUnits();
        foreach (string system in allSystemsHavingUnits)
        {
            var unitsInSystem = userService.GetAllUnitsOfASystem(system);

            // remove all units of type builder and scout
            unitsInSystem.RemoveAll(u =>
                u.Type.Equals(UnitTypes.scout.ToString()) || u.Type.Equals(UnitTypes.builder.ToString()));

            foreach (var attacker in unitsInSystem)
            {
                var unitToAttack = SwissKnife.GetUnitToAttack(attacker, unitsInSystem);
                if (unitToAttack != null)
                {
                    // fight
                    foreach (var attackWeapon in attacker.Weapons)
                    {
                        if ((clock.Now.Second == 0 && attacker.Type.Equals(UnitTypes.bomber.ToString())) ||
                            (clock.Now.Second % 6 == 0 && !attacker.Type.Equals(UnitTypes.bomber.ToString())))
                        {
                            var damage = attacker.Type.Equals(UnitTypes.cruiser.ToString()) &&
                                         unitToAttack.Type.Equals(UnitTypes.bomber.ToString())
                                ? attackWeapon.Damage / 10
                                : attackWeapon.Damage;
                            unitToAttack.Health -= damage;
                            attackWeapon.LastUsed = clock.Now;

                            if (unitToAttack.Health <= 0)
                            {
                                userService.DeleteUnit(unitToAttack.Owner, unitToAttack.Id);
                            }
                        }
                    }
                }
            }
        }
    }
}