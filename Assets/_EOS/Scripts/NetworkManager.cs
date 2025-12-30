using UnityEngine;
using System;
using System.Net.Sockets;
using System.Text;
using TMPro;

public class NetworkManager : MonoBehaviour {
    private TcpClient client;
    private const string IP = "127.0.0.1"; 
    private const int PORT = 9000;

    public TMP_InputField idInput;
    public GameObject playerPrefab; 
    public GameObject loginUI; 
    public GameObject logoutButton;

    // 로그인 성공 정보를 담아둘 변수들
    private bool isLoginSuccess = false;
    private float lastX, lastY;

    // 인스펙터 창에서 드래그해서 넣어줄 변수들
public GameObject loginBGM; // BGM_Player
public GameObject gameBGM;  // BGM_Game

    void Start() {
        ConnectToServer();
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
            if(loginBGM != null) loginBGM.SetActive(false); // 로그인 음악 끄기
            if(gameBGM != null) gameBGM.SetActive(true);    // 게임 음악 켜기
        }
    }

    public void OnLoginButtonClicked() {
        if (client != null && client.Connected) {
            string userId = idInput.text;
            if (string.IsNullOrEmpty(userId)) return;

            NetworkStream stream = client.GetStream();
            string jsonMsg = "{\"type\":\"LOGIN\", \"userId\":\"" + userId + "\"}";
            byte[] data = Encoding.UTF8.GetBytes(jsonMsg + "\n");
            stream.Write(data, 0, data.Length);
        }
    }

    public void OnLogoutButtonClicked() {
        GameObject player = GameObject.Find("MyPlayer");
        if (player != null) Destroy(player);

        if (loginUI != null) loginUI.SetActive(true);
        if (logoutButton != null) logoutButton.SetActive(false);
        if (idInput != null) idInput.text = "";

        Debug.Log("<color=orange>로그아웃 성공: 메인 화면으로 돌아갑니다.</color>");
    }

    private void HandleMessage(string json) {
        try {
            LoginResponse response = JsonUtility.FromJson<LoginResponse>(json);
            if (response.success) {
                // 데이터를 변수에 담고 성공 신호를 켭니다.
                lastX = response.userData.lastX;
                lastY = response.userData.lastY;
                isLoginSuccess = true; //Update에서 캐릭터 소환
                // 👈 [추가] 여기서 즉시 로그아웃 버튼을 활성화합니다.
                if (logoutButton != null) {
                    logoutButton.SetActive(true); 
                    Debug.Log("<color=cyan>로그아웃 버튼 활성화 완료!</color>");
                }
                Debug.Log($"<color=green>{response.userData.nickname}님 환영합니다!</color>");
            } else {
                Debug.LogWarning($"<color=red>[로그인 실패]</color> {response.message}");
            }
        } catch (Exception e) {
            Debug.LogError("메시지 해석 오류: " + e.Message);
        }
    }

    void SpawnPlayer(float x, float y) {
        Vector3 spawnPos = new Vector3(x, 0.5f, y); 
        GameObject go = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        go.name = "MyPlayer";
        Debug.Log("<color=cyan>캐릭터 소환 완료!</color>");
    }
}