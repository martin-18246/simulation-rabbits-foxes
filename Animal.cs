using UnityEngine;
using System.IO;
using System;

[System.Serializable]
public abstract class Animal : MonoBehaviour
{
    // type
    public enum AnimalType { Fox, Rabbit }
    public AnimalType animalType;

    // eating
    [SerializeField] protected float hunger = 20;
    [SerializeField] protected float baseHungerDecreaseRate = 0.2f;
    [SerializeField] protected float maximumStarvingHunger = 10;
    [SerializeField] protected float minimumHungerForReproduction = 25;
    [SerializeField] private float lastDecreasedHungerTime = 0f;


    // reproduction
    public Animal partner = null;
    public float potentialPartnerDistance = float.MaxValue;
    [SerializeField] public bool isFemale;
    [SerializeField] protected float lastReproductionTime = 0;
    [SerializeField] protected float? startedMatingTime = null;
    [SerializeField] protected float matingDuration = 3;
    [SerializeField] protected float pregnancyTime = 5;
    [SerializeField] protected float reproductionCooldown;
    [SerializeField] public Pregnancy pregnancy = null;

    // size
    [SerializeField] protected Vector3 originalScale;
    [SerializeField] protected float baseScale = 0.2f;
    public float massPercentage = 1f;

    // predation
    public bool predatorIsNeraby;


    
    // ageing
    [SerializeField] protected float currentAge = 0f;
    [SerializeField] protected float maximumAge = 1000f;
    [SerializeField] protected float maturityAge = 100f;

    // vision and range
    [SerializeField] protected float visionRange = 350f;
    [SerializeField] protected float roamingRange = 200f;

    // speed and agility
    [SerializeField] protected float movementSpeed;
    [SerializeField] protected float rotateSpeed = 100f;

    // goals and targets
    [SerializeField] protected Vector3? goalPosition = null;

    public IEdible targetFood = null;
    public bool hasFood; // for debugging
    // movement
    protected Vector3 movementVector;

    // distances and initial positions
    [SerializeField] protected float minimumInteractionDistance = 1f;

    // genes
    public HereditaryInformation genesInfo;

    public GameObject parameterSettings;


    // behavior state
    
    [SerializeField] private float? maxTimeSpentInThisState;

    // hunting
    protected Animal predator = null;
    public Animal prey = null;


    // behavior states and animation states
    public enum AnimalState
    {
        Idle,
        Roaming,

        SearchingFood,
        Hunting,
        Eating,

        EscapingPredator,

        SearchingPartner,
        GoingToMatingSpot,  
        WaitingForPartner,
        Mating,

        Dead
    }
    public AnimalState currentState;

    [SerializeField] private AnimationStateAnimal currentAnimationState;
    public AnimationStateAnimal GetAnimationStateAnimal() => currentAnimationState;


    // References to instances
    public GameObject entityManagerObject;
    protected EntityManager entityManager;
    public GameObject selfPrefab;
    public GameObject pointCenter;
    public GameObject pointEdge;
    private float distanceCenterEdge;

    // timers
    protected float lastChangedStateTime;
    [SerializeField] protected float maxTimeSpentInIdle = 2f;
    [SerializeField] protected float maxTimeSpentInRoaming = 2f;
    protected float startTime;
    protected float? eatingStartTime;


    
    public static float NormalDistribution(float meanFloat, float standardDeviation)
    {
        double meanDouble = (double)meanFloat;
        System.Random random = new System.Random();
        double u1 = random.NextDouble();
        double u2 = random.NextDouble();
        double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        float result = (float)(meanDouble + standardDeviation * z);
        if (result < 0) return -result;

        return result;
    }   


    private void Start()
    {
        startTime = Time.time;

        entityManager = entityManagerObject.GetComponent<EntityManager>();
        distanceCenterEdge = Vector3.Distance(pointCenter.transform.position, pointEdge.transform.position);

        originalScale = transform.localScale;

        // select random genes for animals at start with no parents
        if (genesInfo == null)
        {
            genesInfo = BaseTraitSettings.GetRandomGenes(NormalDistribution, animalType);
        }

        ApplyGenes();

        isFemale = ChooseRandomIsFemale();

        currentState = AnimalState.Roaming;

        int randomSkinChoice = UnityEngine.Random.Range(0, EntityManager.foxTextures.Count);
        if (this is Fox) transform.Find("fox4/fox1").GetComponent<Renderer>().material.mainTexture = EntityManager.foxTextures[randomSkinChoice];

    }



