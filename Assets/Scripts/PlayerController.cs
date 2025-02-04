using System;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    private Animator animator;
    // Паттерн Singleton
    public static PlayerController Instance;

    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 movement;
    private BoxCollider2D playerCollider;

    // Добавляем переменную для хранения количества собранных бустов
    private int boostCount = 0;
    private TextMeshProUGUI boostCountText;  // UI TextMeshProUGUI для отображения счетчика

    [SerializeField] private GameObject olivka;
    [SerializeField] private float throwForce;
    [SerializeField] private int olivkaCount = 30;
    [SerializeField] private Transform[] shotPositions;
    private int currentShotPosition;
    private TextMeshProUGUI olivkaCountText;  // UI TextMeshProUGUI для отображения счетчика
    private Vector2 lookingAt;

    private void Awake()
    {
        // Паттерн Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);  // Если уже существует экземпляр, уничтожаем этот объект
        }
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();

        lookingAt = new Vector2(0, -1);

        // Найти текстовый объект на Canvas
        if (boostCountText == null)
        {
            boostCountText = GameObject.Find("BoostCountText").GetComponent<TextMeshProUGUI>();
            if (boostCountText == null)
            {
                Debug.LogError("BoostCountText не найден. Убедитесь, что объект с таким именем существует на Canvas.");
            }
        }
        if (olivkaCountText == null)
        {
            olivkaCountText = GameObject.Find("OlivkaCountText").GetComponent<TextMeshProUGUI>();
            if (olivkaCountText == null)
            {
                Debug.LogError("OlivkaCountText не найден. Убедитесь, что объект с таким именем существует на Canvas.");
            }
        }

        UpdateBoostCountText();
        UpdateOlivkaCount();
    }

    private void Update()
    {
        
        // Получаем ввод для перемещения игрока
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");


        if (movement.x != 0.0f && movement.y != 0.0f)
        {
            movement = movement.normalized;  // Нормализуем вектор движения, чтобы скорость была одинаковой по обеим осям
        }
        
        // shooting
        if (Input.GetMouseButtonDown(0))
        {
            ThrowOlivka();
        }
        
        
        //Debug.Log("x: " + movement.x+ " y: " + movement.y);
        if (movement.x == 0.0f && movement.y == 0.0f)
        {
            animator.SetBool("isIdle", true);
            animator.SetInteger("move", 0);
            return;
        }
        else
            animator.SetBool("isIdle", false);


        if(movement.x==1&& movement.y==0)
        {
            animator.SetInteger("move", 3);
            currentShotPosition = 2;
        }
        if (movement.x == 0 && movement.y == 1)
        {
            animator.SetInteger("move", 1);
            currentShotPosition = 0;
        }
        if (movement.x == -1 && movement.y == 0)
        {
            animator.SetInteger("move", 4);
            currentShotPosition = 1;
        }
        if (movement.x == 0 && movement.y == -1)
        {
            animator.SetInteger("move", 2);
            currentShotPosition = 0;
        }

        lookingAt = movement;
        

    }

    private void FixedUpdate()
    {
        // Перемещение игрока через Rigidbody2D
        rb.velocity = movement * moveSpeed;

        
        
    }

    public void AddBoost()
    {
        boostCount++;
        UpdateBoostCountText();  // Обновляем UI с количеством бустов

        if (boostCount == 3)
        {
            GenerateMaze.Instance.OpenExit();  // Открываем выход
        }
    }

    private void UpdateBoostCountText()
    {
        Debug.Log("Boost count обновляется: " + boostCount);
        if (boostCountText != null)
        {
            boostCountText.text = boostCount.ToString();  // Обновляем текст
            Debug.Log("Boost count text обновлен: " + boostCountText.text);
        }
        else
        {
            Debug.LogError("boostCountText не установлен в Inspector!");
        }
    }

    private void UpdateOlivkaCount()
    {
        olivkaCountText.text = olivkaCount.ToString();
    }

    private void ThrowOlivka()
    {
        if(olivkaCount <= 0)
            return;
        
        var newOlivka = Instantiate(olivka, shotPositions[currentShotPosition].position, Quaternion.identity);
        var olivkaRb = newOlivka.GetComponent<Rigidbody2D>();
        olivkaRb.AddForce(lookingAt * throwForce);

        olivkaCount--;
        UpdateOlivkaCount();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Логируем столкновения
        Debug.Log("OnTriggerEnter2D: столкновение с объектом " + other.gameObject.name);

        if (other.CompareTag("Exit"))
        {
            if (boostCount == 3)
            {
                Debug.Log("Вы успешно покинули лабиринт!");
                SceneManager.LoadSceneAsync("Win");  // Переход на сцену Win
            }
            else
            {
                Debug.Log("Сначала соберите все бусты!");
            }
        }

        if (other.CompareTag("Boost"))
        {
            AddBoost();
            Debug.Log("Буст собран");
        }
        
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        // losing
        if(other.gameObject.CompareTag("Buter"))
        {
            Debug.Log("Touching buter");
            SceneManager.LoadSceneAsync("Lose");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Логируем столкновения в реальном времени
        //Debug.Log("OnCollisionStay2D: Столкновение с объектом " + collision.gameObject.name);

        // Проверяем, если объект - стена
        if (collision.gameObject.CompareTag("Wall"))
        {
            // Если игрок продолжает сталкиваться со стеной, мы предотвращаем его движение
            //Debug.Log("Игрок продолжает столкновение со стеной.");
        }
    }

}

