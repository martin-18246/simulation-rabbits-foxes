using UnityEngine;


public class Rabbit : Animal, IEdible
{
    FoodType foodType = FoodType.Meat;

    [SerializeField] protected float nutritionPerSecond = 3f;
    [SerializeField] protected float TimeCanBeEaten = 5f;
    [SerializeField] protected float HasBeenEatenFor = 0f;
    [SerializeField] protected bool eaten = false;


    public Transform GetTransform { get { return selfPrefab == null? null : selfPrefab.transform; } }
    public FoodType GetFoodType { get { return foodType; } }
    public float GetNutritionPerSecond { get { return nutritionPerSecond; } }
    public float GetTimeCanBeEaten { get { return TimeCanBeEaten; } }
    public float GetTimeHasBeenEatenTotal { get { return HasBeenEatenFor; } }
    public bool IsEaten { get { return eaten; } }

    GameObject IEdible.GetGameObject() { return gameObject; }

    void IEdible.TimeEatenSinceLastBite(float time)
    {
        HasBeenEatenFor += time;

        ((IEdible)this).ChangeScaleFood();
    }

    public void Awake()
    {
        animalType = AnimalType.Rabbit;

    }


    void IEdible.ChangeScaleFood()
    {
        massPercentage = 1 - (HasBeenEatenFor / TimeCanBeEaten);

        if (gameObject == null) return;

        if (massPercentage <= 0)
        {
            entityManager.RemoveFoodFromList(this);
            Destroy(gameObject);
            eaten = true;
            return;
        }

        transform.localScale = originalScale * massPercentage;
    }


    protected override void ActOutSearchingForFood()
    {
        if (targetFood == null)
        {
            IEdible closestFood = FindClosestFoodInRange();

            if (closestFood == null)
            {
                if (goalPosition == null)
                {
                    goalPosition = FindGoalPosition();
                } else
                {
                    MoveTowardsGoal();
                    return;
                }

            }
            else
            {
                targetFood = (IEdible)closestFood;

                goalPosition = targetFood.GetTransform.position;
            }
        }
        else
        {
            if (!targetFood.IsEaten)
            {
                goalPosition = targetFood.GetTransform.position;
            }
            else
            {
                IEdible closestFood = FindClosestFoodInRange();

                if (closestFood == null)
                {
                    if (goalPosition == null)
                    {
                        goalPosition = FindGoalPosition();
                    }

                }
                else
                {
                    targetFood = (IEdible)closestFood;
                    goalPosition = targetFood.GetTransform.position;
                }
            }

        }

        MoveTowardsGoal();
    }

    protected override void ActOutHunting() { } // will not be called for rabbits

    protected override void ActOutEscapingPredator()
    {
        if (goalPosition == null && predator == null) goalPosition = transform.position;
        if (goalPosition == null) goalPosition = FindGoalPosition();
        if (IsInGoal()) goalPosition = FindGoalPosition();

        MoveTowardsGoal();
    }
}