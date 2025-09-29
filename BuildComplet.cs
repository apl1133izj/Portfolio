using UnityEngine;
using UnityEngine.UI;

public class BuildComplet : MonoBehaviour
{
    Material material;
    public int requiredWood;
    public int currentWood;
    public int requiredStone;
    public int currentStone;
    public GameObject penelGameObject;
    public GameObject rotationTransform;
    public Text[] completText;
    public Text[] currentText;
    public int id;
    public GameObject socetInst;
    public Transform[] socetTransforms;
    public bool[] materialType; //0:목제 ,1:돌
    public GameObject[] ui;//0:남은 겟수,1:삭제 2: 수리
    public Image[] gauge;//0:한번에 넣기 게이지 ,1:삭제 게이지 2:수리
    public bool well;
    GameObject playerDistence;
    PlayerDemo player;
    public bool buildCompletBool;
    private void Awake()
    {
        Socet();
    }
    void Start()
    {
        playerDistence = GameObject.Find("PlayerDemo");
        player = FindAnyObjectByType<PlayerDemo>();
        Renderer renderer = GetComponent<Renderer>();
        material = renderer.material; // 렌더러가 있다면 Material 할당

    }

    // Update is called once per frame
    void Update()
    {
        
        Vector3 targetPosition = player.transform.position;
        Vector3 lookDirection = targetPosition - penelGameObject.transform.position;
        Vector3 lookDirection2 = targetPosition - ui[1].transform.position;
        Vector3 lookDirection3 = targetPosition - ui[2].transform.position;
        lookDirection.y = 0; // Y축 방향 제거

        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-lookDirection);
            penelGameObject.transform.rotation = Quaternion.Slerp(
                penelGameObject.transform.rotation,
                targetRotation,
                Time.deltaTime * 20
            );
        }
        if (lookDirection2 != Vector3.zero)
        {
            Quaternion targetRotation2 = Quaternion.LookRotation(-lookDirection);
            ui[1].transform.rotation = Quaternion.Slerp(
                 ui[1].transform.rotation,
                targetRotation2,
                Time.deltaTime * 20
            );
            if (lookDirection3 != Vector3.zero)
            {
                Quaternion targetRotation3 = Quaternion.LookRotation(-lookDirection);
                ui[2].transform.rotation = Quaternion.Slerp(
                     ui[2].transform.rotation,
                    targetRotation3,
                    Time.deltaTime * 20
                );
            }
        }

        //BuildData buildDatal = DataManager.GetDataManager.FindBuildDataById(id);
        completText[0].text = requiredWood.ToString();
        completText[1].text = requiredStone.ToString();
        currentText[0].text = currentWood.ToString();
        currentText[1].text = currentStone.ToString();
        //모든 청사진 재료가 넣어졌을경우
        if (currentWood == requiredWood && currentStone == requiredStone)
        {
            if (!penelGameObject.activeSelf) return;
            Color color = material.color;
            color.a = 1f;
            material.color = Color.white;
            gameObject.layer = 7;
            BoxCollider boxCollider = GetComponent<BoxCollider>();
            boxCollider.isTrigger = false;
            penelGameObject.SetActive(false);
        }else
        {
            gameObject.layer = 9;
        }
        if(Distence() >= 10)
        {
            penelGameObject.SetActive(false);
        }else
        {
            penelGameObject.SetActive(true);
        }
        if (Build.instance.isBuilding) return;
        penelGameObject.SetActive(false);
    }
    float Distence()
    {
        return Vector3.Distance(gameObject.transform.position, playerDistence.transform.position);  
    }

    public void Socet()
    {
        Bounds bounds = new Bounds();
        if (TryGetComponent<Renderer>(out var rend))
        {
            bounds = rend.bounds;
        }

        float halfX = bounds.size.x / 2;
        float halfZ = bounds.size.z / 2;
        float halfY = bounds.size.y / 2;

        Vector3[] directions;
        if (!well)
        {
            directions = new Vector3[]
            {
            new Vector3(+halfX, 0, 0),  // 오른쪽
            new Vector3(-halfX, 0, 0),  // 왼쪽
            new Vector3(0, 0, +halfZ),  // 앞쪽
            new Vector3(0, 0, -halfZ)   // 뒤쪽
            };
        }
        else
        {
            directions = new Vector3[]
            {
            new Vector3(+halfX, 0, 0),  // 오른쪽
            new Vector3(-halfX, 0, 0),  // 왼쪽
            new Vector3(0, +halfY, 0),  // 위쪽
            new Vector3(0, -halfY, 0)   // 아래쪽
            };
        }

        socetTransforms = new Transform[directions.Length];
        for (int i = 0; i < directions.Length; i++)
        {
            GameObject socet = Instantiate(socetInst, transform);
            Vector3 rotatedDirection = transform.rotation * directions[i];
            socet.transform.position = bounds.center + rotatedDirection;
            socet.name = $"Socet_{i}";
            socetTransforms[i] = socet.transform;
        }
        gameObject.transform.rotation = Build.instance.previewPartSave.transform.rotation;
    }
}