    private void Update()
    {
        hasFood = targetFood != null;
        if (currentState == AnimalState.Dead) { UpdateAnimationState(); return; } // do nothing

        HandleAgeing();
        HandleHunger();

        UpdateState(); // takes into account hunger, ability to reproduce, and changes changes currentState accordingly
        
        ActAccordingToState(); // take action based on the currentState that the animal is in

        if (isFemale && pregnancy != null) { ResolvePregnancy(); }

        
        UpdateAnimationState(); // set AnimationState depending on AnimalState

    }

    /// <summary>
    /// Sets the values of gained genes to actual characteristics fields. –
    /// </summary>
    protected void ApplyGenes()
    {
        pregnancyTime = genesInfo._pregnancyTime;
        reproductionCooldown = genesInfo._reproductionCooldown;
        maximumAge = genesInfo._maximumAge;
        maturityAge = genesInfo._maturityAge;
        visionRange = genesInfo._visionRange;
        roamingRange = genesInfo._roamingRange;
        movementSpeed = genesInfo._movementSpeed;
    }

    /// <summary>
    /// Assign sex at random. –
    /// </summary>
    public bool ChooseRandomIsFemale() => UnityEngine.Random.Range(0, 2) == 0;

    /// <summary>
    /// Increase age and die if it surpasses maximumAge. –
    /// </summary>
    public void HandleAgeing()
    {
        currentAge = Time.time - startTime;
        //ResolveSize();

        if (currentAge > maximumAge) Die();
    }


    /// <summary>
    /// Increase age and die if it surpasses maximumAge. –
    /// </summary>
    protected void HandleHunger()
    {
        if (Time.time - lastDecreasedHungerTime >= baseHungerDecreaseRate)
        {
            lastDecreasedHungerTime = Time.time;
            hunger -= 1;
            if (hunger <= 0) Die();
        }
    }

    /// <summary>
    /// Remove the animal from lists, destroy GameObject and ensure that other animals don't save references.
    /// </summary>
    protected void Die()
    {
        EndPartnership();
        entityManager.RemoveAnimalFromList(this);
        Destroy(gameObject);

    }

    /// <summary>
    /// Find out if a particular animal qualifies as a predator. –
    /// </summary>
    public bool PredatorRequirements(Animal potentialPredator)
    {
        float distanceToPredator = Vector3.Distance(transform.position, potentialPredator.transform.position);

        return
                potentialPredator.animalType == AnimalType.Fox &&
                potentialPredator != this &&
                distanceToPredator < visionRange;
                //potentialPredator.currentState == AnimalState.Hunting;
    }


    /// <summary>
    /// If there's a predator in your vision range, save its reference to "predator".
    /// </summary>
    protected void FindPredatorNearby()
    {
        Animal _predator = FindClosestAnimalSatisfyingRequirements(PredatorRequirements);
        predator = _predator;

    }

    protected abstract void ActOutSearchingForFood();
    protected abstract void ActOutEscapingPredator();
    protected abstract void ActOutHunting();

    /// <summary>
    /// If captured by predator, become edible.
    /// </summary>
    public void CapturedByPredator()
    {
        // kills the prey
        ChangeState(AnimalState.Dead, null);
        entityManager.RemoveAnimalFromList(this); // only remove from list
        if (this is Rabbit)
        {
            EntityManager.foodList.Add((IEdible)this);
        }

    }

