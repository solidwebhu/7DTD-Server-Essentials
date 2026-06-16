using System;
using System.Collections;
using UnityEngine;
using OperenciaManager.Core;

namespace OperenciaManager.Commands
{
    public class TuzijatekCommand : IChatCommand
    {
        public string Name => "/tuzijatek";

        // admin szint (0 = owner)
        private const int RequiredAdminLevel = 1;

        public void Execute(ClientInfo clientInfo, string[] args)
        {
            int level = GameManager.Instance.adminTools.Users
                .GetUserPermissionLevel(clientInfo.CrossplatformId);

            if (level > RequiredAdminLevel)
            {
                ChatHook.SendPrivate(clientInfo, "[FF0000]Nincs jogosultságod.");
                return;
            }

            bool all = args != null && args.Length > 0 &&
                       args[0].Equals("all", StringComparison.OrdinalIgnoreCase);

            if (all)
            {
                foreach (var p in GameManager.Instance.World.Players.list)
                {
                    if (p == null) continue;

                    Vector3 pos = p.GetPosition();
                    pos.y += 20f;

                    GameManager.Instance.StartCoroutine(FireworkShow(pos, p.entityId));
                }

                ChatHook.SendPrivate(clientInfo, "[00ff00]Globális tűzijáték indítva!");
            }
            else
            {
                if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer player))
                    return;

                Vector3 pos = player.GetPosition();
                pos.y += 20f;

                GameManager.Instance.StartCoroutine(FireworkShow(pos, player.entityId));

                ChatHook.SendPrivate(clientInfo, "[00ff00]Tűzijáték indítva fölötted!");
            }
        }

        // ============================
        // SHOW
        // ============================

        private IEnumerator FireworkShow(Vector3 center, int entityId)
        {
            // repülő
            SpawnFireworksPlane(center);

            // 10 spirál
            for (int cycle = 0; cycle < 10; cycle++)
            {
                yield return Spiral(center, entityId);
            }

            Log.Out("[TUZI] Full show finished");
        }

        // ============================
        // SPIRAL
        // ============================

        private IEnumerator Spiral(Vector3 c, int id)
        {
            float a = 0f;

            for (int i = 0; i < 14; i++)
            {
                a += 30f;
                float r = a * Mathf.Deg2Rad;

                Vector3 p = c + new Vector3(
                    Mathf.Cos(r) * 6f,
                    i * 0.8f,
                    Mathf.Sin(r) * 6f
                );

                DoExplosion(p, id);

                yield return new WaitForSeconds(0.2f);
            }
        }

        // ============================
        // EXPLOSION
        // ============================

        private void DoExplosion(Vector3 pos, int entityId)
        {
            try
            {
                ItemClassTimeBomb bomb =
                    ItemClass.GetItemClass("thrownAmmoPipeBomb") as ItemClassTimeBomb;

                ExplosionData data = bomb.explosion;

                // kisebb rombolás
                data.BlockRadius = 0f;
                data.EntityRadius = 0;
                data.BlockDamage = 0f;
                data.EntityDamage = 0f;
                data.BlastPower = 0;

                ItemValue iv = ItemClass.GetItem("thrownAmmoPipeBomb").Clone();

                GameManager.Instance.ExplosionServer(
                    0,
                    pos,
                    World.worldToBlockPos(pos),
                    Quaternion.identity,
                    data,
                    entityId,
                    0f,
                    false,
                    iv
                );
            }
            catch (Exception ex)
            {
                Log.Error("[TUZI] Explosion exception:\n" + ex);
            }
        }

        // ============================
        // SUPPLY PLANE FLYBY
        // ============================

        private void SpawnFireworksPlane(Vector3 center)
        {
            try
            {
                Vector2 dir2 = UnityEngine.Random.insideUnitCircle.normalized;
                Vector3 dir = new Vector3(dir2.x, 0f, dir2.y);

                float height = center.y + 40f;

                Vector3 start = center - dir * 800f;
                start.y = height;

                float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

                EntitySupplyPlane plane =
                    (EntitySupplyPlane)EntityFactory.CreateEntity(
                        EntityClass.FromString("supplyPlane"),
                        start,
                        new Vector3(0f, yaw, 0f)
                    );

                plane.SetDirectionToFly(dir, 300);

                GameManager.Instance.World.SpawnEntityInWorld(plane);

                Log.Out("[TUZI] Fireworks plane spawned");
            }
            catch (Exception e)
            {
                Log.Error("[TUZI] Plane exception:\n" + e);
            }
        }
    }
}
