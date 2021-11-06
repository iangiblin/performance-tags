using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;

public enum TestMode
{
    BETTERTAG_BY_LIST,
    BETTERTAG_BY_ARRAY,
    UNITY_TAG
}

public class CubeManager : MonoBehaviour
{
    // references:
    //
    // https://www.jacksondunstan.com/articles/3058
    // https://forum.unity.com/threads/list-vs-array-finding-out-which-is-faster-when-being-iterated-using-foreach-enumerator.922025/

    [SerializeField] private int _objectsPerSide = 10;
    [SerializeField] private float _localScale = 0.2f;
    [SerializeField] private Transform _parent;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private TestMode _objectFindingMode;
    [SerializeField] private int _testLoops = 10;

    [SerializeField] private TMP_Text _reportText;

    private Stopwatch _stopwatch;
    private long _msec;

    private long _allMsecs;
    private long _cycles;
    
    private void Awake()
    {
        if (_parent == null)
        {
            _parent = this.transform;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        BuildGameObjects();

        // housekeeping experiment
        BetterTag.Bake();

        _cycles = 0;
        _allMsecs = 0;
        
        _stopwatch = new Stopwatch();
    }

    private void BuildGameObjects()
    {
        // Ideally build enough objects that it takes a finite time
        // to change them every frame.
        
        for (int i = 0; i < _objectsPerSide; i++)
        {
            for (int j = 0; j < _objectsPerSide; j++)
            {
                for (int k = 0; k < _objectsPerSide; k++)
                {
                    GameObject obj;

                    var pos = new Vector3(i, j, k);

                    if (_prefab == null)
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    else
                        obj = Instantiate(_prefab, pos, Quaternion.identity);

                    obj.transform.position = new Vector3(i, j, k);
                    obj.transform.localScale = _localScale * Vector3.one;
                    obj.transform.parent = _parent;
                    obj.name = $"{obj.name} ({i},{j},{k})";
                    obj.tag = "Cube";
                }
            }
        }
    }

    // ------------------------------------------------------------------------
    void Update()
    {
        string report = "";
        
        _stopwatch.Reset();
        _stopwatch.Start();

        for (int i = 0; i < _testLoops; i++)
        {
            switch (_objectFindingMode)
            {
                case TestMode.UNITY_TAG:
                    report = SwitchByUnityTag();
                    break;

                case TestMode.BETTERTAG_BY_LIST:
                    report = SwitchByBetterTagUsingLists();
                    break;
                
                case TestMode.BETTERTAG_BY_ARRAY:
                    report = SwitchByBetterTagUsingArrays();
                    break;

            }
        }
        _msec = _stopwatch.ElapsedMilliseconds;
        
        _cycles++;
        _allMsecs += _msec;
        double average = _allMsecs / _cycles;
        
        string txt = $"{report} {_testLoops} times in {_msec} ms (average {average} ms)";
        _reportText.SetText(txt);
        Debug.Log(txt);
    }
    
    // ------------------------------------------------------------------------
    private string SwitchByUnityTag()
    {
        var cubeArray = GameObject.FindGameObjectsWithTag("Cube");
        var sphereArray = GameObject.FindGameObjectsWithTag("Sphere");
        
        for (int cc = 0; cc < cubeArray.Length; cc++) { cubeArray[cc].tag = "Sphere"; }
        for (int ss = 0; ss < sphereArray.Length; ss++) { sphereArray[ss].tag = "Cube"; }
        
        return $"Switched {cubeArray.Length + sphereArray.Length} items" +
                 $" by Unity Tag";
    }

    // ------------------------------------------------------------------------
    private string SwitchByBetterTagUsingLists()
    {
        // build lists before we change anything
        var cubeList = BetterTag.TaggedObjectsHashSet(BetterTagType.CUBE).ToList();
        var sphereList = BetterTag.TaggedObjectsHashSet(BetterTagType.SPHERE).ToList();

        // switch cubes -> spheres
        for (int cc = 0; cc < cubeList.Count; cc++)
        {
            var betterTag = cubeList[cc].GetComponent<BetterTag>();
            betterTag.UnregisterTaggedObject();
            betterTag.RegisterTaggedObject(BetterTagType.SPHERE);
        }

        // switch spheres -> cubes
        for (int ss = 0; ss < sphereList.Count; ss++)
        {
            var betterTag = sphereList[ss].GetComponent<BetterTag>();
            betterTag.UnregisterTaggedObject();
            betterTag.RegisterTaggedObject(BetterTagType.CUBE);
        }
        
        return $"Switched {cubeList.Count + sphereList.Count} items" +
                  $" by BetterTag using Lists";
    }
    
    // ------------------------------------------------------------------------
    private string SwitchByBetterTagUsingArrays()
    {
        // the only difference is the ToArray() step which might be faster.

        // build arrays before we change anything
        var cubeList = BetterTag.TaggedObjectsHashSet(BetterTagType.CUBE).ToArray();
        var sphereList = BetterTag.TaggedObjectsHashSet(BetterTagType.SPHERE).ToArray();

        // switch cubes -> spheres
        for (int cc = 0; cc < cubeList.Length; cc++)
        {
            var betterTag = cubeList[cc].GetComponent<BetterTag>();
            betterTag.UnregisterTaggedObject();
            betterTag.RegisterTaggedObject(BetterTagType.SPHERE);
        }

        // switch spheres -> cubes
        for (int ss = 0; ss < sphereList.Length; ss++)
        {
            var betterTag = sphereList[ss].GetComponent<BetterTag>();
            betterTag.UnregisterTaggedObject();
            betterTag.RegisterTaggedObject(BetterTagType.CUBE);
        }
        
        return $"Switched {cubeList.Length + sphereList.Length} items" +
               $" by BetterTag using Arrays";
    }

}