    protected void UpdateState()
    {
        FindPredatorNearby(); // should be null if predator too far away
        bool isPredatorNearby = (predator != null && animalType != AnimalType.Fox); // 
        bool isStarving = hunger <= maximumStarvingHunger;
        bool isNormalHunger = hunger > maximumStarvingHunger && hunger < minimumHungerForReproduction;
        bool isInGoal = goalPosition != null ? Vector3.Distance(transform.position, goalPosition.Value) < minimumInteractionDistance : false;

        switch (currentState)
        {
            case AnimalState.Idle:
                if (isPredatorNearby) { ChangeState(AnimalState.EscapingPredator, null); return; }

                if (isStarving) { ChangeState(AnimalState.SearchingFood, null); return; }

                if (IsReadyForReproduction()) { ChangeState(AnimalState.SearchingPartner, null); return; }

                if (isNormalHunger) // not ready for reproduction because of hunger
                {
                    ChangeState(AnimalState.SearchingFood, null);
                    return;
                }
                else
                {
                    if (ShouldChangeStateOnTimer()) { ChangeState(AnimalState.Roaming, UnityEngine.Random.Range(maxTimeSpentInRoaming - 1f, maxTimeSpentInRoaming + 1f)); }
                }

                return;

            case AnimalState.Roaming:
                if (isPredatorNearby) { ChangeState(AnimalState.EscapingPredator, null); return; }

                if (isStarving) { ChangeState(AnimalState.SearchingFood, null); return; }

                if (IsReadyForReproduction()) { ChangeState(AnimalState.SearchingPartner, null); return; }

                if (isNormalHunger) // not ready for reproduction because of hunger
                {
                    ChangeState(AnimalState.SearchingFood, null);
                    return;
                }
                else // not ready for reproduction because of cooldown/pregnancy/immaturity
                {
                    if (ShouldChangeStateOnTimer()) { ChangeState(AnimalState.Idle, UnityEngine.Random.Range(maxTimeSpentInIdle - 1f, maxTimeSpentInIdle + 1f)); }
                }

                return;


            // states concerning nutrition
            case AnimalState.SearchingFood:
                if (this is Fox) { ChangeState(AnimalState.Hunting, null); }

                if (isPredatorNearby) { ChangeState(AnimalState.EscapingPredator, null); return; }

                if (isInGoal && targetFood != null) { ChangeState(AnimalState.Eating, null); return; }

                break;


            case AnimalState.Eating:
                if (isPredatorNearby) { ChangeState(AnimalState.EscapingPredator, null); return; }

                if (targetFood == null || targetFood.IsEaten)
                {
                    eatingStartTime = null;
                    ChangeState(AnimalState.Idle, 0);
                }
                break;


            // escaping
            case AnimalState.EscapingPredator:


                if (!isInGoal || predator != null) return;

                // is in goal and predator is null

                if (isStarving) { ChangeState(AnimalState.SearchingFood, null); return; }

                if (IsReadyForReproduction()) { ChangeState(AnimalState.SearchingPartner, null); return; }

                if (isNormalHunger) // not ready for reproduction because of hunger
                {
                    ChangeState(AnimalState.SearchingFood, null);
                    return;
                }
                else // not ready for reproduction because of cooldown/pregnancy/immaturity
                {
                    ChangeState(AnimalState.Idle, UnityEngine.Random.Range(maxTimeSpentInIdle - 1f, maxTimeSpentInIdle + 1f));
                }

                break;


            // states concerning reproduction
            case AnimalState.SearchingPartner:
                if (isPredatorNearby) { ChangeState(AnimalState.EscapingPredator, null); return; }

                if (isStarving) { ChangeState(AnimalState.SearchingFood, null); return; }


                if (partner != null)
                {
                    ChangeState(AnimalState.GoingToMatingSpot, null);
                }
                break;

            case AnimalState.GoingToMatingSpot:
                if (isPredatorNearby) { ChangeState(AnimalState.EscapingPredator, null); return; }

                if (isStarving) { ChangeState(AnimalState.SearchingFood, null); return; }


                if (!IsPartnerGoingThroughWithIt()) { EndPartnership(); ChangeState(AnimalState.Roaming, 3); }

                if (isInGoal)
                {
                    ChangeState(AnimalState.WaitingForPartner, null);
                }
                break;

            case AnimalState.WaitingForPartner:
                if (isPredatorNearby) { ChangeState(AnimalState.EscapingPredator, null); return; }

                if (isStarving) { ChangeState(AnimalState.SearchingFood, null); return; }

                if (!IsPartnerGoingThroughWithIt()) { EndPartnership(); ChangeState(AnimalState.Roaming, 3); return; }

                if (partner.currentState == AnimalState.WaitingForPartner)
                {
                    if (isFemale)
                    {
                        ChangeState(AnimalState.Mating, null);
                        partner.StartMating();
                    }

                }

                break;

            case AnimalState.Mating:
                if (isPredatorNearby) { ChangeState(AnimalState.EscapingPredator, null); return; }

                if (isStarving) { ChangeState(AnimalState.SearchingFood, null); return; }

                if (!IsPartnerGoingThroughWithIt()) { EndPartnership(); ChangeState(AnimalState.Roaming, 3); }


                if (pregnancy != null || !IsPartnerGoingThroughWithIt())
                {
                    ChangeState(AnimalState.Idle, 1);
                }

                break;
        }


    }

