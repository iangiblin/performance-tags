using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum BetterTagType
{
    CUBE,
    SPHERE,
    PYRAMID
}

public class BetterTag : MonoBehaviour
{
    // This is meant to be a faster replacement for FindObjectsWithTag
    
    // Put this component on each object you need to find later, then get a
    // HashSet<GameObject> from BetterTag.SetOfTaggedObjects(_tag).
    //
    // Notes:    * Disabled objects get removed from the set during OnDisable
    //           * You can add/remove multiple times, but it's discouraged
    //           * ...

    // ================ STATIC =========================
    
    private static Dictionary<BetterTagType, HashSet<GameObject>> tagSet;
    private static Dictionary<BetterTagType, List<GameObject>> tagSetList;

    private static HashSet<GameObject> _emptyHashSet = new HashSet<GameObject>();
    private static List<GameObject> _emptyList = new List<GameObject>();

    // ------------------------------------------------------------------------
    private static void AddToSet(GameObject obj, BetterTagType myTag, bool bake=false)
    {
        // create dictionary entry if missing
        if (tagSet.ContainsKey(myTag) == false)
        {
            Debug.Log($"creating new tracking HashSet <B>{myTag}</B>");
            tagSet[myTag] = new HashSet<GameObject>();
        }
        
        // add to dictionary
        tagSet[myTag].Add(obj);

        if (bake)
        {
            // prebuild the list, very inefficient to rebake this every time!
            tagSetList[myTag] = tagSet[myTag].ToList();
        }
    }

    public static void Bake()
    {
        // after manipulating large hashsets, you can bake them into lists; this
        // is just an experiment and would only be useful where you're not changing
        // tags at run time.
        
        foreach (BetterTagType myTag in tagSet.Keys)
        {
            tagSetList[myTag] = tagSet[myTag].ToList();
        }
    }
    
    // ------------------------------------------------------------------------
    private static void RemoveFromSet(GameObject obj, BetterTagType myTag)
    {
        if (tagSet.ContainsKey(myTag) == false)
        {
            // this can happen when we pre-warm the object pool, from OnDisable,
            // and it may be OK. Leaving warning on for now.
            
            #if UNITY_EDITOR
            Debug.LogError($"RemoveFromSet: key <B>{myTag}</B> not" +
                           $" in master Dictionary");
            #endif
            return;
        }
        
        if (tagSet[myTag].Remove(obj) == false)
        {
            Debug.LogError($"RemoveFromSet: Known Tag <B>{myTag}</B>" +
                           $" but unknown object {obj}");
            return;
        }
    }

    // ------------------------------------------------------------------------
    public static HashSet<GameObject> TaggedObjectsHashSet(BetterTagType myTag)
    {
        // remember, HashSet is like an unordered list with set-like uniqueness
        
        if (tagSet.ContainsKey(myTag))
            return tagSet[myTag];

        return _emptyHashSet;
    }

    public static List<GameObject> TaggedObjectsList(BetterTagType myTag)
    {
        if (tagSet.ContainsKey(myTag))
            return tagSetList[myTag];

        return _emptyList;
    }

    // ------------------------------------------------------------------------
    public static GameObject Singleton(BetterTagType myTag, bool required=true)
    {
        // example: _alphabetManager = BetterTag.Singleton(BetterTagType.ALPHABET_MANAGER);

        // there should be exactly one of these
        if (tagSet.ContainsKey(myTag) && tagSet[myTag].Count == 1)
            return tagSet[myTag].First();

        if (tagSet.ContainsKey(myTag) == false)
        {
            if (required)
            {
                Debug.LogError($"Expected Singleton of {myTag}, found nothing");
            }
        }
        else
        {
            if (required)
            {
                Debug.LogError($"Expected Singleton of {myTag}, found {tagSet[myTag].Count}");
                foreach (var o in tagSet[myTag])
                {
                    Debug.LogError($"{myTag}: {o.name}");
                }
            }
            return tagSet[myTag].First();
        }

        return null;
    }

    // ============= NOT STATIC ===============================================

    [SerializeField] private BetterTagType _defaultTag;
    
    private BetterTagType _tag;

    private void Awake()
    {
        // Objects could switch tags many times during gameplay, especially
        // for shared projectile pools etc., but assigning once here
        // will happen before first usage when we are pre-warming pools.
        
        _tag = _defaultTag;
        AddToSet(gameObject, _tag);
    }

    // ------------------------------------------------------------------------
    public void RegisterTaggedObject(BetterTagType myTag)
    {
        BetterTag.AddToSet(gameObject, myTag);
        _tag = myTag;
    }

    // sometimes we take an object from the pool and it is not correctly
    // registered. Why is that? Anyway we can re-register it here.
    
    public void RegisterJustInCase()
    {
        RegisterTaggedObject(_tag);
    }
    
    // ------------------------------------------------------------------------
    private void OnDisable()
    {
        UnregisterTaggedObject();
    }

    // ------------------------------------------------------------------------
    public void UnregisterTaggedObject()
    {
        BetterTag.RemoveFromSet(gameObject, _tag);
        _tag = _defaultTag;
    }
    
    // ------------------------------------------------------------------------
    // for debugging only: get a list of tagged objects from a tagged object!
    
    [ContextMenu("Report all matching objects")]
    private void ReportAllMatchingObjects()
    {
        var items = TaggedObjectsHashSet(_tag).ToList();
        Debug.Log($"Looking for <B><color=blue>{_tag}</color></B>:" +
                  $" found {items.Count}");
        foreach (var taggedObject in items)
        {
            Debug.Log($"{taggedObject} "
                      + (taggedObject.gameObject.activeSelf ? "ACTIVE" : "") );
        }
    }
}
