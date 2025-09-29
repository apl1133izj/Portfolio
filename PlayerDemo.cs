using DoorScript;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDemo : MonoBehaviour
{
    public static PlayerDemo instance { get; private set; }

    public float health;
    [Header("플레이어 데이터")]
    public int playerid;
    public string[] playerInfo;
    public float healthMax;                     // 최대체력
    public float stamina;                    // 스태미나
    public float satiety;                    // 포만감
    public float attackPower;               // 공격력
    public float attackSpeed;               // 공격속도
    public float criticalChance;           // 치명타 확률
    public float criticalDamage;           // 치명타 피해
    public float defense;                   // 방어력
    public float damageReduction;         // 받는 데미지 감소
    public float moveSpeed;               // 이동 속도
    public float moveSpeedEnhance;

    public float jumpPower;               // 점프력
    public float healthRegen;             // 체력 재생
    public float staminaRegen;            // 스태미나 회복량
    public float attackStaminaReduction; // 공격 스태미나 소모 감소
    public float dodgeStaminaReduction;  // 회피 스태미나 소모 감소
    public float satietyDecay;            // 포만감 감소량
    public float expGain;                 // 경험치 획득량
    public Text[] playerInfoText;

    // 기본 스탯
    public float baseHealthMax = 100f;
    public float baseStamina = 50f;
    public float baseSatiety = 50f;
    public float baseAttackPower = 10f;
    public float baseAttackSpeed = 1.0f;
    public float baseCriticalChance = 5f;
    public float baseDefense = 5f;
    public float baseMoveSpeed = 5f;
    public float baseMoveSpeedEnhance;
    [Header("이동 속도 관련")]
    public float defenseBlend;
    public float moveFBBlend;
    public float moveRLBlend;
    public float runSpeed;
    public float walkSpeed;
    public float shitSlow;
    [Header("키 - 움직임에 대한 불 값")]
    public string[] keySatting;
    public int playerStateLayer;
    public bool forwardBool;
    public bool backBool;
    public bool rightMoveBool;
    public bool leftMoveBool;
    public bool runBool;
    public bool shitBool;
    public bool jumpBool;
    public bool pickUpItemBool;
    public bool moveFBBool;
    public bool moveLRBool;
    public bool viewChangeBool;
    public bool snapBool;
    public bool[] toolBool;
    public LayerMask borderLayerMask;

    Rigidbody rb;

    [Header("키 - 공격에 대한 불 값")]
    public bool attackBool;
    public float attackBlend;
    public bool toolGun;
    public bool toolWork;
    public bool toolBuild;
    public bool aim_CancelBool;
    public PlayerAnimator playerAnimator;

    //카메라 관련
    public Camera mainCamera; // 플레이어의 카메라 (1인칭/3인칭 카메라)


    [SerializeField]
    //도구 게임 오브젝트
    public List<ToolGameObject> toolGameObjects = new List<ToolGameObject>();
    //도구 마다 애니메이션 방향
    private Dictionary<int, GameObject[]> toolGameObjectDict = new Dictionary<int, GameObject[]>();

    //도구
    float itemPickUpState; //0:가만이 있는 상태에서,1:뛰거나 걷는 상태
    public float[] bulletSpawnFrame;
    public Transform[] bulletInstPos;
    public GameObject[] bulletPrefab;
    public int[] bullet;
    public int[] magazine;//탄창
    public bool toolChange;
    public bool reLoad;//장전
    public int toolState;
    public int bulletCount;
    public int[] bullsetSpawDelay;
    [Header("상호작용")]
    Build build;
    public LayerMask interactionLayerMask;//레이에 닿은 오브젝트가 interactionLayerMask와 일치 해야함
    public GameObject interactionMessage;
    public bool interactionBool;
    public float rayDistance;
    public bool doorOpenBool;

    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        PlayerSatting();
        PlayerData();
    }

    public void PlayerSatting()
    {
        foreach (var tool in toolGameObjects)
        {
            toolGameObjectDict[tool.key] = tool.toolGameObjects;
        }
        rb = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<PlayerAnimator>();
        build = GetComponent<Build>();
        mainCamera = Camera.main; // 메인 카메라 가져오기
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }
    //csv에서 기본 값 가지고 오기
    public void PlayerData()
    {

        playerid = DataManager.GetDataManager.player_Id;
        playerInfo = new string[playerid];
        keySatting = new string[playerid];
        playerInfo = DataManager.GetDataManager.GetPlayerData(playerid.ToString());
        keySatting = DataManager.GetDataManager.GetKeySattingPlayerData(playerid.ToString());
        healthMax = float.Parse(playerInfo[1]);                  // 체력        
        stamina = float.Parse(playerInfo[2]);                 // 스태미나
        satiety = float.Parse(playerInfo[3]);                 // 포만감
        attackPower = float.Parse(playerInfo[4]);             // 공격력
        attackSpeed = float.Parse(playerInfo[5]);             // 공격속도
        criticalChance = float.Parse(playerInfo[6]);          // 치명타 확률
        criticalDamage = float.Parse(playerInfo[7]);          // 치명타 피해
        defense = float.Parse(playerInfo[8]);                 // 방어력
        damageReduction = float.Parse(playerInfo[9]);         // 받는 데미지 감소
        runSpeed = float.Parse(playerInfo[10]);              // 이동 속도
        walkSpeed = float.Parse(playerInfo[11]);
        jumpPower = float.Parse(playerInfo[12]);              // 점프력
        healthRegen = float.Parse(playerInfo[13]);            // 체력 재생
        staminaRegen = float.Parse(playerInfo[14]);           // 스태미나 회복량
        attackStaminaReduction = float.Parse(playerInfo[15]); // 공격 스태미나 소모 감소
        dodgeStaminaReduction = float.Parse(playerInfo[16]);  // 회피 스태미나 소모 감소
        satietyDecay = float.Parse(playerInfo[17]);           // 포만감 감소량
        expGain = float.Parse(playerInfo[18]);                // 경험치 획득량

        health = healthMax;
        ResetToBaseStats();
    }
    public void ResetToBaseStats()
    {
        baseHealthMax = healthMax;
        baseStamina = stamina;
        baseSatiety = satiety;

        baseAttackPower = attackPower;
        baseAttackSpeed = attackSpeed;
        baseCriticalChance = criticalChance;

        baseDefense = defense;
        baseMoveSpeed = moveSpeed;
    }
    void Update()
    {
        if (!build.buildUI.activeSelf)//크래프팅UI가 비활성화인 경우만
        {
            Key();
            Mover();
            Attack();
            Tool();
            Interaction();
            SetToolActiveState();
        }
        else
        {
            leftMoveBool = false;
            rightMoveBool = false;
            forwardBool = false;
            backBool = false;
            runBool = false;
            shitBool = false;
            jumpBool = false;
            moveFBBool = false;
            moveLRBool = false;
        }
    }
    public void Tool()
    {
        if (!reLoad)
        {

            if (toolBool[0])//맨손
            {
                toolState = 0;
                attackBlend = 0;
                toolGun = false;
                toolWork = false;
                toolBuild = false;
            }
            else if (toolBool[1])//단검
            {
                toolState = 1;
                attackBlend = 0;
                toolGun = false;
                toolWork = false;
                toolBuild = false;
            }

            if (toolBool[2])//권총
            {
                toolState = 2;
                toolGun = true;
                toolWork = false;
                toolBuild = false;
                attackBlend = 0;
            }
            else if (toolBool[3])//라이플
            {
                toolState = 2;
                toolGun = true;
                toolWork = false;
                toolBuild = false;
                attackBlend = 1;
            }
            else if (toolBool[4])//샷건
            {
                toolState = 2;
                toolGun = true;
                toolWork = false;
                toolBuild = false;
                attackBlend = 2;
            }
            else if (toolBool[5])//스나이퍼 라이플
            {
                toolState = 2;
                toolGun = true;
                toolWork = false;
                toolBuild = false;
                attackBlend = 3;
            }

            if (toolBool[6])//건축 도구
            {
                toolState = 3;
                toolGun = false;
                toolWork = false;
                toolBuild = true;
                attackBlend = 0;
            }
            else if (toolBool[7])//파밍 도구
            {
                toolState = 3;
                toolGun = false;
                toolWork = true;
                toolBuild = false;
                attackBlend = 1;
            }
        }
    }

    private void Key()
    {
        KeyCode leftKey, rightKey, forwardKey, backKey, runKey, shiftKey, jumpKey, interactKey, viewChange, snap, tool1, tool2, tool3, tool4, tool5, tool6, tool7,
                 AttackKey, aim_Cancel;


        bool result;
        string keyString;

        // 앞으로 가기 키
        keyString = DataManager.GetDataManager.isUsingCustomKeys[0] ? DataManager.GetDataManager.customKeyValue[0].ToUpper() : keySatting[1].ToUpper();
        result = Enum.TryParse(keyString, out forwardKey);
        //Debug.Log($"[키 설정] forwardKey: {keyString} → {forwardKey} (성공: {result})");

        // 뒤로 가기 키
        keyString = DataManager.GetDataManager.isUsingCustomKeys[1] ? DataManager.GetDataManager.customKeyValue[1].ToUpper() : keySatting[2].ToUpper();
        result = Enum.TryParse(keyString, out backKey);
        //Debug.Log($"[키 설정] backKey: {keyString} → {backKey} (성공: {result})");

        // 우측 이동 키
        keyString = DataManager.GetDataManager.isUsingCustomKeys[2] ? DataManager.GetDataManager.customKeyValue[2].ToUpper() : keySatting[3].ToUpper();
        result = Enum.TryParse(keyString, out rightKey);
        // Debug.Log($"[키 설정] rightKey: {keyString} → {rightKey} (성공: {result})");

        // 좌측 이동 키
        keyString = DataManager.GetDataManager.isUsingCustomKeys[3] ? DataManager.GetDataManager.customKeyValue[3].ToUpper() : keySatting[4].ToUpper();
        result = Enum.TryParse(keyString, out leftKey);
        //Debug.Log($"[키 설정] leftKey: {keyString} → {leftKey} (성공: {result})");

        // 런 키
        keyString = DataManager.GetDataManager.isUsingCustomKeys[4] ? DataManager.GetDataManager.customKeyValue[4].ToUpper() : keySatting[5];
        result = Enum.TryParse(keyString, out runKey);
        // Debug.Log($"[키 설정] runKey: {keyString} → {runKey} (성공: {result})");

        // Shift 키
        keyString = DataManager.GetDataManager.isUsingCustomKeys[5] ? DataManager.GetDataManager.customKeyValue[5].ToUpper() : keySatting[6].ToUpper();
        result = Enum.TryParse(keyString, out shiftKey);
        // Debug.Log($"[키 설정] shiftKey: {keyString} → {shiftKey} (성공: {result})");

        // 점프 키
        keyString = DataManager.GetDataManager.isUsingCustomKeys[6] ? DataManager.GetDataManager.customKeyValue[6].ToUpper() : keySatting[7];
        result = Enum.TryParse(keyString, out jumpKey);
        // Debug.Log($"[키 설정] jumpKey: {keyString} → {jumpKey} (성공: {result})");

        // 상호작용 키
        keyString = DataManager.GetDataManager.isUsingCustomKeys[7] ? DataManager.GetDataManager.customKeyValue[7].ToUpper() : keySatting[8].ToUpper();
        result = Enum.TryParse(keyString, out interactKey);
        // Debug.Log($"[키 설정] interactKey: {keyString} → {interactKey} (성공: {result})");

        // 시점변경 작용 키
        keyString = DataManager.GetDataManager.isUsingCustomKeys[8] ? DataManager.GetDataManager.customKeyValue[8].ToUpper() : keySatting[9].ToUpper();
        result = Enum.TryParse(keyString, out viewChange);
        // Debug.Log($"[키 설정] interactKey: {keyString} → {interactKey} (성공: {result})");

        // 아이템 키
        keyString = DataManager.GetDataManager.isUsingCustomKeys[9] ? DataManager.GetDataManager.customKeyValue[9].ToUpper() : keySatting[10];
        result = Enum.TryParse(keyString, out snap);
        // Debug.Log($"[키 설정] tool1: {keyString} → {tool1} (성공: {result})");

        keyString = DataManager.GetDataManager.isUsingCustomKeys[10] ? DataManager.GetDataManager.customKeyValue[10].ToUpper() : keySatting[11];
        result = Enum.TryParse(keyString, out tool1);
        // Debug.Log($"[키 설정] tool2: {keyString} → {tool2} (성공: {result})");

        keyString = DataManager.GetDataManager.isUsingCustomKeys[11] ? DataManager.GetDataManager.customKeyValue[11].ToUpper() : keySatting[12];
        result = Enum.TryParse(keyString, out tool2);
        //Debug.Log($"[키 설정] tool3: {keyString} → {tool3} (성공: {result})");

        keyString = DataManager.GetDataManager.isUsingCustomKeys[12] ? DataManager.GetDataManager.customKeyValue[12].ToUpper() : keySatting[13];
        result = Enum.TryParse(keyString, out tool3);
        //Debug.Log($"[키 설정] tool4: {keyString} → {tool4} (성공: {result})");

        keyString = DataManager.GetDataManager.isUsingCustomKeys[13] ? DataManager.GetDataManager.customKeyValue[13].ToUpper() : keySatting[14];
        result = Enum.TryParse(keyString, out tool4);
        // Debug.Log($"[키 설정] tool5: {keyString} → {tool5} (성공: {result})");

        keyString = DataManager.GetDataManager.isUsingCustomKeys[14] ? DataManager.GetDataManager.customKeyValue[14].ToUpper() : keySatting[15];
        result = Enum.TryParse(keyString, out tool5);
        //Debug.Log($"[키 설정] tool6: {keyString} → {tool6} (성공: {result})");
        keyString = DataManager.GetDataManager.isUsingCustomKeys[15] ? DataManager.GetDataManager.customKeyValue[15].ToUpper() : keySatting[16];
        result = Enum.TryParse(keyString, out tool6);
        // Debug.Log($"[키 설정] tool7: {keyString} → {tool7} (성공: {result})");

        keyString = DataManager.GetDataManager.isUsingCustomKeys[16] ? DataManager.GetDataManager.customKeyValue[16].ToUpper() : keySatting[17];
        result = Enum.TryParse(keyString, out tool7);
        //Debug.Log($"[키 설정] tool8: {keyString} → {tool8} (성공: {result})");

        keyString = DataManager.GetDataManager.isUsingCustomKeys[17] ? DataManager.GetDataManager.customKeyValue[17].ToUpper() : keySatting[18];
        result = Enum.TryParse(keyString, out AttackKey);
        // Debug.Log($"[키 설정] tool7: {keyString} → {tool7} (성공: {result})");

        keyString = DataManager.GetDataManager.isUsingCustomKeys[18] ? DataManager.GetDataManager.customKeyValue[18].ToUpper() : keySatting[19];
        result = Enum.TryParse(keyString, out aim_Cancel);
        //Debug.Log($"[키 설정] tool8: {keyString} → {tool8} (성공: {result})");
        Input.GetKeyDown(KeyCode.Mouse0);
        leftMoveBool = Input.GetKey(leftKey);//왼쪽
        rightMoveBool = Input.GetKey(rightKey);//오른쪽
        forwardBool = Input.GetKey(forwardKey);//앞
        backBool = Input.GetKey(backKey);//뒤
        runBool = Input.GetKey(runKey);//달리기
        shitBool = Input.GetKey(shiftKey);//앉기
        jumpBool = Input.GetKey(jumpKey);//점프
        snapBool = Input.GetKeyDown(snap);//건축 - 자석
        attackBool = Input.GetKeyDown(AttackKey);
        aim_CancelBool = Input.GetKeyDown(aim_Cancel);
        if (!Build.instance.isBuilding && !Build.instance.interactionBuild)//건설 중인때 상호 작용 버튼 과 시점 변경이 불가
        {
            interactionBool = Input.GetKeyDown(interactKey);//상호작용
            if (Input.GetKeyDown(viewChange))//시점 변경
            {
                viewChangeBool = !viewChangeBool;
            }
        }


        toolBool[0] = Input.GetKeyDown(tool1);
        toolBool[1] = Input.GetKeyDown(tool2);
        toolBool[2] = Input.GetKeyDown(tool3);
        toolBool[3] = Input.GetKeyDown(tool4);
        toolBool[4] = Input.GetKeyDown(tool5);
        toolBool[5] = Input.GetKeyDown(tool6);
        toolBool[6] = Input.GetKeyDown(tool7);
    }

    void Interaction()//상호작용
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;
        Debug.DrawRay(rayOrigin, transform.forward * rayDistance, Color.green);

        // Raycast 수행
        bool interactionHit = Physics.Raycast(transform.position, transform.forward, out hit, rayDistance, interactionLayerMask);

        // 상호작용 메시지 표시 여부 설정
        interactionMessage.SetActive(interactionHit);
        //빌드 중이 아닐때
        if (interactionHit && !build.isBuilding)
        {
            //레이에 닿은 오브젝트가 interactionLayerMask와 일치 해야함
            if (hit.transform.gameObject.CompareTag("Door"))
            {
                // 충돌한 객체의 태그 가져오기
                string hitTag = hit.transform.GetChild(0).gameObject.tag;
                if (hitTag == "Door" && interactionBool)
                {
                    doorOpenBool = true;
                    playerAnimator.SetAnimationLayer(3, 1);
                    playerAnimator.OpenDoorAnimation();
                    Door door = hit.transform.GetChild(0).GetComponent<Door>();
                    door.OpenDoor();
                }
            }
        }
        else
        {
            if (!doorOpenBool)
            {
                playerAnimator.SetAnimationLayer(3, 0);
            }
            if (interactionBool)
            {
                playerAnimator.PickUpItemAnimation();
            }
        }
    }
    private void Mover()
    {
        float moveFB = 0f;
        float moveRL = 0f;

        moveLRBool = false;
        moveFBBool = false;

        //앉아 있으면 애니메이션 레이어1번 으로 전환
        playerStateLayer = shitBool ? 1 : 0;
        //앉아 있으면 속도 -2감소 
        shitSlow = shitBool ? 2 : 0;

        if (jumpBool && isGround())
        {
            playerAnimator.JumpAnimation();
            rb.AddForce(Vector3.up * 25);
        }

        // W, S 키에 따른 이동 처리 (앞/뒤)
        if (forwardBool || backBool)
        {
            moveFBBool = true;
            moveFB = runBool ? (forwardBool ? runSpeed : -runSpeed) : (forwardBool ? walkSpeed : -walkSpeed);
        }

        // A, D 키에 따른 이동 처리 (좌/우)
        if (rightMoveBool || leftMoveBool)
        {
            moveLRBool = true;
            moveRL = runBool ? (rightMoveBool ? runSpeed : -runSpeed) : (rightMoveBool ? walkSpeed : -walkSpeed);
        }
        // 카메라의 방향을 기준으로 이동 방향 설정
        Vector3 forward = mainCamera.transform.forward;
        Vector3 right = mainCamera.transform.right;

        // Y축(상하) 이동 제거 (이동이 수평면에서만 작동하도록)
        forward.y = 0;
        right.y = 0;

        forward.Normalize();
        right.Normalize();

        // 최종 이동 방향 계산
        Vector3 moveDirection = (forward * moveFB + right * moveRL).normalized;

        // 이동 적용
        if (!StopToWall(moveDirection))
        {
            //moveSpeed = (runBool ? runSpeed - shitSlow : walkSpeed - shitSlow) + moveSpeedEnhance;
            float baseSpeed = runBool ? runSpeed : walkSpeed;

            // 퍼센트 단위로 강화 적용
            moveSpeed = (baseSpeed - shitSlow) * (1f + moveSpeedEnhance / 100f);
            transform.position += moveDirection * moveSpeed * Time.deltaTime;
        }


        //방향이 바뀔 때 즉시 0으로 설정
        if (moveFBBlend > 0 && moveFB < 0 || moveFBBlend < 0 && moveFB > 0)
        {
            moveFBBlend = 0; // 방향이 바뀔 때 즉시 0으로 설정
        }
        if (moveRLBlend > 0 && moveRL < 0 || moveRLBlend < 0 && moveRL > 0)
        {
            moveRLBlend = 0; // 방향이 바뀔 때 즉시 0으로 설정
        }
        // 애니메이터 블렌드 업데이트 - 걷기 -5 ,5 뛰기 -10,10
        moveFBBlend = Mathf.Clamp(moveFBBlend + moveFB * Time.deltaTime * 5f, runBool ? -runSpeed : -walkSpeed, runBool ? runSpeed : walkSpeed);
        moveRLBlend = Mathf.Clamp(moveRLBlend + moveRL * Time.deltaTime * 5f, runBool ? -runSpeed : -walkSpeed, runBool ? runSpeed : walkSpeed);

    }
    public float CurrentMoveSpeed
    {
        get
        {
            float baseSpeed = runBool ? runSpeed : walkSpeed;
            return (baseSpeed - shitSlow) * (1f + moveSpeedEnhance / 100f);
        }
    }
    public bool StopToWall(Vector3 moveDirection)
    {
        bool isBoder = Physics.Raycast(transform.position + new Vector3(0f, 0.5f, 0f), moveDirection, 1.5f, borderLayerMask);
        return isBoder;
    }
    //공격
    public void Attack()
    {
        //길게누르면 연속 공격 가능
        if (Input.GetMouseButton(0) && !attackBool)
        {
            playerAnimator.AttackAnimation();
        }
    }
    void SetToolActiveState()
    {
        // 모든 게임 오브젝트 비활성화
        foreach (var key in toolGameObjectDict.Keys)
        {
            foreach (var obj in toolGameObjectDict[key])
            {
                obj.SetActive(false);
            }
        }
        // 현재 toolState에 해당하는 게임 오브젝트만 활성화
        if (toolGameObjectDict.ContainsKey(toolState))
        {
            for (int i = 0; i < toolGameObjectDict[toolState].Length; i++)
            {
                bool isActive = (i == (int)attackBlend); // attackBlend와 일치하는 인덱스만 true
                toolGameObjectDict[toolState][i].SetActive(isActive); // 해당 인덱스만 활성화
            }
        }
    }
    bool isGround()
    {
        bool ground = Physics.Raycast(transform.position, Vector3.down, 0.1f);
        return ground;
    }
}
[System.Serializable]
public class ToolGameObject
{
    [Header("key : toolState(도구 종류)")]
    public int key;  // Dictionary의 Key 역할
    [Header("toolState에 맞는 도구 게임 오브젝트 ")]
    public GameObject[] toolGameObjects;  // Value 역할
}