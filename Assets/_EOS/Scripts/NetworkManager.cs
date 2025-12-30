using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using TMPro;
using UnityEngine.Networking;

public class NetworkManager : MonoBehaviour {
    private TcpClient client;
    private const string IP = "127.0.0.1"; 
    private const int PORT = 9000;
    
    [Header("서버 설정")]
    [Tooltip("HTTP 서버 URL (예: http://127.0.0.1:8080)")]
    public string serverUrl = "http://127.0.0.1:8080";

    public TMP_InputField idInput;
    public GameObject playerPrefab; 
    public GameObject loginUI; 
    public GameObject logoutButton;

    // 로그인 성공 정보를 담아둘 변수들
    private bool isLoginSuccess = false;
    private float lastX, lastY;
    private string playerNickname; // 로그인 시 받아온 닉네임 저장

    // 인스펙터 창에서 드래그해서 넣어줄 변수들
    // public GameObject loginBGM; // BGM_Player
    // public GameObject gameBGM;  // BGM_Game

    [Header("Audio Settings")]
    public AudioSource bgmSpeaker;      // 음악을 틀어줄 스피커 (AudioSource)
    public AudioClip loginMusicClip;    // 로그인 화면 BGM 파일
    public AudioClip gameMusicClip;     // (옵션) 게임 접속했을 때 나올 BGM

    void Start() {
        ConnectToServer();
        PlayLoginMusic(); // 게임 시작하면 바로 로그인 음악 재생
    }

    void ConnectToServer() {
        try {
            client = new TcpClient(IP, PORT);
            Debug.Log("<color=green><b>서버 연결 성공!</b></color>");
        } catch (Exception e) {
            Debug.LogError("연결 실패: " + e.Message);
        }
    }

    void Update() {
        // 1. 서버로부터 데이터 수신 확인
        if (client != null && client.Connected && client.GetStream().DataAvailable) {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            
            if (bytesRead > 0) {
                string responseJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                HandleMessage(responseJson);
            }
        }

        // 2. 로그인 성공 신호가 오면 '메인 스레드'인 여기서 캐릭터를 소환합니다.
        if (isLoginSuccess) {
            SpawnPlayer(lastX, lastY);
            if (loginUI != null) loginUI.SetActive(false);
            isLoginSuccess = false; // 소환 후 신호 초기화
            // 여기서 음악을 바꿔줍니다!
          //  if(loginBGM != null) loginBGM.SetActive(false); // 로그인 음악 끄기
          //  if(gameBGM != null) gameBGM.SetActive(true);    // 게임 음악 켜기
        }
    }

    public void OnLoginButtonClicked() {
        string userId = idInput.text;
        if (string.IsNullOrEmpty(userId)) {
            Debug.LogWarning("<color=yellow>사용자 ID를 입력해주세요.</color>");
            return;
        }

        // UnityWebRequest를 사용하여 로그인 요청
        StartCoroutine(LoginRequest(userId));
    }
    
