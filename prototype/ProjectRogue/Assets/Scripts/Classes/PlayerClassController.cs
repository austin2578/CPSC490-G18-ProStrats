using System;
using ProjectRogue.Movement;
using UnityEngine;

[RequireComponent(typeof(AttributeSet))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ResourcePool))]
[RequireComponent(typeof(ClassTraversal))]
public sealed class PlayerClassController : MonoBehaviour
{
    [Header("Available Starting Classes")]
    [SerializeField]
    private ClassDefinition[] availableClasses =
        new ClassDefinition[3];

    [SerializeField, Min(0)]
    private int startingClassIndex = 0;

    private AttributeSet attributes;
    private Health health;
    private ResourcePool resources;
    private ClassTraversal traversal;

    public ClassDefinition CurrentClass { get; private set; }

    public ClassDefinition[] AvailableClasses => availableClasses;

    public event Action<ClassDefinition> ClassChanged;


    private void Awake()
    {
        CacheComponents();
    }


    private void Start()
    {
        if (CurrentClass == null &&
            availableClasses != null &&
            availableClasses.Length > 0)
        {
            SelectClass(startingClassIndex);
        }
    }


    private void CacheComponents()
    {
        if (attributes == null)
            attributes = GetComponent<AttributeSet>();

        if (health == null)
            health = GetComponent<Health>();

        if (resources == null)
            resources = GetComponent<ResourcePool>();

        if (traversal == null)
            traversal = GetComponent<ClassTraversal>();
    }


    public bool SelectClass(int index)
    {
        if (availableClasses == null ||
            index < 0 ||
            index >= availableClasses.Length)
        {
            return false;
        }

        ClassDefinition definition = availableClasses[index];

        if (definition == null)
            return false;

        ApplyClass(definition);

        return true;
    }


    public bool SelectClass(ClassKind kind)
    {
        if (availableClasses == null)
            return false;

        for (int i = 0; i < availableClasses.Length; i++)
        {
            ClassDefinition definition = availableClasses[i];

            if (definition != null &&
                definition.classKind == kind)
            {
                ApplyClass(definition);
                return true;
            }
        }

        return false;
    }


    public ClassDefinition GetDefinition(ClassKind kind)
    {
        if (availableClasses == null)
            return null;

        foreach (ClassDefinition definition in availableClasses)
        {
            if (definition != null &&
                definition.classKind == kind)
            {
                return definition;
            }
        }

        return null;
    }


    private void ApplyClass(ClassDefinition definition)
    {
        CacheComponents();

        attributes.SetBaseProfile(
            definition.vigor,
            definition.strength,
            definition.dexterity,
            definition.mind,
            definition.constitution);

        health.ConfigureMax(
            definition.maxHealth,
            refill: true);

        resources.ConfigureMaxima(
            definition.maxStamina,
            definition.maxMana,
            refill: true);

        traversal.SelectClass((int)definition.classKind);

        CurrentClass = definition;

        ClassChanged?.Invoke(definition);

        Debug.Log(
            $"Selected class: {definition.displayName} | " +
            $"HP {definition.maxHealth:0}, " +
            $"Stamina {definition.maxStamina:0}, " +
            $"Mana {definition.maxMana:0}");
    }


    // Used by editor setup and automated verification.
    public void ConfigureClasses(
        ClassDefinition[] definitions,
        int startIndex = 0)
    {
        availableClasses = definitions;

        startingClassIndex = Mathf.Clamp(
            startIndex,
            0,
            Mathf.Max(0, definitions.Length - 1));
    }
}