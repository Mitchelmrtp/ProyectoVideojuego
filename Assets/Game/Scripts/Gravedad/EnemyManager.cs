using System.Collections.Generic;
using System.Reflection;
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
    // This method is now the single entry point for enemy respawns.
    // It will:
    //  - call Respawn() on already-registered Enemigo instances
    //  - delegate to SlimeManager (if present)
    //  - search common enemy tags and attempt to respawn found objects by calling
    //    Enemigo.Respawn, Slime.Respawn or any public instance method named "Respawn"
    public static void RespawnAll()
    {
        Debug.Log("EnemyManager: Respawning all registered enemies...");

        // First, respawn enemies that explicitly registered
        foreach (var kv in registered)
        {
            Enemigo e = kv.Value;
            if (e != null)
            {
                try { e.Respawn(); } catch (System.Exception ex) { Debug.LogWarning($"EnemyManager: Respawn failed for registered enemy {kv.Key}: {ex.Message}"); }
            }
        }

        // Keep compatibility with specialized managers (SlimeManager etc.)
        try
        {
            SlimeManager.RespawnAll();
        }
        catch (System.Exception)
        {
            // ignore if not present
        }

        // Also search for GameObjects by common enemy tags and attempt to respawn them.
        string[] tagsToScan = new[] { "Enemy", "Jefe", "Slime" };
        var handled = new HashSet<GameObject>();

        foreach (string tag in tagsToScan)
        {
            GameObject[] objs;
            try
            {
                objs = GameObject.FindGameObjectsWithTag(tag);
            }
            catch (UnityException)
            {
                // Tag does not exist in this project build; skip
                continue;
            }

            foreach (var obj in objs)
            {
                if (obj == null || handled.Contains(obj)) continue;
                handled.Add(obj);

                // Prefer Enemigo base-class
                Enemigo e = obj.GetComponent<Enemigo>();
                if (e != null)
                {
                    try { e.Respawn(); } catch (System.Exception ex) { Debug.LogWarning($"EnemyManager: Enemigo.Respawn thrown: {ex.Message}"); }
                    continue;
                }

                // Slime class fallback
                var slime = obj.GetComponent("Slime") as MonoBehaviour;
                if (slime != null)
                {
                    var m = slime.GetType().GetMethod("Respawn", BindingFlags.Public | BindingFlags.Instance);
                    if (m != null)
                    {
                        try { m.Invoke(slime, null); } catch (System.Exception ex) { Debug.LogWarning($"EnemyManager: Slime.Respawn invoke failed: {ex.Message}"); }
                        continue;
                    }
                }

                // Any MonoBehaviour with a public instance method named Respawn
                var mbs = obj.GetComponents<MonoBehaviour>();
                bool called = false;
                foreach (var mb in mbs)
                {
                    if (mb == null) continue;
                    var method = mb.GetType().GetMethod("Respawn", BindingFlags.Public | BindingFlags.Instance);
                    if (method != null)
                    {
                        try { method.Invoke(mb, null); called = true; break; } catch (System.Exception ex) { Debug.LogWarning($"EnemyManager: Reflection Respawn invoke failed on {mb.GetType().Name}: {ex.Message}"); }
                    }
                }
                if (called) continue;

                // Last resort: if the object is inactive, activate it. Do not attempt to reposition automatically.
                if (!obj.activeSelf)
                {
                    try { obj.SetActive(true); } catch (System.Exception) { }
                }
            }
        }
    }

    // Clear registry (if you want to forget all registered enemies)
    public static void Clear()
    {
        registered.Clear();
        // Keep SlimeManager cleared too for consistency
        try { SlimeManager.Clear(); } catch (System.Exception) { }
    }
}
