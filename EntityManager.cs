using UnityEngine;
using System.Collections.Generic;
using TMPro.Examples;

public class EntityManager : MonoBehaviour
{
    // Start is called before the first frame update

    // food prefabs
    public GameObject foodPrefab;
    public GameObject mushroom1Prefab;
    public GameObject applePrefab;
    public GameObject pearPrefab;

    // environment constraints/boundaries
    public GameObject pointCenter;
    public GameObject pointRadius;
    public float distanceCenterRadius;

    // animal prefabs
    public GameObject rabbitPrefab;
    public GameObject foxPrefab;

    // fox skins
    public Texture2D foxSkin1;
    public Texture2D foxSkin2;
    public Texture2D foxPink;
    public Texture2D foxPolar;
    public Texture2D foxFaded;

    // environment prefabs
    public GameObject treePrefab;
    public GameObject stone1;
    public GameObject stone2;


    // timers
    private double startTime;
    private double lastSpawnedFood;
    [SerializeField] private double foodSpawnFrequency = 3f;

    // entity lists
    public static List<IEdible> foodList;
    public static List<Animal> animalList;
    public static List<GameObject> prefabList;
    public static List<Texture2D> foxTextures;

    // initial numbers
    [SerializeField] private int rabbitsAtStart = 20;
    [SerializeField] private int foxesAtStart = 2;
    [SerializeField] private int foodAtStart = 10;

    // initial environment
    [SerializeField] private int treesStart = 20;
    [SerializeField] private int stonesStart = 20;

    public int activeFood = 0;
    public int activeRabbits = 0;
    public int activeFoxes = 0;



    void Awake()
    {
        startTime = Time.time;
        lastSpawnedFood = startTime;

        distanceCenterRadius = Vector3.Distance(pointCenter.transform.position, pointRadius.transform.position);

        // list initialization
        foodList = new List<IEdible>();
        animalList = new List<Animal>();
        prefabList = new List<GameObject>() { applePrefab, mushroom1Prefab, pearPrefab };
        foxTextures = new List<Texture2D>() { foxSkin1, foxSkin2, foxPolar, foxFaded, foxPink };

    }

    public void Start()
    {

        SpawnStartAnimals();
        SpawnStartEnvironment();
        for (int i = 0; i < foodAtStart; i++)
        {
            SpawnRandomFood();
        }

    }


    void Update()
    {
        if (Time.time - lastSpawnedFood >= foodSpawnFrequency) SpawnRandomFood();
    }

    /// <summary>
    /// Spawn random food at a random location. –
    /// </summary>
    private void SpawnRandomFood()
    {
        lastSpawnedFood = Time.time;

        Vector3 randomVector = Utilities.GenerateRandomLocationInCircle(transform.position, distanceCenterRadius);

        int randomFoodToss = UnityEngine.Random.Range(0, prefabList.Count);

        GameObject objectToSpawn = prefabList[randomFoodToss];
        GameObject newFoodObject = Instantiate(objectToSpawn, new Vector3(randomVector.x, 0, randomVector.z), Quaternion.identity) as GameObject;

        Food foodScript = newFoodObject.GetComponent<Food>();
        foodList.Add(foodScript);
        activeFood += 1;

    }

    private void SpawnStartEnvironment()
    {
        for (int i = 0; i < treesStart; i++)
        {
            GameObject newTree = Instantiate(treePrefab, Utilities.GenerateRandomLocationInRangeSquare(Vector3.zero, distanceCenterRadius * 0.7f), Quaternion.identity);
            newTree.transform.localScale = UnityEngine.Random.Range(1f, 3f) * Vector3.one;
        }

        for (int i = 0; i < stonesStart; i++)
        {
            GameObject newStone = Instantiate(stone1, Utilities.GenerateRandomLocationInRangeSquare(Vector3.zero, distanceCenterRadius * 0.7f), Quaternion.identity);
            newStone.transform.localScale = UnityEngine.Random.Range(1f, 3f) * Vector3.one;

        }
    }


    /// <summary>
    /// Spawn rabbits and foxes at the start of the simulation in numbers set in Inspector. –
    /// </summary>
    private void SpawnStartAnimals()
    {
        for (int i = 0; i < foxesAtStart; i++)
        {
            SpawnRandomAnimal(foxPrefab);
            activeFoxes += 1;
        }

        for (int i = 0; i < rabbitsAtStart; i++)
        {
            SpawnRandomAnimal(rabbitPrefab);
            activeRabbits += 1;

        }
    }

    /// <summary>
    /// Spawn the animal in argument at a random location. Used at the start of the simulation. –
    /// </summary>
    public void SpawnRandomAnimal(GameObject animal)
    {
        Vector3 randomPosition = Utilities.GenerateRandomLocationInCircle(transform.position, distanceCenterRadius);

        GameObject newAnimal = Instantiate(animal, randomPosition, Quaternion.identity);
        Animal animalBehavior = newAnimal.GetComponent<Animal>();
        animalList.Add(animalBehavior);
        if (animalBehavior is Fox) activeFoxes += 1;
        if (animalBehavior is Rabbit) activeRabbits += 1;


    }

    /// <summary>
    /// Instantiate a new animal, with traits inherited from parents, at the location of the mother.
    /// </summary>
    public void SpawnBirthedAnimal(GameObject animal, Vector3 spawnPosition, HereditaryInformation genesInformation)
    {
        GameObject newAnimal = Instantiate(animal, spawnPosition, Quaternion.identity);
        Animal animalBehavior = newAnimal.GetComponent<Animal>();
        animalList.Add(animalBehavior);
        animalBehavior.genesInfo = genesInformation;
        if (animalBehavior is Fox) activeFoxes += 1;
        if (animalBehavior is Rabbit) activeRabbits += 1;
    }


    public void RemoveFoodFromList(IEdible foodToDestroy)
    {
        if (foodList.Remove(foodToDestroy)) activeFood -= 1;
    }

    public void RemoveAnimalFromList(Animal animal)
    {
        animalList.Remove(animal);
    } 



}
