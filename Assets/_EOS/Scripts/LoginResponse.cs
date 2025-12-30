using System;

/// <summary>
/// 서버에서 받은 로그인 응답 JSON을 담는 그릇입니다.
/// 서버의 LoginResponse와 UserDto 구조에 맞춰 정의되었습니다.
/// </summary>
[Serializable]
public class LoginResponse
{
    public string type;       // "LOGIN_RESPONSE"
    public bool success;      // true / false
    public string message;    // "로그인 성공" 또는 에러 메시지
    public UserData userData; // 상세 유저 데이터 (성공 시)
}

/// <summary>
/// 사용자 정보 데이터 클래스
/// 서버의 UserDto 구조에 맞춰 정의되었습니다.
/// password는 보안상 포함되지 않습니다.
/// </summary>
[Serializable]
public class UserData
{
    public long userNo;       // 사용자 일련번호
    public string userId;     // 사용자 아이디 (로그인 ID)
    public string nickname;   // 게임 내 닉네임
    public long gold;         // 보유 골드
    public float lastX;       // 마지막 접속 X 좌표
    public float lastY;       // 마지막 접속 Y 좌표
}