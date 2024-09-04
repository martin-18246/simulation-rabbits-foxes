using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class Food : MonoBehaviour, IEdible
{
    public EntityManager entityManager;
    public GameObject groundObject;

    public float nutritionPerSecond = 500;

    public float massPercentage = 1f;

    public float TimeCanBeEaten = 5;
    public float HasBeenEatenFor = 0;
    public Vector3 originalScale;

    public bool eaten = false;

    public Transform GetTransform { get { return  transform; } }
    public FoodType GetFoodType { get { return FoodType.Vegetation; } }
    public float GetNutritionPerSecond { get { return nutritionPerSecond; } }
    public float GetTimeCanBeEaten { get { return TimeCanBeEaten; } }
    public float GetTimeHasBeenEatenTotal { get { return HasBeenEatenFor; } }
    public bool IsEaten { get { return eaten; } }

    GameObject IEdible.GetGameObject() { return gameObject; }


    private void Start()
    {
        entityManager = groundObject.GetComponent<EntityManager>();
        originalScale = transform.localScale;
    }


    public void TimeEatenSinceLastBite(float time)
    {
        HasBeenEatenFor += time;
        ChangeScaleFood();
    }

    public void ChangeScaleFood()
    {
        massPercentage = 1 - (HasBeenEatenFor / TimeCanBeEaten);

        if (this == null || !gameObject || gameObject == null) return;

        if (massPercentage <= 0)
        {
            entityManager.RemoveFoodFromList(this);
            Destroy(gameObject);
            eaten = true;
            return;
        }

        transform.localScale = originalScale * massPercentage;
    }


}


public interface IEdible
{
    public Transform GetTransform { get; }
    public FoodType GetFoodType { get; }
    public float GetNutritionPerSecond { get; }
    public float GetTimeCanBeEaten { get; }
    public float GetTimeHasBeenEatenTotal { get; }
    public void TimeEatenSinceLastBite(float time);
    public void ChangeScaleFood();

    public bool IsEaten { get; }

    public GameObject GetGameObject();

}

public enum FoodType
{
    Vegetation,
    Meat
}