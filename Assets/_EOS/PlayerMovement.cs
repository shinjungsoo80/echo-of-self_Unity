using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // =========================================================
    // [변수 설정 영역]
    // =========================================================
    public float moveSpeed = 5f; // 이동 속도
    
    // [추가] 회전할 때 너무 휙휙 돌지 않게 부드러움을 조절하는 변수입니다.
    public float rotateSpeed = 10f; 

    private Vector3 targetPosition; // 이동할 목표 지점
    
    // 이 변수가 마우스 이동 상태를 기억하는 핵심 변수입니다.
    private bool isMoving = false; 

    // 애니메이터를 미리 담아둘 변수
    Animator anim; 

    // =========================================================
    // [초기화 영역]
    // =========================================================
    void Start() {
        // 시작할 때는 현재 내 위치를 목표로 설정 (시작하자마자 튀는 것 방지)
        targetPosition = transform.position;
        
        // [수정] 애니메이터는 게임 시작할 때 한 번만 찾아오는 게 성능에 좋습니다.
        anim = GetComponent<Animator>(); 
    }

    // =========================================================
    // [실시간 업데이트 영역]
    // =========================================================
    void Update() {
        // 1. 마우스 왼쪽 버튼 클릭 시 목표 지점 설정
        if (Input.GetMouseButtonDown(0)) {
            SetTargetPosition();
        }

        // 2. 목표 지점까지 이동 (isMoving이 true일 때만 실행)
        if (isMoving) {
            MoveToTarget();
        }

        // [애니메이션 처리]
        // 도시남님이 만들어둔 'isMoving' 변수 상태를 애니메이터에 전달합니다.
        // (걷고 있으면 true, 멈추면 false를 전달해서 애니메이션을 전환)
        if (anim != null)
        {
            // 주의: 애니메이터 파라미터 이름("isWalking")이 대소문자까지 정확해야 합니다!
            anim.SetBool("isWalking", isMoving);
        }
    }

    // =========================================================
    // [기능 함수 영역]
    // =========================================================
    void SetTargetPosition() {
        // 화면에서 마우스 위치로 빛(Ray)을 쏩니다.
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // 빛이 바닥(혹은 물체)에 맞았다면 그 좌표를 저장
        if (Physics.Raycast(ray, out hit)) {
            
            // [중요 수정] 꿈틀거림 방지 핵심 1: 높이 고정
            // 클릭한 바닥의 높이(y)가 아니라, 내 캐릭터의 현재 키(y)를 유지해야 
            // 캐릭터가 땅으로 파고들거나 위로 솟구치지 않습니다.
            targetPosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
            
            isMoving = true; // 이동 시작! (애니메이션도 여기서 켜짐)
            
            Debug.Log($"<color=yellow>목표 설정 완료: {targetPosition}</color>");
        }
    }

    void MoveToTarget() {
        // [수정] 방향 벡터가 0이 아닐 때만 계산 (에러 방지)
        Vector3 direction = (targetPosition - transform.position).normalized;

        // 1. 부드럽게 회전시키기
        // 목표 방향이 명확할 때만 회전합니다. (제자리에서 도는 현상 방지)
        if (direction != Vector3.zero) {
            // 바라볼 방향 계산
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // 현재 각도에서 목표 각도로 부드럽게 회전 (Slerp 사용)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
        }

        // 2. 목표 위치로 이동 (기존 코드 유지)
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // [중요 수정] 꿈틀거림 방지 핵심 2: 도착 판정 강화
        // 목표 지점에 '거의' 도착했다면(0.1f 이내), 강제로 멈춰 세웁니다.
        // 이걸 안 하면 목표 지점 주변에서 미세하게 계속 움직이려고 해서 캐릭터가 떱니다.
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f) {
            transform.position = targetPosition; // 위치를 목표점에 딱 고정!
            isMoving = false; // 이동 끝! (애니메이션도 여기서 꺼짐)
        }
    }
}