    private void ActOutRoaming()
    {
        if (goalPosition == null)
        {
            goalPosition = FindGoalPosition();
        }

        if (IsInGoal())
        {
            goalPosition = FindGoalPosition();
        }

        MoveTowardsGoal();
    }

    private void ActOutEating()
    {
        if (eatingStartTime == null)
        {
            eatingStartTime = Time.time;
        }

        else
        {
            float timeSinceLastBite = Time.time - eatingStartTime.Value;
            eatingStartTime = Time.time;
            hunger += (timeSinceLastBite) * targetFood.GetNutritionPerSecond;
            if (targetFood != null) targetFood.TimeEatenSinceLastBite(timeSinceLastBite);
        }
    }

    private void ActOutSearchingForPartner()
    {
        Animal suitablePartner = FindClosestAnimalSatisfyingRequirements(PartnerRequirements);


        if (suitablePartner != null)
        {
            PartnerProposalResponse proposalResponse = SendPartnerProposal(suitablePartner);

            if (proposalResponse._proposalAccepted)
            {
                partner = suitablePartner;
                partner.partner = this;

                goalPosition = proposalResponse._meetingSpot;
                return;
            }
        }

        if (goalPosition == null || IsInGoal())
        {
            goalPosition = FindGoalPosition(); // Modify the FindGoal method so that it takes into account that SearchingPartner state called it
        }

        MoveTowardsGoal();
    }

    private void ActOutGoingToMatingSpot()
    {
        if (!IsPartnerGoingThroughWithIt())
        {
            EndPartnership();
            return;
        }

        if (goalPosition != null)
        {
            MoveTowardsGoal();
        }
        else
        {
            EndPartnership();
            return;
        }
    }

    private void ActOutMating()
    {
        if (!IsPartnerGoingThroughWithIt())
        {
            EndPartnership();
            return;
        }

        switch (isFemale)
        {
            case true:
                if (Time.time - startedMatingTime >= matingDuration)
                {
                    BecomePregnantWith(partner);
                }
                return;

            case false:
                return;

        }
    }
    protected void ActAccordingToState()
    {
        switch (currentState)
        {
            case AnimalState.Idle:
                break;

            case AnimalState.Roaming:
                ActOutRoaming();
                break;


            case AnimalState.SearchingFood:
                ActOutSearchingForFood();
                break;

            case AnimalState.Hunting:
                ActOutHunting();
                break;

            case AnimalState.Eating:
                ActOutEating();
                break;

            case AnimalState.EscapingPredator:
                ActOutEscapingPredator();
                break;



            case AnimalState.SearchingPartner:
                ActOutSearchingForPartner();
                break;

            case AnimalState.GoingToMatingSpot:
                ActOutGoingToMatingSpot();
                break;

            case AnimalState.WaitingForPartner:
                if (!IsPartnerGoingThroughWithIt())
                {
                    EndPartnership();
                }
                break;

            case AnimalState.Mating:
                ActOutMating();
                break;




        }
    }

    protected void ChangeState(AnimalState newState, float? maxTimeInState)
    {
        if (currentState == newState) return;

        currentState = newState;

        switch (currentState)
        {
            case AnimalState.Idle:
                movementVector = Vector3.zero;
                EndPartnership();
                goalPosition = null;
                break;

            case AnimalState.Roaming:
                movementVector = Vector3.zero;
                EndPartnership();
                goalPosition = null;
                break;

            case AnimalState.SearchingFood:
                EndPartnership();
                goalPosition = null;
                break;

            case AnimalState.GoingToMatingSpot:
                break;

            case AnimalState.Eating:
                EndPartnership();
                goalPosition = null;
                break;

            case AnimalState.Mating:
                startedMatingTime = Time.time;
                break;

            case AnimalState.EscapingPredator:
                targetFood = null;
                EndPartnership();
                break;

            case AnimalState.Dead:
                pregnancy = null;
                goalPosition = null;
                predator = null;
                prey = null;
                break;
                

        }




        lastChangedStateTime = Time.time;
        maxTimeSpentInThisState = maxTimeInState;
    }

    /// <summary>
    /// If you've been in this state too long, change it.
    /// </summary>
    protected bool ShouldChangeStateOnTimer() => (maxTimeSpentInThisState != null && Time.time - lastChangedStateTime > maxTimeSpentInThisState);

