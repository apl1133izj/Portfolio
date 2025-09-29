using UnityEngine;

public class CameraMove : MonoBehaviour
{
    PlayerDemo playerDemo;
    public GameObject player;
    public float y;
    public Transform[] cameraRig;
    public float rotationSpeed = 100f; // 회전 속도
    public float targetRotationY;
    public float targetRotationX;
    public GameObject[] viewCamera;//0:3인칭 - 기본 ,1: 1인칭 - V키
    public LayerMask wellCheckLayerMask;

    public float clampXMax;
    public float clampXMin;

    public float height;
    public float wight;
    private void Start()
    {
        playerDemo = GetComponentInParent<PlayerDemo>();
        cameraRig[0].transform.rotation = Quaternion.identity;
    }
    void Update()
    {
        if (Build.instance.buildUI.activeSelf || EnhanceManager.instance.statuseWindowisActive) return;
        bool viewChange = playerDemo.viewChangeBool;
        int viewCount = viewChange ? 1 : 0;
        rotationSpeed = DataManager.GetDataManager.sensitivity;
        transform.parent = cameraRig[viewCount];
        ViewCamera(viewCount, viewChange);
        CameraRotate(viewCount, viewChange);   
        WellCheck(viewChange);      
    }
    void WellCheck(bool viewChange)
    {
        if (viewChange) return;
        Vector3 rayDir = transform.position - player.transform.position;
        if (Physics.Raycast(player.transform.position, rayDir, out RaycastHit hit, float.MaxValue, wellCheckLayerMask))
        {
            transform.position = hit.point - rayDir.normalized + new Vector3(0, 0.6f, 0);
        }
    }
    void CameraRotate(int viewCount ,bool viewChange)
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // 좌우 회전 → 플레이어 자체가 회전
        targetRotationY += mouseX * rotationSpeed * Time.deltaTime;
        player.transform.rotation = Quaternion.Euler(0, targetRotationY, 0);

        // 상하 회전 → 카메라Rig만 회전 (X축만)
        targetRotationX -= mouseY * rotationSpeed * Time.deltaTime;
        targetRotationX = Mathf.Clamp(targetRotationX, clampXMin, clampXMax);
        cameraRig[viewCount].localRotation = Quaternion.Euler(targetRotationX, 0, 0);
        if (viewChange) return;
        // 카메라 위치 계산
        Vector3 rigPosition = player.transform.position + Vector3.up * height;
        Vector3 offset = cameraRig[viewCount].rotation * new Vector3(0, 0, -wight);
        transform.position = rigPosition + offset;

        // 플레이어 쪽 바라보기
        transform.LookAt(rigPosition);

    }

    private void ViewCamera(int view , bool viewChange )
    {
        // transform.localPosition = Vector3.zero;
         //gameObject.transform.position = viewCamera[view].transform.position;
        transform.localPosition = viewCamera[view].transform.localPosition + (viewChange ? new Vector3(0, -1.5f, 0) : new Vector3(0, height, wight));
        transform.localRotation = viewCamera[view].transform.localRotation;



    }
}
