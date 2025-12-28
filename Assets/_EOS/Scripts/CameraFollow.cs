using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    // [중요] 변수 이름을 'Target'으로 확실히 정해줍니다.
    public Transform Target; 
    public Vector3 Offset = new Vector3(0, 8, -8); 
    public float SmoothSpeed = 1f; // 1이면 즉시 따라붙습니다.

    void LateUpdate()
    {
        // 1. 만약 따라갈 대상(Target)이 없다면 직접 찾습니다.
        if (Target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Target = player.transform;
                // 찾자마자 카메라를 그 위치로 순간이동!
                transform.position = Target.position + Offset;
            }
            return; // 대상을 찾을 때까지는 아래 코드를 실행하지 않습니다.
        }

        // 2. 대상이 있다면 무조건 따라갑니다.
        Vector3 desiredPosition = Target.position + Offset;
        
        // Lerp를 쓰지 않고 즉시 위치를 맞춰버리는 가장 확실한 방법입니다.
        transform.position = desiredPosition;

        // 3. 카메라가 항상 캐릭터를 쳐다보게 합니다.
        transform.LookAt(Target);
    }
}