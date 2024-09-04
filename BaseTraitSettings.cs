using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Set default values for various animal traits. From these, random genome will be calculated. –
/// </summary>
/// 

public static class BaseTraitSettings
{
    // Rabbit

    public static float pregnancyTimeBaseRabbit = 10;
    public static float pregnancyTimeDifRabbit = 2;

    public static float reproductionCooldownBaseRabbit = 20;
    public static float reproductionCooldownDifRabbit = 5;

    public static float maximumAgeBaseRabbit = 180;
    public static float maximumAgeDifRabbit = 20;

    public static float maturityAgeBaseRabbit = 30;
    public static float maturityAgeDifRabbit = 5;

    public static float visionRangeBaseRabbit = 50;
    public static float visionRangeDifRabbit = 10;

    public static float roamingRangeBaseRabbit = 90;
    public static float roamingRangeDifRabbit = 20;

    public static float movementSpeedBaseRabbit = 15;
    public static float movementSpeedDifRabbit = 2;


    // Fox
    public static float pregnancyTimeBaseFox = 15;
    public static float pregnancyTimeDifFox = 2;

    public static float reproductionCooldownBaseFox = 50;
    public static float reproductionCooldownDifFox = 10;

    public static float maximumAgeBaseFox = 180;
    public static float maximumAgeDifFox = 15;

    public static float maturityAgeBaseFox = 40;
    public static float maturityAgeDifFox = 5;

    public static float visionRangeBaseFox = 50;
    public static float visionRangeDifFox = 10;

    public static float roamingRangeBaseFox = 70;
    public static float roamingRangeDifFox = 10;

    public static float movementSpeedBaseFox = 18;
    public static float movementSpeedDifFox = 2;


    /// <summary>
    /// Determines how a random gene will be calculated from the base value and difference value. –
    /// </summary>
    public delegate float RandomTraitFunc(float baseValue, float dif);

    /// <summary>
    /// Generate a random genome for an animal at the start of the simulation. –
    /// </summary>
    public static HereditaryInformation GetRandomGenes(RandomTraitFunc CalculateRandomGene, Animal.AnimalType animalType)
    {
        float _pregnancyTime;
        float _reproductionCooldown;
        float _maximumAge;
        float _maturityAge;
        float _visionRange;
        float _roamingRange;
        float _movementSpeed;

        HereditaryInformation NewGenes;

        switch (animalType)
        {
            case Animal.AnimalType.Rabbit:
                _pregnancyTime = CalculateRandomGene(pregnancyTimeBaseRabbit, pregnancyTimeDifRabbit);
                _reproductionCooldown = CalculateRandomGene(reproductionCooldownBaseRabbit, reproductionCooldownDifRabbit);
                _maximumAge = CalculateRandomGene(maximumAgeBaseRabbit, maximumAgeDifRabbit);
                _maturityAge = CalculateRandomGene(maturityAgeBaseRabbit, maturityAgeDifRabbit);
                _visionRange = CalculateRandomGene(visionRangeBaseRabbit, visionRangeDifRabbit);
                _roamingRange = CalculateRandomGene(roamingRangeBaseRabbit, roamingRangeDifRabbit);
                _movementSpeed = CalculateRandomGene(movementSpeedBaseRabbit, movementSpeedDifRabbit);

                NewGenes = new HereditaryInformation(_pregnancyTime, _reproductionCooldown, _maximumAge, _maturityAge, _visionRange, _roamingRange, _movementSpeed);
                break;

            default:
                _pregnancyTime = CalculateRandomGene(pregnancyTimeBaseFox, pregnancyTimeDifFox);
                _reproductionCooldown = CalculateRandomGene(reproductionCooldownBaseFox, reproductionCooldownDifFox);
                _maximumAge = CalculateRandomGene(maximumAgeBaseFox, maximumAgeDifFox);
                _maturityAge = CalculateRandomGene(maturityAgeBaseFox, maturityAgeDifFox);
                _visionRange = CalculateRandomGene(visionRangeBaseFox, visionRangeDifFox);
                _roamingRange = CalculateRandomGene(roamingRangeBaseFox, roamingRangeDifFox);
                _movementSpeed = CalculateRandomGene(movementSpeedBaseFox, movementSpeedDifFox);
                NewGenes = new HereditaryInformation(_pregnancyTime, _reproductionCooldown, _maximumAge, _maturityAge, _visionRange, _roamingRange * 1.3f, _movementSpeed * 1.3f);
                break;
        }

        return NewGenes;
    }


}


public class HereditaryInformation
{
    public float _pregnancyTime;
    public float _reproductionCooldown;
    public float _maximumAge;
    public float _maturityAge;
    public float _visionRange;
    public float _roamingRange;
    public float _movementSpeed;

    public HereditaryInformation(float pregnancyTime, float reproductionCooldown, float maximumAge, float maturityAge, float visionRange, float roamingRange, float movementSpeed)
    {
        _pregnancyTime = pregnancyTime;
        _reproductionCooldown = reproductionCooldown;
        _maximumAge = maximumAge;
        _maturityAge = maturityAge;
        _visionRange = visionRange;
        _roamingRange = roamingRange;
        _movementSpeed = movementSpeed;
    }
}