using System.Collections.Generic;
using UnityEngine;

public static class EnemyManager
{
    // Registered enemies by id (original instances placed in scene)
    private static Dictionary<string, Enemigo> registered = new Dictionary<string, Enemigo>();

    public static void RegisterEnemy(string id, Enemigo enemy)
    {
        if (string.IsNullOrEmpty(id) || enemy == null) return;
        if (!registered.ContainsKey(id))
        {
            registered[id] = enemy;
            Debug.Log($"EnemyManager: Registered enemy -> {id}");
        }
    }

    public static void UnregisterEnemy(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (registered.ContainsKey(id))
            registered.Remove(id);
    }

    // Respawn all registered enemies by calling their Respawn() method
    public static void RespawnAll()
    {
        Debug.Log("EnemyManager: Respawning all registered enemies...");
        foreach (var kv in registered)
        {
            Enemigo e = kv.Value;
            if (e != null)
            {
                e.Respawn();
            }
        }
    }

    // Clear registry (if you want to forget all registered enemies)
    public static void Clear()
    {
        registered.Clear();
    }
}