    protected IEdible FindClosestFoodInRange()
    {
        if (EntityManager.foodList == null) return null;

        float closestDistanceToFood = float.MaxValue;
        IEdible closestFood = null;

        foreach (var food in EntityManager.foodList)
        {
            if (food is null) continue; // there shouldn't be null there, fix this

            float distanceToFood = Vector3.Distance(transform.position, food.GetTransform.position);

            if (distanceToFood < closestDistanceToFood && distanceToFood < visionRange)
            {
                closestDistanceToFood = distanceToFood;
                closestFood = food;
            }
        }

        return closestFood;

    }


    public delegate bool RequirementCheck(Animal potentialAnimal);

    /// <summary>
    /// Check if an animal can qualify as a partner.
    /// </summary>
    public bool PartnerRequirements(Animal potentialPartner)
    {
        float distanceToPartner = Vector3.Distance(transform.position, potentialPartner.transform.position);

        return
                potentialPartner != this &&
                distanceToPartner < visionRange &&
                distanceToPartner < potentialPartnerDistance &&
                isFemale != potentialPartner.isFemale &&
                (GetType() == potentialPartner.GetType()) &&
                potentialPartner.currentState == AnimalState.SearchingPartner;
    }

    protected Animal FindClosestAnimalSatisfyingRequirements(Predicate<Animal> MeetsRequirements)
    {

        if (EntityManager.animalList == null) return null;

        Animal closestSuitableTarget = null;

        foreach (var animal in EntityManager.animalList)
        {
            if (animal == null) continue;
            float distanceToTarget = Vector3.Distance(transform.position, animal.transform.position);


            bool meetsRequirements = MeetsRequirements(animal);
            if (meetsRequirements)
            {
                potentialPartnerDistance = distanceToTarget;
                closestSuitableTarget = animal;
            }
        }

        potentialPartnerDistance = float.MaxValue;
        return closestSuitableTarget;
    }

    protected class PartnerProposalResponse
    {
        public bool _proposalAccepted;
        public Vector3? _meetingSpot;

        public PartnerProposalResponse(bool proposalAccepted, Vector3? meetingSpot)
        {
            _proposalAccepted = proposalAccepted;
            _meetingSpot = meetingSpot;
        }
    }

    protected PartnerProposalResponse SendPartnerProposal(Animal potentialPartner)
    {
        PartnerProposalResponse answer = potentialPartner.ManagePartnerProposal(this);
        return answer;
    }

    protected PartnerProposalResponse ManagePartnerProposal(Animal proposalSender)
    {
        if (currentState == AnimalState.SearchingPartner)
        {
            partner = proposalSender;
            ChangeState(AnimalState.GoingToMatingSpot, null);
            Vector3 meetingSpot = SelectMeetingSpot(this, proposalSender);
            PartnerProposalResponse proposalResponse = new PartnerProposalResponse(true, meetingSpot);
            goalPosition = meetingSpot;
            return proposalResponse;
            
        }

        return new PartnerProposalResponse(false, null);
    }

    /// <summary>
    /// Set mutual goal for both partners.
    /// </summary>
    protected Vector3 SelectMeetingSpot(Animal animalA, Animal animalB)
    {
        return (animalA.transform.position + animalB.transform.position) / 2;
    }


    protected bool IsReadyForReproduction()
    {
        return
            hunger >= minimumHungerForReproduction &&
            currentAge >= maturityAge &&
            pregnancy == null &&
            Time.time - lastReproductionTime >= reproductionCooldown;
    }

    /// <summary>
    /// Manage fields so that former partners no longer point at each other, happens when one partner starts doing something else (escaping predator).
    /// </summary>
    protected void EndPartnership()
    {
        if (partner != null) partner.partner = null;

        partner = null;
        goalPosition = null;
    }

    /// <summary>
    /// Check if your partner still has YOU as a partner.
    /// </summary>
    protected bool IsPartnerGoingThroughWithIt() =>
            partner != null &&
            partner.gameObject &&
            partner.partner != null &&
            partner.partner == this &&
            (partner.currentState == AnimalState.SearchingPartner || partner.currentState == AnimalState.GoingToMatingSpot || partner.currentState == AnimalState.WaitingForPartner || partner.currentState == AnimalState.Mating)
            ;

