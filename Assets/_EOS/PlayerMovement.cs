using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 이동 속도
    private Vector3 targetPosition; // 이동할 목표 지점
    
    // 이 변수가 마우스 이동 상태를 기억하는 핵심 변수입니다.
    private bool isMoving = false; 

    // 애니메이터를 미리 담아둘 변수
    Animator anim; 

    void Start() {
        // 시작할 때는 현재 내 위치를 목표로 설정
        targetPosition = transform.position;
        
        // [수정] 애니메이터는 게임 시작할 때 한 번만 찾아오는 게 성능에 좋습니다.
        anim = GetComponent<Animator>(); 
    }

    void Update() {
        // 1. 마우스 왼쪽 버튼 클릭 시 목표 지점 설정
        if (Input.GetMouseButtonDown(0)) {
            SetTargetPosition();
        }

        // 2. 목표 지점까지 이동
        if (isMoving) {
            MoveToTarget();
        }

        // [수정] 여기가 문제였습니다!
        // 기존에는 키보드 입력을 체크하고 있었는데, 
        // 이제는 도시남님이 만들어둔 'isMoving' 변수를 그대로 애니메이터에 전달합니다.
        if (anim != null)
        {
            anim.SetBool("isWalking", isMoving);
        }
    }

    void SetTargetPosition() {
        // 화면에서 마우스 위치로 빛(Ray)을 쏩니다.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 빛이 바닥(혹은 물체)에 맞았다면 그 좌표를 저장
        if (Physics.Raycast(ray, out hit)) {
            targetPosition = hit.point;
            targetPosition.y = transform.position.y; // 캡슐이 바닥에 파묻히지 않게 높이 유지
            
            isMoving = true; // 이동 시작! (애니메이션도 여기서 켜짐)
            
            Debug.Log($"<color=yellow>목표 설정: {targetPosition}</color>");
        }
    }

    void MoveToTarget() {
        // 1. 부드럽게 회전시키기 (기존 코드 유지)
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero) {
            // 바라볼 방향 계산
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // 현재 각도에서 목표 각도로 부드럽게 회전 (회전 속도 10f)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }

        // 2. 목표 위치로 이동 (기존 코드 유지)
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // 목표 지점에 거의 도착했다면 멈춤
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f) {
            isMoving = false; // 이동 끝! (애니메이션도 여기서 꺼짐)
        }
    }
}