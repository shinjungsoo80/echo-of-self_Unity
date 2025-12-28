using System;

// 서버의 JSON 데이터를 담는 그릇입니다.
[Serializable]
public class LoginResponse
{
    public string type;       // "LOGIN_RESPONSE"
    public bool success;      // true / false
    public string message;    // "로그인 성공"
    public UserData userData; // 상세 유저 데이터
}

[Serializable]
public class UserData
{
    public long userNo;
    public string userId;
    public string nickname;
    public long gold;
    public float lastX;
    public float lastY;
}