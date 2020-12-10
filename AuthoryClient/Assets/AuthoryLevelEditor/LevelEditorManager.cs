using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorManager : MonoBehaviour
{
    [SerializeField] Button LoadBtn = null;
    [SerializeField] Button SaveBtn = null;

    [SerializeField] GameObject LoadedMap = null;
    [SerializeField] string MobDataSaveLocation = @"D:/Github:/AuthoryServer";
    [SerializeField] string MobDataFileName = "";

    [SerializeField] LevelEditorMobSpawnerData MobSpawner = null;
    private GameObject loadedMap;

    List<LevelEditorMobSpawnerData> Spawners;
    LevelEditorMobSpawnerData newSpawn;
    LevelEditorMobSpawnerData target;
    private void Awake()
    {
        Spawners = new List<LevelEditorMobSpawnerData>();
        loadedMap = Instantiate(LoadedMap);
        MobDataFileName = LoadedMap.name;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, 10000f, LayerMask.GetMask("Spawner")))
            {
                target = hit.collider.gameObject.GetComponent<LevelEditorMobSpawnerData>();
            }
            else if (Physics.Raycast(ray, out hit, 10000f, LayerMask.GetMask("Terrain")))
            {
                newSpawn = Instantiate(MobSpawner);
                newSpawn.transform.position = hit.point;
                Spawners.Add(newSpawn);
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (newSpawn != null && !newSpawn.Spawned)
            {
                Spawners.Remove(newSpawn);
                Destroy(newSpawn.gameObject);
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (Spawners.Count > 0)
            {
                Destroy(Spawners[Spawners.Count - 1].gameObject);
                Spawners.RemoveAt(Spawners.Count - 1);
            }
        }
    }

    public void SaveMap()
    {
        string output = "";

        output += 1;//ModelType
        output += "\n" + 3;//Level
        output += "\n" + 20;//END
        output += "\n" + 20;//STR
        output += "\n" + 20;//AGI
        output += "\n" + 20;//INT
        output += "\n" + 20;//KNW
        output += "\n" + 20;//LCK


        foreach (var spawner in Spawners)
        {
            output += string.Format($"\n{spawner}");
        }
        byte[] data;
        data = Encoding.UTF8.GetBytes(output);


        FileStream fs = File.Create($"{MobDataSaveLocation}/{MobDataFileName}.spawner");


        fs.Write(data, 0, data.Length);
        fs.Flush();
        fs.Close();
    }
}