    /// <summary>
    /// Find your next destination according to your state.
    /// </summary>
    protected Vector3? FindGoalPosition()
    {
        Vector3 suggestedGoal;

        switch (currentState)
        {
            case AnimalState.SearchingFood:
                IEdible closestFood = FindClosestFoodInRange();
                if (closestFood == null) break;
                suggestedGoal = closestFood.GetTransform.position;
                return suggestedGoal;

            case AnimalState.EscapingPredator:
                if (predator == null) break;

                Vector3 directionAwayFromPredatorNormalized = (transform.position - predator.transform.position).normalized; // ok


                Vector3 orthogonalNormalized = Vector3.Cross(directionAwayFromPredatorNormalized, Vector3.up).normalized;
                float randomModifier = UnityEngine.Random.Range(-0.2f, 0.2f);
                Vector3 varianceEscapeVector = randomModifier * orthogonalNormalized;

                Vector3 escapeMovementVector = (directionAwayFromPredatorNormalized + varianceEscapeVector).normalized;

                suggestedGoal = transform.position + roamingRange * escapeMovementVector;

                if (Vector3.Distance(suggestedGoal, pointCenter.transform.position) > distanceCenterEdge) break;

                return suggestedGoal;

                

        }

        for (int attemptNum = 0; attemptNum < 10; attemptNum++)
        {
            suggestedGoal = Utilities.GenerateRandomLocationInRangeSquare(transform.position, roamingRange);
            if (Vector3.Distance(suggestedGoal, pointCenter.transform.position) > distanceCenterEdge) continue;

            return suggestedGoal;

        }

        Die();
        return null;

    }

    /// <summary>
    /// Move one step closer to your destination.
    /// </summary>
    protected void MoveTowardsGoal()
    {
        if (goalPosition == null) { return; } 

        movementVector = goalPosition.Value - transform.position;
        Vector3 movementVector3Normalized = movementVector.normalized.Flatten();

        if (movementVector3Normalized != Vector3.zero)
        {
            transform.forward = Vector3.Slerp(transform.forward, movementVector3Normalized, Time.deltaTime * rotateSpeed);

        }
        transform.position += movementVector3Normalized * Time.deltaTime * movementSpeed;
    }

    public void StartMating()
    {
        if (currentState == AnimalState.WaitingForPartner || currentState == AnimalState.GoingToMatingSpot) ChangeState(AnimalState.Mating, null);
    }

    protected void ResolveSize()
    {
        switch (currentAge > maturityAge)
        {
            case true:
                transform.localScale = Vector3.one; // not optimal, set a switch for maturity so you don't have to do this every time
                break;

            case false:
                transform.localScale = new Vector3(baseScale, baseScale, baseScale) + new Vector3(1 - baseScale, 1 - baseScale, 1 - baseScale) * (currentAge / maturityAge);
                break;
        }

        
    }

    protected bool IsInGoal() => goalPosition != null && Vector3.Distance(transform.position, goalPosition.Value) < minimumInteractionDistance;

    protected void BecomePregnantWith(Animal partner)
    {
        pregnancy = new Pregnancy(selfPrefab, this, partner, entityManager, 2);

        if (partner != null)
        {
            partner.lastReproductionTime = Time.time;
            EndPartnership();
        }

        ChangeState(AnimalState.Roaming, 3);
        EndPartnership();
    }

    protected void ResolvePregnancy()
    {
        

        if (Time.time - pregnancy._pregnancyStartTime >= pregnancyTime)
        {
            pregnancy.GiveBirth();
        }
    }

    protected void UpdateAnimationState()
    {
        switch (currentState)
        {
            case AnimalState.Idle:
            case AnimalState.Eating:
            case AnimalState.WaitingForPartner:
            case AnimalState.Mating:
            case AnimalState.Dead:
                currentAnimationState = AnimationStateAnimal.Idling;
                break;

            case AnimalState.Roaming:
            case AnimalState.SearchingFood:
            case AnimalState.EscapingPredator:
            case AnimalState.SearchingPartner:
            case AnimalState.GoingToMatingSpot:
                currentAnimationState = AnimationStateAnimal.Jumping;
                break;
        }
    }

    public class Pregnancy
    {
        public float _pregnancyStartTime;

        protected int _litterSize;
        protected UnityEngine.GameObject _selfPrefab;
        protected Animal _parentMother;
        protected Animal _parentFather;
        private EntityManager _entityManager;

