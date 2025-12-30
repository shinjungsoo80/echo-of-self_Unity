using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 캐릭터 머리 위에 닉네임을 표시하는 스크립트입니다.
/// World Space Canvas를 사용하여 3D 공간에 UI를 표시합니다.
/// </summary>
public class PlayerNameDisplay : MonoBehaviour
{
    [Header("UI 연결")]
    [Tooltip("인스펙터에서 TextMeshProUGUI 컴포넌트를 드래그해서 연결해주세요.")]
    public TextMeshProUGUI nameText;
    
    [Header("닉네임 표시 설정")]
    [Tooltip("캐릭터 머리 위에 표시될 높이 (Y 오프셋)")]
    public float nameOffsetY = 2.5f; // 캐릭터 머리 위 높이
    
    private Canvas nameCanvas; // nameText의 부모 Canvas (자동으로 찾음)
    private Camera mainCamera;
    
    void Start()
    {
        // 메인 카메라 참조 가져오기
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
        
        // nameText가 인스펙터에서 연결되었는지 확인
        if (nameText == null)
        {
            Debug.LogError("<color=red>PlayerNameDisplay: nameText가 인스펙터에서 연결되지 않았습니다! TextMeshProUGUI를 연결해주세요.</color>");
            return;
        }
        
        // nameText의 부모에서 Canvas 찾기
        nameCanvas = nameText.GetComponentInParent<Canvas>();
        if (nameCanvas == null)
        {
            Debug.LogWarning("<color=yellow>PlayerNameDisplay: nameText의 부모에 Canvas를 찾을 수 없습니다. Billboard 효과가 작동하지 않을 수 있습니다.</color>");
        }
        
        Debug.Log($"<color=green>PlayerNameDisplay 초기화 완료. nameText: {nameText != null}, Canvas: {nameCanvas != null}</color>");
    }
    
    void LateUpdate()
    {
        // Canvas가 없으면 Billboard 효과 스킵
        if (nameCanvas == null || mainCamera == null)
        {
            return;
        }
        
        // 카메라를 항상 바라보도록 회전 (Billboard 효과)
        nameCanvas.transform.LookAt(nameCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                                   mainCamera.transform.rotation * Vector3.up);
        
        // 캐릭터 머리 위에 항상 위치하도록 업데이트
        Vector3 namePosition = transform.position + Vector3.up * nameOffsetY;
        nameCanvas.transform.position = namePosition;
    }
    
    
    /// <summary>
    /// 닉네임을 설정합니다. NetworkManager에서 호출합니다.
    /// </summary>
    /// <param name="nickname">표시할 닉네임</param>
    public void SetNickname(string nickname)
    {
        Debug.Log($"<color=magenta>[디버깅] SetNickname 호출됨. 전달받은 nickname: '{nickname}'</color>");
        Debug.Log($"<color=magenta>[디버깅] nameText가 null인가? {nameText == null}</color>");
        
        if (nameText != null)
        {
            string oldText = nameText.text;
            nameText.text = nickname;
            Debug.Log($"<color=cyan>닉네임 설정 완료: '{oldText}' -> '{nickname}'</color>");
            Debug.Log($"<color=magenta>[디버깅] nameText.text 현재 값: '{nameText.text}'</color>");
        }
        else
        {
            Debug.LogWarning($"<color=yellow>nameText가 아직 초기화되지 않았습니다. nickname='{nickname}' 저장 후 나중에 적용합니다.</color>");
            
            // nameText가 아직 초기화되지 않았으면, Start()가 완료된 후 적용하기 위해 저장
            StartCoroutine(SetNicknameWhenReady(nickname));
        }
    }

    /// <summary>
    /// nameText가 아직 null일 때, 연결될 때까지 기다렸다가 닉네임을 적용하는 코루틴입니다.
    /// </summary>
    private System.Collections.IEnumerator SetNicknameWhenReady(string nickname)
    {
        // nameText가 연결될 때까지(null이 아닐 때까지) 매 프레임 대기
        // (무한 루프 방지를 위해 최대 2초 정도만 기다리게 안전장치를 둘 수도 있지만, 일단 심플하게 갑니다)
        while (nameText == null)
        {
            yield return null; // 다음 프레임까지 대기
        }

        // 드디어 nameText가 연결됨!
        string oldText = nameText.text;
        nameText.text = nickname;
        
        // 확실하게 텍스트 갱신
        nameText.ForceMeshUpdate();

        Debug.Log($"<color=cyan>[지연 적용 성공] 드디어 nameText가 연결되어 닉네임을 설정했습니다: '{oldText}' -> '{nickname}'</color>");
    }
    
    
    /// <summary>
    /// 닉네임 오프셋을 동적으로 변경합니다.
    /// </summary>
    /// <param name="offsetY">Y 오프셋 값</param>
    public void SetNameOffset(float offsetY)
    {
        nameOffsetY = offsetY;
    }
    
}

