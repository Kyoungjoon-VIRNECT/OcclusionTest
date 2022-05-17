using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum TestState
{
    SPAWN,
    SHOOTING
}

[RequireComponent(typeof(ARRaycastManager))]
public class PlaneTracking : MonoBehaviour
{
    [SerializeField] GameObject prefab1;
    [SerializeField] GameObject prefab2;
    [SerializeField] Toggle changePrefabToggle;

    [SerializeField]
    [Tooltip("Instantiates this prefab on a plane at the touch location.")]
    GameObject m_PlacedPrefab;

    [SerializeField] GameObject shootingObject;

    [SerializeField] Button sizeUpButton;
    [SerializeField] Button sizeDownButton;
    [SerializeField] Button moveLeft;
    [SerializeField] Button moveRight;
    [SerializeField] Button moveUp;
    [SerializeField] Button moveDown;
    [SerializeField] Button turnPrefab;
    [SerializeField] Button changeMode;
    [SerializeField] Button rePosittion;
    [SerializeField] Button planeActive;

    private ARPlaneManager planeManager;
    private bool isPlaneActive = true;
    bool repos = false;
    TestState state = TestState.SPAWN;

    /// <summary>
    /// The prefab to instantiate on touch.
    /// </summary>
    public GameObject placedPrefab
    {
        get { return m_PlacedPrefab; }
        set { m_PlacedPrefab = value; }
    }

    /// <summary>
    /// The object instantiated as a result of a successful raycast intersection with a plane.
    /// </summary>
    public GameObject spawnedObject { get; private set; }

    void Awake()
    {
        changePrefabToggle.onValueChanged.AddListener((value) =>
       {
           if (value)
           {
               m_PlacedPrefab = prefab1;
           }
           else
           {
               m_PlacedPrefab = prefab2;
           }
       });
        m_RaycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();
        sizeUpButton.onClick.AddListener(() =>
        {
            spawnedObject.transform.localScale += new Vector3(0.1f, 0.1f, 0.1f);
        });

        sizeDownButton.onClick.AddListener(() =>
        {
            spawnedObject.transform.localScale -= new Vector3(0.1f, 0.1f, 0.1f);
        });

        moveUp.onClick.AddListener(() => { spawnedObject.transform.localPosition += new Vector3(0, 0.1f, 0);});
        moveDown.onClick.AddListener(() => { spawnedObject.transform.localPosition -= new Vector3(0, 0.1f, 0);});
        moveRight.onClick.AddListener(() => { spawnedObject.transform.localPosition += new Vector3(0.1f, 0, 0);});
        moveLeft.onClick.AddListener(() => { spawnedObject.transform.localPosition -= new Vector3(0.1f, 0, 0); });
        turnPrefab.onClick.AddListener(() => { spawnedObject.transform.Rotate(0, 90, 0); });
        changeMode.onClick.AddListener(() =>
        {
            if (state == TestState.SPAWN)
            {
                state = TestState.SHOOTING;
                changeMode.GetComponentInChildren<TextMeshProUGUI>().text = "SHOOTING ";
            }
            else if (state == TestState.SHOOTING)
            {
                state = TestState.SPAWN;
                changeMode.GetComponentInChildren<TextMeshProUGUI>().text = "SPAWN ";
            }
        });

        rePosittion.onClick.AddListener(() =>
        {
            repos ^= true;
            if (repos)
            {
                rePosittion.GetComponentInChildren<TextMeshProUGUI>().text = "drag repos allow";
            }
            else
            {
                rePosittion.GetComponentInChildren<TextMeshProUGUI>().text = "drag repos stop";
            }
        });

        planeActive.onClick.AddListener(() => {
            if (isPlaneActive)
            {
                foreach (var plane in planeManager.trackables)
                {
                    plane.gameObject.SetActive(false);
                }
                planeActive.GetComponentInChildren<TextMeshProUGUI>().text = "PlaneDisable";
                isPlaneActive = false;
            }
            else {
                foreach (var plane in planeManager.trackables)
                {
                    plane.gameObject.SetActive(true);
                }
                planeActive.GetComponentInChildren<TextMeshProUGUI>().text = "PlaneEnable";
                isPlaneActive = true;
            }
        });
    }

    bool TryGetTouchPosition(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0)
        {
            touchPosition = Input.GetTouch(0).position;
            return true;
        }

        touchPosition = default;
        return false;
    }

    void Update()
    {
        if (state == TestState.SPAWN)
        {
            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return;

            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.PlaneWithinPolygon))
            {
                // Raycast hits are sorted by distance, so the first one
                // will be the closest hit.
                var hitPose = s_Hits[0].pose;

                if (spawnedObject == null)
                {
                    spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
                    AROcclusionManager a = new AROcclusionManager();
                }
                else
                {
                    //repositioning of the object 
                    if (repos)
                    {
                        spawnedObject.transform.position = hitPose.position;
                    }
                }
            }
        }

        if (state == TestState.SHOOTING)
        {
            if (!TryGetTouchPosition(out Vector2 touchPosition))
                return;

            if (m_RaycastManager.Raycast(touchPosition, s_Hits, TrackableType.Depth))
            {
                spawnedObject = Instantiate(shootingObject, Camera.main.transform.position, Camera.main.transform.localRotation);
            }
        }
    }

    static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

    ARRaycastManager m_RaycastManager;
}

