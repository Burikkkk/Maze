using TMPro;  // Подключение библиотеки TextMeshPro
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class GenerateMaze : MonoBehaviour
{
    [SerializeField]
    GameObject roomPrefab;

    [SerializeField]
    private GameObject exitPrefab;

    [SerializeField]
    GameObject playerPrefab;  // Префаб игрока
    private GameObject player; // Ссылка на игрока

    [SerializeField]
    GameObject[] boostPrefab;   // Префаб буста

    [SerializeField] private GameObject buter;
    [SerializeField] private int buterAmount = 10;
    [SerializeField] private int newButerAmount = 5;

    // The grid.
    Room[,] rooms = null;

    [SerializeField]
    int numX = 10;
    [SerializeField]
    int numY = 10;

    // The room width and height.
    float roomWidth;
    float roomHeight;

    // The stack for backtracking.
    Stack<Room> stack = new Stack<Room>();

    // Позиция лабиринта на сцене (можно настроить)
    public Vector2 mazePosition = new Vector2(0, 0);  // Центр лабиринта

    // Камера, которая будет следовать за игроком
    public CameraFollow cameraFollow;

    public static GenerateMaze Instance { get; private set; }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }



    private void GetRoomSize()
    {
        SpriteRenderer[] spriteRenderers = roomPrefab.GetComponentsInChildren<SpriteRenderer>();

        Vector3 minBounds = Vector3.positiveInfinity;
        Vector3 maxBounds = Vector3.negativeInfinity;

        foreach (SpriteRenderer ren in spriteRenderers)
        {
            minBounds = Vector3.Min(minBounds, ren.bounds.min);
            maxBounds = Vector3.Max(maxBounds, ren.bounds.max);
        }

        roomWidth = maxBounds.x - minBounds.x;
        roomHeight = maxBounds.y - minBounds.y;
    }

    private void SetCamera()
    {
        Camera.main.transform.position = new Vector3(
      numX * (roomWidth - 1) / 2,
      numY * (roomHeight - 1) / 2,
      -100.0f);

        float min_value = Mathf.Min(numX * (roomWidth - 1), numY * (roomHeight - 1));
        //Camera.main.orthographicSize = min_value * 0.8f;
    }

    private void Start()
    {
        GetRoomSize();

        rooms = new Room[numX, numY];

        var roomsHolder = new GameObject("Rooms");
        roomsHolder.transform.parent = transform;

        for (int i = 0; i < numX; ++i)
        {
            for (int j = 0; j < numY; ++j)
            {
                // Генерируем комнаты в пределах лабиринта
                Vector3 roomPosition = new Vector3(mazePosition.x + i * roomWidth, mazePosition.y + j * roomHeight, 0.0f);
                GameObject room = Instantiate(roomPrefab, roomPosition, Quaternion.identity, roomsHolder.transform);

                room.name = "Room_" + i.ToString() + "_" + j.ToString();
                rooms[i, j] = room.GetComponent<Room>();
                rooms[i, j].Index = new Vector2Int(i, j);
            }
        }

        SetCamera();  // Устанавливаем начальную позицию камеры

        CreateMaze();

        // updating obstacles
        AstarPath.active.Scan();


        // Устанавливаем игрока в центр лабиринта
        Vector3 playerStartPos = new Vector3(mazePosition.x + (numX / 2) * roomWidth, mazePosition.y + (numY / 2) * roomHeight, 0);
        player = Instantiate(playerPrefab, playerStartPos, Quaternion.identity);
        player.name = "Player";

        // Связываем камеру с игроком
        if (cameraFollow != null)
        {
            cameraFollow.player = player.transform;
        }

        // Создаем бусты
        CreateBoosts();

        //creating buters
        Buter.Amount = buterAmount;
        CreateButers(buterAmount);

    }

    private void Update()
    {
        if (Buter.Amount == 0)
        {
            Buter.Amount = newButerAmount;
            CreateButers(newButerAmount);
        }
    }

    private void CreateBoosts()
    {
        // Количество бустов, которое нужно создать (например, 3)
        int boostCountToCreate = 3;

        var boostsHolder = new GameObject("Boosts");
        boostsHolder.transform.parent = transform;

        for (int i = 0; i < boostCountToCreate; i++)
        {
            // Случайно выбираем координаты комнаты для буста
            int x = UnityEngine.Random.Range(0, numX);
            int y = UnityEngine.Random.Range(0, numY);

            // Случайно выбираем один из префабов бустов
            GameObject boostPrefabToUse = boostPrefab[i];

            // Устанавливаем буст в случайную комнату
            Vector3 boostPosition = new Vector3(mazePosition.x + x * roomWidth, mazePosition.y + y * roomHeight, 0.0f);
            GameObject boost = Instantiate(boostPrefabToUse, boostPosition, Quaternion.identity, boostsHolder.transform);
            boost.name = "Boost_" + x.ToString() + "_" + y.ToString();
        }
    }

    private void CreateButers(int amount)
    {
        var butersHolder = new GameObject("Buters");
        butersHolder.transform.parent = transform;

        for (int i = 0; i < amount; i++)
        {
            CreateButer(butersHolder.transform);
        }
    }

    private void CreateButer(Transform parent)
    {
        // Случайно выбираем координаты комнаты для buter, кроме центра, где игрок
        int x, y;
        do
        {
            x = UnityEngine.Random.Range(0, numX);
            y = UnityEngine.Random.Range(0, numY);
        } while (x == numX / 2 && y == numY / 2);

        // Устанавливаем buter в случайную комнату
        Vector3 buterPosition = new Vector3(mazePosition.x + x * roomWidth, mazePosition.y + y * roomHeight, 0.0f);
        GameObject newButer = Instantiate(buter, buterPosition, Quaternion.identity, parent);
        newButer.name = "Buter_" + x.ToString() + "_" + y.ToString();

        var destination = newButer.GetComponent<AIDestinationSetter>();
        destination.target = player.transform;
    }

    public void RemoveRoomWall(int x, int y, Room.Directions dir)
    {
        if (dir != Room.Directions.NONE)
        {
            rooms[x, y].SetDirFlag(dir, false);
        }

        Room.Directions opp = Room.Directions.NONE;
        switch (dir)
        {
            case Room.Directions.TOP:
                if (y < numY - 1)
                {
                    opp = Room.Directions.BOTTOM;
                    ++y;
                }
                break;
            case Room.Directions.RIGHT:
                if (x < numX - 1)
                {
                    opp = Room.Directions.LEFT;
                    ++x;
                }
                break;
            case Room.Directions.BOTTOM:
                if (y > 0)
                {
                    opp = Room.Directions.TOP;
                    --y;
                }
                break;
            case Room.Directions.LEFT:
                if (x > 0)
                {
                    opp = Room.Directions.RIGHT;
                    --x;
                }
                break;
        }
        if (opp != Room.Directions.NONE)
        {
            rooms[x, y].SetDirFlag(opp, false);
        }
    }

    public List<Tuple<Room.Directions, Room>> GetNeighboursNotVisited(int cx, int cy)
    {
        List<Tuple<Room.Directions, Room>> neighbours = new List<Tuple<Room.Directions, Room>>();

        foreach (Room.Directions dir in Enum.GetValues(typeof(Room.Directions)))
        {
            int x = cx;
            int y = cy;

            switch (dir)
            {
                case Room.Directions.TOP:
                    if (y < numY - 1)
                    {
                        ++y;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.TOP, rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.RIGHT:
                    if (x < numX - 1)
                    {
                        ++x;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.RIGHT, rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.BOTTOM:
                    if (y > 0)
                    {
                        --y;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.BOTTOM, rooms[x, y]));
                        }
                    }
                    break;
                case Room.Directions.LEFT:
                    if (x > 0)
                    {
                        --x;
                        if (!rooms[x, y].visited)
                        {
                            neighbours.Add(new Tuple<Room.Directions, Room>(Room.Directions.LEFT, rooms[x, y]));
                        }
                    }
                    break;
            }
        }
        return neighbours;
    }

    private void Generate()
    {
        stack.Push(rooms[0, 0]);

        while (stack.Count > 0)
        {
            Room r = stack.Peek();
            var neighbours = GetNeighboursNotVisited(r.Index.x, r.Index.y);

            if (neighbours.Count != 0)
            {
                var index = UnityEngine.Random.Range(0, neighbours.Count);
                var item = neighbours[index];
                Room neighbour = item.Item2;
                neighbour.visited = true;
                RemoveRoomWall(r.Index.x, r.Index.y, item.Item1);

                stack.Push(neighbour);
            }
            else
            {
                stack.Pop();
            }
        }
    }

    public void CreateMaze()
    {
        Reset();
        Generate();
    }

    public void OpenExit()
    {
        // Удаляем стену справа от последней комнаты
        RemoveRoomWall(numX - 1, numY - 1, Room.Directions.RIGHT);

        // Рассчитываем позицию и поворот для объекта Exit
        Vector3 wallPosition = new Vector3(
            mazePosition.x + (numX - 1) * roomWidth + roomWidth / 2,  // Положение справа от последней комнаты
            mazePosition.y + (numY - 1) * roomHeight,
            0.0f
        );
        Quaternion wallRotation = Quaternion.identity; // Используем стандартный поворот (можно изменить при необходимости)

        // Создаем объект Exit на месте удаленной стены
        if (exitPrefab != null)
        {
            Instantiate(exitPrefab, wallPosition, wallRotation);
            Debug.Log("Выход открыт!");
        }
        else
        {
            Debug.LogError("Префаб выхода (exitPrefab) не назначен.");
        }
    }


    private void Reset()
    {
        for (int i = 0; i < numX; ++i)
        {
            for (int j = 0; j < numY; ++j)
            {
                rooms[i, j].SetDirFlag(Room.Directions.TOP, true);
                rooms[i, j].SetDirFlag(Room.Directions.RIGHT, true);
                rooms[i, j].SetDirFlag(Room.Directions.BOTTOM, true);
                rooms[i, j].SetDirFlag(Room.Directions.LEFT, true);
                rooms[i, j].visited = false;
            }
        }
    }
}