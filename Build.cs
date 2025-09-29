using UnityEngine;
using UnityEngine.Tilemaps;

public class Build : MonoBehaviour
{
    public static Build instance { get; private set; }

    public GameObject buildUI;
    public GameObject[] constructSelectUI;//0:주거 1: 강화 2:방어 3:탈것
    public GameObject[] homeState;
    public GameObject[] enhancetateState;
    public GameObject[] defenseState;
    public GameObject[] vehicleState;
    int buildCount = 0;
    public GameObject interactionPoint;

    public int buildingCountMax = 0;
    public bool buildingUIopen;
    public bool isBuilding;//빌드중

    public Tilemap tilemap;//타일맵
    public GameObject[] buildingParts;//건물 짓는 대 필요한 파츠 프리팹
    public GameObject[] previewParts;
    public GameObject previewPartSave;
    public Material previewPartsMaterial;

    public float previewYSave;

    public LayerMask hitMask;
    public bool partsAboveBool = false;//건물 파츠 위인지
    public bool rotationbool;//회전이 가능한 건물 파츠인가?
    float rotationAngle;//회전값
    float nearPartsRotationYValue;//자석모드시 가장 가까운 게임오브젝트 의 회전값
    public GameObject previewParent;//현재 프리뷰 파츠 저장
    public float rayDistance;

    public bool isDemolitionMode;//철거모드
    BuildComplet buildComplet;//상호작용 - 청사진에 재료,삭제 등등에 대한 buildComplet값 저장
    Durability durability;
    public Transform interactionRayPos;
    public bool interactionBuild;

    public PreviewGroundDetector[] previewGroundDetector;
    public MeshRenderer floorMeshRenderer;
    public Material[] floorMaterials;
    public GameObject messageGameObject;
    float ePushTime = 0;
    float cPushTime = 0;
    float tPushTime = 0;
    //높이 값
    float yValue = 0;
    bool isGround;//땅에 닿음 - 목적 높이 가 땅보다 아래로 내려 갈수 없음

    //자석 모드인지
    public bool snapSocketbool;//붙이기 버튼

    //거리가 멀어지면 true 다시 설치가능 지역이 되면 false
    //snapSocketbool상태는 그대로지만  snapDistencFlaseFunction상태에 따라 스냅 기능이 달라짐
    public bool snapDistencFlaseFunction;

    public LayerMask addResourceLayerMask;
    public LayerMask socketLayerMask;
    private void Awake()
    {
        instance = this;

    }

    void Update()
    {

        if (Input.GetMouseButtonUp(1) && PlayerDemo.instance.toolBuild)
        {
            if (isBuilding)
            {
                ExitBuild();
            }
            else
            {
                buildingUIopen = !buildingUIopen;
            }
        }
        if (Input.GetKey(KeyCode.Escape))
        {
            ExitBuild();
        }
        buildUI.gameObject.SetActive(buildingUIopen);
        interactionPoint.SetActive(PlayerDemo.instance.toolBuild);

        if (DataManager.GetDataManager.isActive) return;
        if (isBuilding)
        {
            previewPartSave = previewParts[buildCount];
            Building();
        }
        //재료 채워넣기
        AddResource();
        if (EnhanceManager.instance.statuseWindowisActive) return;
        Debug.Log("빌드 마우스");
        Cursor.visible = buildUI.gameObject.activeSelf ? true : false;
        Cursor.lockState = buildUI.gameObject.activeSelf ? CursorLockMode.Confined : CursorLockMode.Locked;
        buildingUIopen = buildUI.gameObject.activeSelf ? true : false;
    }
    public void OpenBuildUI()
    {
        buildingUIopen = true;
        isBuilding = true;
    }