    /// <summary>
    /// UnityWebRequest를 사용하여 서버에 로그인 요청을 보냅니다.
    /// </summary>
    /// <param name="userId">로그인할 사용자 ID</param>
    private IEnumerator LoginRequest(string userId) {
        // 로그인 요청 JSON 생성
        string jsonBody = "{\"type\":\"LOGIN\", \"userId\":\"" + userId + "\"}";
        
        // UnityWebRequest 생성
        using (UnityWebRequest request = new UnityWebRequest(serverUrl + "/api/login", "POST")) {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            Debug.Log($"<color=cyan>로그인 요청 전송: {userId}</color>");
            
            // 요청 전송 및 응답 대기
            yield return request.SendWebRequest();
            
            // 응답 처리
            if (request.result == UnityWebRequest.Result.Success) {
                string responseJson = request.downloadHandler.text;
                Debug.Log($"<color=green>서버 응답 수신: {responseJson}</color>");
                HandleLoginResponse(responseJson);
            } else {
                Debug.LogError($"<color=red>로그인 요청 실패: {request.error}</color>");
                
                // HTTP 에러인 경우에도 응답 본문 확인 시도
                if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text)) {
                    string errorResponse = request.downloadHandler.text;
                    Debug.LogWarning($"<color=yellow>에러 응답: {errorResponse}</color>");
                    HandleLoginResponse(errorResponse); // 에러 응답도 파싱 시도
                }
            }
        }
    }

    public void OnLogoutButtonClicked() {
        GameObject player = GameObject.Find("MyPlayer");
        if (player != null) Destroy(player);

        if (loginUI != null) loginUI.SetActive(true);
        if (logoutButton != null) logoutButton.SetActive(false);
        if (idInput != null) idInput.text = "";

        PlayLoginMusic(); // [추가] 이제 로그인 음악으로 전환!

        Debug.Log("<color=orange>로그아웃 성공: 메인 화면으로 돌아갑니다.</color>");
    }

    /// <summary>
    /// UnityWebRequest로 받은 로그인 응답을 처리합니다.
    /// JsonUtility.FromJson을 사용하여 LoginResponse로 파싱합니다.
    /// </summary>
    /// <param name="json">서버에서 받은 JSON 응답</param>
    private void HandleLoginResponse(string json) {
        try {
            // JsonUtility.FromJson을 사용하여 LoginResponse로 파싱
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(json);
            
            if (response == null) {
                Debug.LogError("<color=red>응답 파싱 실패: response가 null입니다.</color>");
                return;
            }
            
            if (response.success) {
                // 데이터를 변수에 담고 성공 신호를 켭니다.
                if (response.userData != null) {
                    lastX = response.userData.lastX;
                    lastY = response.userData.lastY;
                    
                    // 서버에서 받은 nickname 확인 및 저장
                    string receivedNickname = response.userData.nickname;
                    Debug.Log($"<color=magenta>[디버깅] 서버에서 받은 원본 JSON: {json}</color>");
                    Debug.Log($"<color=magenta>[디버깅] 파싱된 response.userData.nickname 값: '{receivedNickname}'</color>");
                    Debug.Log($"<color=magenta>[디버깅] nickname이 null인가? {receivedNickname == null}</color>");
                    Debug.Log($"<color=magenta>[디버깅] nickname이 비어있는가? {string.IsNullOrEmpty(receivedNickname)}</color>");
                    
                    playerNickname = receivedNickname; // DB에서 받아온 닉네임 저장
                    
                    Debug.Log($"<color=green>로그인 성공! 저장된 playerNickname: '{playerNickname}', 골드: {response.userData.gold}, 위치: ({lastX}, {lastY})</color>");
                } else {
                    Debug.LogWarning("<color=yellow>로그인 성공했지만 userData가 null입니다.</color>");
                }
                
                isLoginSuccess = true; // Update에서 캐릭터 소환
                
                // 로그아웃 버튼 활성화
                if (logoutButton != null) {
                    logoutButton.SetActive(true); 
                    Debug.Log("<color=cyan>로그아웃 버튼 활성화 완료!</color>");
                }

                PlayGameMusic(); // 게임 음악으로 전환
                Debug.Log($"<color=green>{playerNickname}님 환영합니다!</color>");
            } else {
                Debug.LogWarning($"<color=red>[로그인 실패]</color> {response.message}");
            }
        } catch (Exception e) {
            Debug.LogError($"<color=red>메시지 해석 오류: {e.Message}</color>\n응답 JSON: {json}");
        }
    }
    
    /// <summary>
    /// TcpClient를 통한 메시지 처리 (기존 방식, 필요시 사용)
    /// </summary>
    private void HandleMessage(string json) {
        HandleLoginResponse(json); // 동일한 로직 사용
    }

    void SpawnPlayer(float x, float y) {
        Vector3 spawnPos = new Vector3(x, 0.5f, y); 
        GameObject go = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        go.name = "MyPlayer";
        
        Debug.Log($"<color=magenta>[디버깅] SpawnPlayer 호출됨. 전달할 playerNickname: '{playerNickname}'</color>");
        
        // PlayerNameDisplay 컴포넌트에 닉네임 설정
        PlayerNameDisplay nameDisplay = go.GetComponent<PlayerNameDisplay>();
        if (nameDisplay != null) {
            Debug.Log($"<color=magenta>[디버깅] PlayerNameDisplay 컴포넌트 찾음. SetNickname('{playerNickname}') 호출 예정</color>");
            
            // nameText가 초기화될 때까지 대기하기 위해 코루틴 사용
            StartCoroutine(SetNicknameAfterInit(nameDisplay, playerNickname));
        } else {
            Debug.LogWarning("<color=yellow>PlayerNameDisplay 컴포넌트를 찾을 수 없습니다. Player.prefab에 추가해주세요.</color>");
        }
        
        Debug.Log("<color=cyan>캐릭터 소환 완료!</color>");
    }
    
    /// <summary>
    /// PlayerNameDisplay의 nameText가 초기화된 후에 닉네임을 설정합니다.
    /// </summary>
    private IEnumerator SetNicknameAfterInit(PlayerNameDisplay nameDisplay, string nickname) {
        // 최대 1초 동안 nameText 초기화 대기
        float waitTime = 0f;
        const float maxWaitTime = 1f;
        
        while (waitTime < maxWaitTime) {
            // SetNickname 내부에서 nameText null 체크를 하므로 바로 호출해도 되지만,
            // 확실하게 하기 위해 약간의 대기 시간을 둡니다.
            yield return new WaitForSeconds(0.1f);
            waitTime += 0.1f;
            
            // nameDisplay가 여전히 유효한지 확인
            if (nameDisplay == null) {
                Debug.LogWarning("<color=yellow>PlayerNameDisplay가 null이 되었습니다.</color>");
                yield break;
            }
        }
        
        Debug.Log($"<color=magenta>[디버깅] SetNickname 호출: '{nickname}'</color>");
        nameDisplay.SetNickname(nickname);
        Debug.Log($"<color=cyan>닉네임 적용 완료: '{nickname}'</color>");
    }

    // [새로 추가하는 함수] 음악 교체 담당
    void PlayLoginMusic()
    {
        if (bgmSpeaker != null && loginMusicClip != null)
        {
            // 만약 이미 로그인 음악이 나오고 있다면 굳이 처음부터 다시 틀지 않음 (선택사항)
            if (bgmSpeaker.clip == loginMusicClip && bgmSpeaker.isPlaying) return;

            bgmSpeaker.clip = loginMusicClip; // CD 갈아끼우기
            bgmSpeaker.Play(); // 재생!
        }
    }

    // 1. 기존 변수 밑에 이 함수를 추가하세요.
    public void PlayGameMusic()
    {
        if (bgmSpeaker != null && gameMusicClip != null)
        {
            // 이미 게임 음악이 나오고 있으면 통과
            if (bgmSpeaker.clip == gameMusicClip && bgmSpeaker.isPlaying) return;

            bgmSpeaker.clip = gameMusicClip; // 게임 음악 CD로 교체
            bgmSpeaker.Play(); 
        }
    }

}