        private HereditaryInformation _genesMother;
        private HereditaryInformation _genesFather;
        private HereditaryInformation _genesChild;

        delegate float CalculateGenes(float geneMother, float geneFather);

        public void GiveBirth()
        {
            for (int i = 0; i < _litterSize; i++)
            {
                _entityManager.SpawnBirthedAnimal(_selfPrefab, _parentMother.transform.position, _genesChild);
            }

            _parentMother.lastReproductionTime = Time.time;
            _parentMother.pregnancy = null;
            
        }

        public Pregnancy(UnityEngine.GameObject selfPrefab, Animal parentMother, Animal parentFather, EntityManager entityManager, int litterSize)
        {
            _pregnancyStartTime = Time.time;
            _selfPrefab = selfPrefab;
            _parentMother = parentMother;
            _parentFather = parentFather;
            _entityManager = entityManager;
            _litterSize = litterSize;



            _genesMother = _parentMother.genesInfo;
            _genesFather = _parentFather.genesInfo;

            //_genesChild = CombineGenes(_genesMother, _genesFather, (float a, float b) => (a + b) / 2);
            if (parentMother is Rabbit) _genesChild = CombineGenes(_genesMother, _genesFather, NormalDistribution, AnimalType.Rabbit);
            else _genesChild = CombineGenes(_genesMother, _genesFather, NormalDistribution, AnimalType.Fox);

        }

        private HereditaryInformation CombineGenes(HereditaryInformation genesMother, HereditaryInformation genesFather, CalculateGenes CalculateGeneFromBaseAndDiff, AnimalType animType) // change argument names for CalculateGenes
        {
            if (animType == AnimalType.Rabbit)
            {

            
                float pregnancyTime = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._pregnancyTime, genesFather._pregnancyTime), BaseTraitSettings.pregnancyTimeBaseRabbit);
                float reproductionCooldown = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._reproductionCooldown, genesFather._reproductionCooldown), BaseTraitSettings.reproductionCooldownBaseRabbit);
                float maximumAge = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._maximumAge, genesFather._maximumAge), BaseTraitSettings.maximumAgeBaseRabbit);
                float maturityAge = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._maturityAge, genesFather._maturityAge), BaseTraitSettings.maturityAgeBaseRabbit);
                float visionRange = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._visionRange, genesFather._visionRange), BaseTraitSettings.visionRangeBaseRabbit);
                float roamingRange = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._roamingRange, genesFather._roamingRange), BaseTraitSettings.roamingRangeBaseRabbit);
                float movementSpeed = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._movementSpeed, genesFather._movementSpeed), BaseTraitSettings.movementSpeedBaseRabbit);

                return new HereditaryInformation(pregnancyTime, reproductionCooldown, maximumAge, maturityAge, visionRange, roamingRange, movementSpeed);
            } else
            {
                float pregnancyTime = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._pregnancyTime, genesFather._pregnancyTime), BaseTraitSettings.pregnancyTimeBaseFox);
                float reproductionCooldown = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._reproductionCooldown, genesFather._reproductionCooldown), BaseTraitSettings.reproductionCooldownBaseFox);
                float maximumAge = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._maximumAge, genesFather._maximumAge), BaseTraitSettings.maximumAgeBaseFox);
                float maturityAge = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._maturityAge, genesFather._maturityAge), BaseTraitSettings.maturityAgeBaseFox);
                float visionRange = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._visionRange, genesFather._visionRange), BaseTraitSettings.visionRangeBaseFox);
                float roamingRange = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._roamingRange, genesFather._roamingRange), BaseTraitSettings.roamingRangeBaseFox);
                float movementSpeed = CalculateGeneFromBaseAndDiff(GetAverageTraitValue(genesMother._movementSpeed, genesFather._movementSpeed), BaseTraitSettings.movementSpeedBaseFox);
                return new HereditaryInformation(pregnancyTime, reproductionCooldown, maximumAge, maturityAge, visionRange, roamingRange, movementSpeed);
            }
        }

        private float GetAverageTraitValue(float value1, float value2) => (value1 + value2) / 2f;


        
    }

    public float GetTrait(TraitType trait)
    {
        switch (trait)
        {
            case TraitType.movementSpeed:
                return movementSpeed;

            case TraitType.visionRange:
                return visionRange;
        }

        return 0;
    } 

    public enum TraitType
    {
        movementSpeed,
        visionRange
    }
}