    //건물 짓기
    void Building()
    {
        messageGameObject.SetActive(true);
        PlayerDemo.instance.viewChangeBool = false;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, snapSocketbool ? socketLayerMask : hitMask))
        {
            float buildDestence = Vector3.Distance(transform.position, previewParts[buildCount].transform.position);
            Vector3 hitPoint = hit.point;
            Collider collider = hit.collider;
            GameObject partsGameObject = hit.transform.gameObject;

            // partsAboveBool이 true이면, 현재 오브젝트 위에 배치됨
            interactionBuild = true;
            partsAboveBool = hit.transform.CompareTag("Parts") || hit.transform.CompareTag("Door");

            float gameObjectHeight = partsGameObject.transform.position.y; // 기존 설치된 오브젝트의 높이 가져오기
            float colliderHeight = collider.transform.position.y;
            Vector3Int cellPosition = tilemap.WorldToCell(hitPoint);
            Vector3 cellCenterPosition = tilemap.GetCellCenterWorld(cellPosition);

            PreviewColor(buildDestence);
            SetPreviewPosition(cellCenterPosition, hitPoint, gameObjectHeight);


            previewParts[buildCount].SetActive(!isDemolitionMode);

            //거리가 6이상초과 일경우 설치 불가 
            if (buildDestence > 6) return;
            //클릭 시 건설 또는 철거 실행
            if (Input.GetMouseButtonDown(0) && !snapSocketbool && !previewGroundDetector[buildCount].isTriggerCheckBool)
            {
                HandlePlacementOrDemolition(cellPosition, hitPoint, hit);
            }
        }
        else
        {
            interactionBuild = false;
        }
    }
    public void ExitBuild()
    {
        snapSocketbool = false;
        interactionBuild = false;
        isBuilding = false;
        previewParts[buildCount].gameObject.SetActive(false);
        messageGameObject.SetActive(false);
        isDemolitionMode = false;
        buildingUIopen = false;
        previewParts[buildCount].transform.SetParent(previewParent.transform);
        ConstructSelectUIFalse();
    }

    //프리뷰 오브젝트 위치 설정
    void SetPreviewPosition(Vector3 cellCenterPosition, Vector3 hitPoint, float colliderHeight)
    {
        var buildComplet = FindNearestBuildComplet(10f);
        var snapbuildComplet = SnpaBuildComplet(50f);
        if (snapSocketbool)
        {
            previewParts[buildCount].transform.SetParent(null);
        }
        else
        {
            previewParts[buildCount].transform.SetParent(previewParent.transform);
        }
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Q) || previewYSave != 0)
        {
            snapSocketbool = false;
        }
        if (PlayerDemo.instance.snapBool)
        {
            snapSocketbool = !snapSocketbool;
        }
        if (snapSocketbool)
        {
            SnapTarget(snapbuildComplet, hitPoint);
            return;
        }
        //HeightValue : 높이 ,MouseScrollWheelValueY : 회전
        // 물체의 위치는 그대로 유지
        // 회전만 변경하고 높이는 별도로 관리
        Vector3 targetPosition = new Vector3(hitPoint.x, hitPoint.y + HeightValue(), hitPoint.z);
        if (targetPosition.y <= colliderHeight)
        {
            isGround = true;
            return;
        }
        isGround = false;
        previewParts[buildCount].transform.position = targetPosition + new Vector3(0, colliderHeight, 0);

        // 회전 값만 처리
        previewParts[buildCount].transform.localRotation = Quaternion.Euler(0, MouseScrollWheelValueY(), 0);
    }
    //거리 제한 으로 BuildComplet스크립트 검색
    private BuildComplet FindNearestBuildComplet(float maxDistance = 10f)
    {
        BuildComplet[] all = FindObjectsByType<BuildComplet>(FindObjectsSortMode.None);
        BuildComplet nearest = null;
        float minDist = maxDistance;

        foreach (var target in all)
        {
            float dist = Vector3.Distance(previewParts[buildCount].transform.position, target.transform.position);
            if (dist <= minDist)
            {
                minDist = dist;
                nearest = target;
                interactionRayPos.transform.position = interactionRayPos.transform.position + new Vector3(0, nearest.transform.position.y, 0);
                nearPartsRotationYValue = target.transform.eulerAngles.y;
            }
        }

        return nearest;
    }
    //거리 제한 으로 BuildComplet스크립트 검색
    private BuildComplet SnpaBuildComplet(float maxDistance = 50f)
    {
        BuildComplet[] all = FindObjectsByType<BuildComplet>(FindObjectsSortMode.None);
        BuildComplet nearest = null;
        float minDist = maxDistance;

        foreach (var target in all)
        {
            float dist = Vector3.Distance(previewParts[buildCount].transform.position, target.transform.position);
            if (dist <= minDist)
            {
                minDist = dist;
                nearest = target;
                interactionRayPos.transform.position = interactionRayPos.transform.position + new Vector3(0, nearest.transform.position.y, 0);
                nearPartsRotationYValue = target.transform.eulerAngles.y;
            }
            else
            {
                snapSocketbool = false;
            }
        }

        return nearest;
    }


    //자석 효과 - 붙이기
    float snapDistence()
    {
        BuildComplet snapTarget = SnpaBuildComplet(50);

        return Vector3.Distance(transform.position, snapTarget.gameObject.transform.position);
    }
    void SnapTarget(BuildComplet buildComplet, Vector3 hitPoint)
    {
        Debug.Log("SnapTarget");
        float minDistance = float.MaxValue;
        Transform closestMySocket = null;
        Transform closestTargetSocket = null;
        Debug.Log($"closestMySocket : {closestMySocket} \n  closestTargetSocket : {closestTargetSocket}");


        foreach (Transform mySocket in previewGroundDetector[buildCount].socetTransforms)
        {
            if (buildComplet == null) return;
            foreach (Transform tartgetSocket in buildComplet.socetTransforms)
            {
                float distence = Vector3.Distance(mySocket.position, tartgetSocket.position);
                if (distence < minDistance)
                {
                    minDistance = distence;
                    closestMySocket = mySocket;
                    closestTargetSocket = tartgetSocket;
                }
            }
        }


        if (closestMySocket == null || closestTargetSocket == null)
        {
            FindNearestBuildComplet(10);
            Debug.Log(snapDistence());
            return;
        }
        Debug.Log(snapDistence());

        // 1. 위치 먼저 조정
        if (snapDistence() <= 10)
        {
            Vector3 delta = closestTargetSocket.position - closestMySocket.position;
            previewParts[buildCount].transform.position += delta;
        }
        else
        {
            previewParts[buildCount].transform.position = hitPoint;
        }



        // 2. 회전 조정 (Y축만)
        previewParts[buildCount].transform.rotation = Quaternion.Euler(0, nearPartsRotationYValue, 0);

        // 3. 설치
        if (Input.GetMouseButtonUp(0) && snapSocketbool)
        {
            if (previewGroundDetector[buildCount].isTriggerCheckBool) return;
            Vector3 finalPos = previewParts[buildCount].transform.position;
            GameObject parts = Instantiate(buildingParts[buildCount]);
            parts.transform.position = finalPos;
        }
    }

    //클릭 시 건설 또는 철거 실행
    void HandlePlacementOrDemolition(Vector3Int cellPosition, Vector3 hitPoint, RaycastHit hit)
    {
        if (!isDemolitionMode)
        {
            if (!previewGroundDetector[buildCount].isTriggerCheckBool && !snapSocketbool)
            {
                Vector3 targetPosition = new Vector3(hitPoint.x, hitPoint.y + HeightValue(), hitPoint.z);
                GameObject parts = Instantiate(buildingParts[buildCount]);
                parts.transform.position = targetPosition;

            }
        }
    }

    //프리뷰 파츠 생상 변경
    public void PreviewColor(float distence)
    {
        Color color = previewPartsMaterial.color;
        if (distence > 6)
        {
            color = Color.red;
            previewPartsMaterial.color = color;
            return;
        }

        if (!previewGroundDetector[buildCount].isTriggerCheckBool)
        {
            color = Color.green;
        }
        else
        {
            color = Color.red;
        }

        previewPartsMaterial.color = color;
    }
    float MouseScrollWheelValueY()
    {
        float wheelInput = Input.mouseScrollDelta.y;
        previewYSave = Input.mouseScrollDelta.y;
        // 회전값을 누적해서 업데이트
        rotationAngle -= wheelInput;
        return rotationAngle;
    }
    float HeightValue()
    {
        bool yPluse = Input.GetKey(KeyCode.Q);
        bool yMinus = Input.GetKey(KeyCode.E);
        if (yPluse)
        {
            yValue += Time.deltaTime;
            return yValue;
        }
        else if (yMinus && !isGround)
        {
            yValue -= Time.deltaTime;
            return yValue;
        }
        else
        {
            return yValue;
        }
    }
    //건물 재료 넣기
    void AddResource()
    {
        if (!PlayerDemo.instance.toolBuild) return;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
        bool interactionHit = Physics.Raycast(ray, out RaycastHit hit, (rayDistance + 6), addResourceLayerMask);

        Debug.DrawRay(ray.origin, ray.direction * (rayDistance + 6), Color.red);

        if (interactionHit)
        {
            int layer = hit.transform.gameObject.layer;
            buildComplet = hit.transform.GetComponent<BuildComplet>();
            durability = hit.transform.GetComponent<Durability>();
            if (buildComplet == null) return;
            buildComplet.ui[0].gameObject.SetActive(layer == 9);
            buildComplet.ui[1].gameObject.SetActive(layer == 7);
            buildComplet.ui[2].gameObject.SetActive(layer == 11);
            //건설 취소 - 청사진을 삭제한다
            if (layer == 9 && Input.GetKeyUp(KeyCode.C))
            {
                Debug.Log("청사진 삭제 : " + hit.transform.gameObject.name);
                Destroy(hit.transform.gameObject);
            }
            else if (layer == 7)
            {
                if (Input.GetKey(KeyCode.R))
                {
                    Debug.Log("건설 파츠 삭제 : " + hit.transform.gameObject.name);

                    cPushTime += Time.deltaTime;
                    buildComplet.gauge[1].fillAmount = cPushTime;
                    if (cPushTime >= 1f)
                    {
                        Destroy(hit.transform.gameObject);
                        cPushTime = 0;
                    }
                }
                else
                {
                    cPushTime = 0;
                    buildComplet.gauge[1].fillAmount = cPushTime;
                }
            }
            //재료 넣기 - 하나씩
            if (layer == 9)
            {
                if (Input.GetKeyUp(KeyCode.E))//하나씩
                {
                    Debug.Log("재료 하나 넣기 : " + hit.transform.gameObject.name);

                    if (buildComplet.materialType[0])
                    {
                        Debug.Log("넣기 성공");
                        buildComplet.currentWood++;
                    }
                    else if (buildComplet.materialType[1])
                    {
                        buildComplet.currentStone++;
                    }

                }
                if (Input.GetKey(KeyCode.E))//한번에
                {
                    ePushTime += Time.deltaTime;
                    buildComplet.gauge[0].fillAmount = ePushTime;
                    if (ePushTime >= 1f)//한번에
                    {
                        Debug.Log("재료 한번에 넣기 : " + hit.transform.gameObject.name);

                        if (buildComplet.materialType[0])
                        {
                            buildComplet.currentWood += (buildComplet.requiredWood - buildComplet.currentWood);
                        }
                        else if (buildComplet.materialType[1])
                        {
                            buildComplet.currentStone += (buildComplet.requiredStone - buildComplet.currentStone);
                        }
                        ePushTime = 0;
                    }
                }
                else
                {
                    ePushTime = 0;
                    buildComplet.gauge[0].fillAmount = ePushTime;
                }
            }
            //수리
            else if (hit.transform.gameObject.layer == 11)
            {
                if (Input.GetKeyUp(KeyCode.T))//하나씩
                {
                    Debug.Log("수리 : " + hit.transform.gameObject.name);
                    if (buildComplet.materialType[0])
                    {
                        durability.currentDurability++;

                    }
                    else if (buildComplet.materialType[1])
                    {
                        durability.currentDurability++;

                    }
                }
                if (Input.GetKey(KeyCode.T))//한번에
                {
                    tPushTime += Time.deltaTime;
                    buildComplet.gauge[2].fillAmount = tPushTime;
                    if (tPushTime >= 1f)//한번에
                    {
                        Debug.Log("한번에 수리 : " + hit.transform.gameObject.name);

                        if (buildComplet.materialType[0])
                        {
                            durability.currentDurability += (durability.maxDurability - durability.currentDurability);
                        }
                        else if (buildComplet.materialType[1])
                        {
                            durability.currentDurability += (durability.maxDurability - durability.currentDurability);
                        }
                        tPushTime = 0;
                    }
                }
                else
                {
                    tPushTime = 0;
                    buildComplet.gauge[2].fillAmount = tPushTime;
                }
            }
        }
        else
        {
            if (buildComplet == null) return;
            for (int i = 0; i < buildComplet.ui.Length; i++)
            {
                buildComplet.ui[i].gameObject.SetActive(false);
            }

        }
    }
    //버튼 할당
    //빌드 윈도우 비활성화 
    public void BuildWindow()
    {
        buildingUIopen = false;
    }

    // Construct 게임오브젝트 활성화 : 0 :집 , 1:강화 2:방어 3 : 탈것
    public void ConstructType(int type)
    {
        bool currentState = constructSelectUI[type].activeSelf;
        constructSelectUI[type].SetActive(!currentState);
    }
    private void ConstructSelectUIFalse()
    {
        for (int i = 0; i < constructSelectUI.Length; i++)
        {
            constructSelectUI[i].SetActive(false);
        }
    }
    public void HomeState(int type)
    {
        for (int i = 0; i <= homeState.Length - 1; i++)
        {
            homeState[i].SetActive(i == type);
        }
    }

    public void EnhancetateState(int type)
    {
        for (int i = 0; i <= enhancetateState.Length - 1; i++)
        {
            enhancetateState[i].SetActive(i == type);
        }
    }
    public void DefenseState(int type)
    {
        for (int i = 0; i <= defenseState.Length - 1; i++)
        {
            defenseState[i].SetActive(i == type);
        }
    }
    public void VehicleState(int type)
    {
        for (int i = 0; i <= vehicleState.Length - 1; i++)
        {
            vehicleState[i].SetActive(i == type);
        }
    }
    public void PreviousButton(int type)
    {
        bool currentState = constructSelectUI[type].activeSelf;
        constructSelectUI[type].SetActive(!currentState);
    }
    public void BuildID(int id)
    {
        BuildData data = DataManager.GetDataManager.FindBuildDataById(id);
        buildCount = data.order - 1;
        isBuilding = true;
        buildingUIopen = false;
        if (data != null)
        {
            Debug.Log($"이름: {data.nameKey}, 타입: {data.type}, 순서: {data.order}, 레시피: {data.buildRecipeId}");
        }
    }

}
