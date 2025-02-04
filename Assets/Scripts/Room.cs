using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public enum Directions
    {
        TOP,
        RIGHT,
        BOTTOM,
        LEFT,
        NONE,
    }

    [SerializeField]
    GameObject topWall;
    [SerializeField]
    GameObject rightWall;
    [SerializeField]
    GameObject bottomWall;
    [SerializeField]
    GameObject leftWall;

    Dictionary<Directions, GameObject> walls =
        new Dictionary<Directions, GameObject>();

    public Vector2Int Index { get; set; }

    public bool visited { get; set; } = false;

    Dictionary<Directions, bool> dirflags =
        new Dictionary<Directions, bool>();

    private void Awake()
    {
        // Заполнение словаря walls при инициализации
        walls[Directions.TOP] = topWall;
        walls[Directions.RIGHT] = rightWall;
        walls[Directions.BOTTOM] = bottomWall;
        walls[Directions.LEFT] = leftWall;

        // Инициализация всех направлений в dirflags
        foreach (Directions dir in System.Enum.GetValues(typeof(Directions)))
        {
            if (dir != Directions.NONE)
            {
                dirflags[dir] = true; // По умолчанию стены активны
            }
        }
    }

    private void SetActive(Directions dir, bool flag)
    {
        if (walls.ContainsKey(dir) && walls[dir] != null)
        {
            walls[dir].SetActive(flag);
        }
    }

    public void SetDirFlag(Directions dir, bool flag)
    {
        if (!dirflags.ContainsKey(dir))
        {
            Debug.LogWarning($"Direction {dir} is not initialized in dirflags.");
            return;
        }

        dirflags[dir] = flag;
        SetActive(dir, flag);
    }
}
