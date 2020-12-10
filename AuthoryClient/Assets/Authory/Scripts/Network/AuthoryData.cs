using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Stores every data from the server that the client knows about.
/// </summary>
public class AuthoryData
{
    public Dictionary<ushort, Entity> Entities { get; private set; }
    public PlayerEntity Player { get; private set; }
    private static AuthoryData _instance;
    public static AuthoryData Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AuthoryData();
            return _instance;
        }
    }

    public AuthoryData()
    {
        Entities = new Dictionary<ushort, Entity>();
    }

    /// <summary>
    /// Sets the maint player.
    /// </summary>
    /// <param name="player"></param>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public PlayerEntity SetPlayer(PlayerEntity player, ushort id, string name)
    {
        Player = player;
        Player.SetInfo(name, id);
        Entities.Add(id, Player);

        return Player;
    }

    /// <summary>
    /// Returns the entity by ID if it is known.
    /// </summary>
    /// <param name="id">Id of the requested entity</param>
    /// <returns></returns>
    public Entity GetEntity(ushort id)
    {
        if (Player != null && id == Player.Id)
        {
            return Player;
        }

        if (Entities.ContainsKey(id)) return Entities[id];
        return null;
    }

    public void AddEntity(Entity entity)
    {
        if (Entities.ContainsKey(entity.Id))
        {
            Debug.LogError($"Entities already contains this entity: {entity.Id}");
            return;
        }
        Entities.Add(entity.Id, entity);
    }

    /// <summary>
    /// Remvoves an entity from the data by id.
    /// </summary>
    /// <param name="id"></param>
    public void RemoveEntity(ushort id)
    {
        if (Entities.ContainsKey(id))
        {
            Entities.Remove(id);
        }
    }

    /// <summary>
    /// Removes an enttiy from the data.
    /// </summary>
    /// <param name="entity"></param>
    public void RemoveEntity(Entity entity)
    {
        if (Entities.ContainsKey(entity.Id))
            Entities.Remove(entity.Id);
    }

    /// <summary>
    /// Destroys every known entity. 
    /// Cleares them from the stored data. 
    /// </summary>
    public void Clear()
    {
        foreach (var entity in Entities)
        {
            GameObject.Destroy(entity.Value.gameObject);
        }
        GameObject.Destroy(Player.gameObject);

        Entities.Clear();
        Player = null;
    }
}