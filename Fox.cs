using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Fox : Animal
{
    public void Awake() // gets called whenever instantiated, before first frame
    {
        animalType = AnimalType.Fox;
    }

    protected override void ActOutSearchingForFood() { } // will not happen, will go straigth to hunting
    protected override void ActOutEscapingPredator() { } // will not happen to predator


    /// <summary>
    /// Checks if an animal can be considered as prey to the caller.
    /// </summary>
    public bool PreyRequirements(Animal potentialPrey)
    {
        float distanceToPrey = Vector3.Distance(transform.position, potentialPrey.transform.position);

        return
                potentialPrey.animalType == AnimalType.Rabbit &&
                potentialPrey.currentState != AnimalState.Dead &&
                potentialPrey != this &&
                distanceToPrey < visionRange;
    }


    protected override void ActOutHunting()
    {


        if (prey == null || prey.currentState == AnimalState.Dead || Vector3.Distance(prey.transform.position, transform.position) > visionRange)
        {
            prey = FindClosestAnimalSatisfyingRequirements(PreyRequirements);

            if (prey == null && goalPosition == null) goalPosition = FindGoalPosition();


        } else
        {
            goalPosition = prey.transform.position;
        }

        // assumes that prey is not null
        
        MoveTowardsGoal();

        if (IsInGoal() && prey != null)
        {
            prey.CapturedByPredator();
            targetFood = (IEdible)prey;
            ChangeState(AnimalState.Eating, null);
            return;
        }

        if (IsInGoal())
        {
            goalPosition = FindGoalPosition();
        }
    }

}
