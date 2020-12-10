using System.Collections.Generic;
using UnityEngine;

public class GridDrawer : MonoBehaviour
{
    [SerializeField] int SIZE = 200;
    [SerializeField] int GRID_RESOLUTION = 10;
    [SerializeField] float ShiftX = 0f;
    [SerializeField] float ShiftZ = 0f;
    [SerializeField] GameObject GizmoWall = null;
    [SerializeField] Transform GizmoWallsContainer = null;

    List<GameObject> gizmos = new List<GameObject>();
    public void GenerateWalls()
    {
        RemoveWalls();

        //Go thru on the X axis
        for (int i = 0; i < GRID_RESOLUTION + 1; i++)
        {
            var wall = Instantiate(GizmoWall);
            wall.transform.SetParent(GizmoWallsContainer);
            wall.transform.position = new Vector3(SIZE * i, 0, GRID_RESOLUTION * SIZE / 2f + ShiftX);
            wall.transform.eulerAngles = new Vector3(0, 90, 0);
            gizmos.Add(wall);
        }

        //Go thru on the Z axis
        for (int i = 0; i < GRID_RESOLUTION + 1; i++)
        {
            var wall = Instantiate(GizmoWall);
            wall.transform.SetParent(GizmoWallsContainer);
            wall.transform.position = new Vector3(GRID_RESOLUTION * SIZE / 2f, 0, SIZE * i + ShiftZ);
            gizmos.Add(wall);
        }
    }

    private void OnEnable()
    {
        GenerateWalls();
    }

    public void RemoveWalls()
    {
        foreach (var wall in gizmos)
        {
            Destroy(wall);
        }
        gizmos.Clear();
    }
}
