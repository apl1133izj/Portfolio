using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    PlayerDemo player;
    public Animator animator;
    [SerializeField]
    [Header("장비에 맞는 상체 회전 값 조절 - Y : 좌우 , Z : 상하")]
    private List<AnimationClipArrays> animationClipList = new List<AnimationClipArrays>();
    private Dictionary<int, AnimationClip[]> animationClipDict = new Dictionary<int, AnimationClip[]>();
    [Header("상체를 카메라 방향에 맞춤")]
    public Transform spine; // 아바타의 상체

    [SerializeField]
    // 조정할 미세 회전 값 (값을 변경해서 테스트)
    private List<RotationOffsetYs> rotationOffsetYs = new List<RotationOffsetYs>();
    private Dictionary<int, float[]> rotationOffsetYDict = new Dictionary<int, float[]>();
    [SerializeField]
    private List<RotationOffsetZs> rotationOffsetZs = new List<RotationOffsetZs>();
    private Dictionary<int, float[]> rotationOffsetZDict = new Dictionary<int, float[]>();
    [SerializeField]
    private List<RotationOffsetSitZs> rotationOffsetSitZs = new List<RotationOffsetSitZs>();
    private Dictionary<int, float[]> rotationOffsetSitZDict = new Dictionary<int, float[]>();

    void Start()
    {
        player = GetComponent<PlayerDemo>();
        animator = GetComponent<Animator>();
        PlayerAnimationSatting();
    }
    private void LateUpdate()
    {
        SpineRotation();
    }
    public void SpineRotation()
    {
        //공격 모션 중일때만 상체 회전 값 변경
        if (player.attackBool || player.reLoad)
        {
            //상체를 카메라 방향에 맞춤
            Vector3 lookDirection = player.mainCamera.transform.forward;
            lookDirection.y = 0;

            Quaternion lookRotation = Quaternion.LookRotation(lookDirection);
            lookRotation = Quaternion.Euler(spine.eulerAngles.x,
                                           lookRotation.eulerAngles.y + rotationOffsetYDict[player.toolState][(int)player.attackBlend],
                                           spine.eulerAngles.z +
                                           (player.shitBool ? rotationOffsetSitZDict[player.toolState][(int)player.attackBlend] : rotationOffsetZDict[player.toolState][(int)player.attackBlend]));
            spine.rotation = lookRotation;
        }
    }
    public void PlayerAnimationSatting()
    {
        foreach (var item in animationClipList)
        {
            animationClipDict[item.key] = item.clips;
        }
        foreach (var item in rotationOffsetYs)
        {
            rotationOffsetYDict[item.key] = item.rotationOffsetY;
        }
        foreach (var item in rotationOffsetZs)
        {
            rotationOffsetZDict[item.key] = item.rotationOffsetZ;
        }
        foreach (var item in rotationOffsetSitZs)
        {
            rotationOffsetSitZDict[item.key] = item.rotationOffsetSiztZ;
        }
        spine = animator.GetBoneTransform(HumanBodyBones.Spine);//상체Transform 가져오기
    }
    // Update is called once per frame
    void Update()
    {
        MoveAnimator();
        ToolAnimator();
    }
    //움직임 애니메이션
    private void MoveAnimator()
    {
        // 모든 레이어 초기화 후, 필요한 레이어만 활성화
        animator.SetLayerWeight(0, player.playerStateLayer == 0 ? 1 : 0);//일어난 상태
        animator.SetLayerWeight(1, player.playerStateLayer == 1 ? 1 : 0);//앉아 있는 상태

        animator.SetBool("FBMove Bool", player.moveFBBool);
        animator.SetBool("LRMove Bool", player.moveLRBool);


        //도구 가 없는 경우 오,왼 이동 애니메이션 0 보다 큰값은 값은 도구 가 있는 상태
        animator.SetFloat("RLStateBlend Float", player.toolState);
        animator.SetFloat("MoveFBBlend", player.moveFBBlend);

        //도구 가 없는 경우 앞,뒤 이동 애니메이션 0 보다 큰값은 값은 도구 가 있는 상태
        animator.SetFloat("FBStateBlend Float", player.toolState);
        animator.SetFloat("MoveRLBlend", player.moveRLBlend);


        //도구 마다 공격 애니메이션
        animator.SetFloat("AttackTool Float", player.toolState);
    }
    //도구 를 활용한 공격 밎 작업
    public void ToolAnimator()
    {
        //attackBlend 공격 모션이 추가 될경우 사용 기본값인 0으로 초기화중
        animator.SetBool("AttackBool", player.attackBool);
        animator.SetFloat("AttackBlend", player.attackBlend);
        animator.SetFloat("AttackTool Float", player.toolState);
    }

    public void JumpAnimation()
    {
        animator.SetTrigger("JumpTrigger");
    }
    public void OpenDoorAnimation()
    {
        animator.SetTrigger("DoorOpen Trigger");
    }

    public void PickUpItemAnimation()
    {
        animator.SetTrigger("PickUpItemTrigger");
    }
    public void SetAnimationLayer(int layer, int state)
    {
        animator.SetLayerWeight(layer, state);
    }
    public void AttackAnimation()
    {
        StartCoroutine(CheckToolAnimationState());
    }
    IEnumerator CheckToolAnimationState()
    {
        player.bulletCount = 0;
        Debug.Log("시작");
        //도구 변경 애니메이션 작성
        //추가 :  playerAnimator.SetTrigger("ToolChangeTrigger"); - 도구 변경 애니메이션
        //도구 변경 애니메이션 추가시 변경 : yield return new WaitUntil(()=> toolChange)
        while (true)
        {
            SetAnimationLayer(2, 1);
            //장전 중일경우 
            //playerAnimator.SetTrigger("ReLoadTrigger");
            //장전 애니메이션 추가시 변경 : yield return new WaitUntil(()=> reLoad)
            player.attackBool = true;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(2);
            float normalizedTime = stateInfo.normalizedTime % 1; // 진행 비율 (0~1)
            //Debug.Log("애니메이션 이름 : " + animationClipDict[toolState][(int)attackBlend]);
            float frameRate = animationClipDict[player.toolState][(int)player.attackBlend].frameRate; //예시: 0번 키의 2번째 클립의 FPS
            float animationLength = animationClipDict[player.toolState][(int)player.attackBlend].length; //예시: 0번 키의 2번째 클립 길이
            int totalFrames = Mathf.FloorToInt(frameRate * animationLength); // 전체 프레임 수
            // 진행된 프레임 수 (현재 normalizedTime을 프레임 수로 변환)
            int currentFrame = Mathf.FloorToInt(normalizedTime * totalFrames);
            player.bulletCount++;
            // 애니메이션이 끝났는지 확인
            if (!Input.GetMouseButton(0))
            {
                if (currentFrame >= totalFrames - 1)
                {
                    SetAnimationLayer(2, 0);
                    player.attackBool = false;
                    player.bulletCount = 0;
                    yield break;
                }
            }
            yield return null;
        }
    }
    /*애니메이션 클립*/
    public void BulletInst()
    {
        if (player.bulletCount >= player.bullsetSpawDelay[(int)player.attackBlend])
        {
            /* 탄창 - 재장전 애니메이션이 생겼을 경우
            if (bullet[(int)attackBlend] < magazine[bullet[(int)attackBlend]])
            {
                bullet[(int)attackBlend]++;
            }
            else
            {
                reLoad = true;
                attackBool = false;
            }
            */
            GameObject bulletGameObject = Instantiate(player.bulletPrefab[0], player.bulletInstPos[(int)player.attackBlend].position, Quaternion.identity);
            Destroy(bulletGameObject, 20f);
        }

    }
    public void PickUpItemEvent()
    {
        SetAnimationLayer(2, 0);
    }
    public void DoorOpen()
    {
        player.doorOpenBool = false;
    }
}
[System.Serializable]
public class AnimationClipArrays
{
    [Header("key : toolState(도구 종류)")]
    public int key;  // Dictionary의 Key 역할
    [Header("clips : AnimationClip(도구 에 맞는 애니메이션 클립)")]
    public AnimationClip[] clips;  // Value 역할
}
[System.Serializable]
public class RotationOffsetYs
{
    [Header("key : toolState(도구 종류)")]
    public int key;  // Dictionary의 Key 역할
    [Header("rotationOffsetY : 상체 : 좌우 값 조절 ")]
    public float[] rotationOffsetY;  // Value 역할
}
[System.Serializable]
public class RotationOffsetZs
{
    [Header("key : toolState(도구 종류)")]
    public int key;  // Dictionary의 Key 역할
    [Header("rotationOffsetZ : 상체 : 상하 값 조절 ")]
    public float[] rotationOffsetZ;  // Value 역할
}
[System.Serializable]
public class RotationOffsetSitZs
{
    [Header("key : toolState(도구 종류)")]
    public int key;  // Dictionary의 Key 역할
    [Header("rotationOffsetZ : 상체 : 앉아 있을경우 상하 값 조절 ")]
    public float[] rotationOffsetSiztZ;  // Value 역할
}
