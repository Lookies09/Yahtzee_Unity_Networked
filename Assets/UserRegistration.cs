using UnityEngine;
using UnityEngine.UI;  // UI 관련 클래스 포함
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using UnityEngine.SceneManagement;

public class UserRegistration : MonoBehaviour
{
    [System.Serializable]
    public class User
    {
        public string name;
        public string pw;
    }

    public class UserDTO
    {
        public int id;
        public string name;
        public int bestScore;
    }

    [Header("로그인 UI")]
    public GameObject signinPage;
    public InputField usernameInput_Signin;
    public InputField passwordInput_Signin;
    public Button SigninButton;
    public Button toSignupButton;

    // UI InputField
    [Header("회원가입 UI")]
    public GameObject signupPage;
    public InputField usernameInput_Signup;
    public InputField passwordInput_Signup;
    public Button signupButton;
    public Button toSigninButton;    

    // 서버 URL
    private string serverUrl = "http://localhost:8080/unity/signup";

    private string serverUrl2 = "http://localhost:8080/unity/signin";

    private void Start()
    {
        // 버튼 클릭 시 RegisterUser 메서드 호출
        signupButton.onClick.AddListener(OnRegisterButtonClicked);
        SigninButton.onClick.AddListener(OnSigninButtonClick);

        toSigninButton.onClick.AddListener(OnToSigninBtnClick);
        toSignupButton.onClick.AddListener(OnToSignUpBtnClick);
    }

    // 버튼 클릭 시 호출되는 메서드
    private void OnRegisterButtonClicked()
    {
        string name = usernameInput_Signup.text;
        string pw = passwordInput_Signup.text;

        // 이름과 비밀번호가 입력되지 않았다면 처리
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pw))
        {
            Debug.LogError("이름과 비밀번호를 모두 입력해주세요!");
            return;
        }

        // 로그인 요청
        SignupUser(name, pw);
    }

    private void OnSigninButtonClick()
    {
        string name = usernameInput_Signin.text;
        string pw = passwordInput_Signin.text;
        // 이름과 비밀번호가 입력되지 않았다면 처리
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pw))
        {
            Debug.LogError("이름과 비밀번호를 모두 입력해주세요!");
            return;
        }
        // 회원가입 요청
        SigninUser(name, pw);
    }

    public void OnToSigninBtnClick()
    {
        signinPage.SetActive(true);
        signupPage.SetActive(false);
    }

    public void OnToSignUpBtnClick()
    {
        signinPage.SetActive(false);
        signupPage.SetActive(true);
    }

    // 회원가입 요청 함수
    public void SignupUser(string name, string pw)
    {
        User user = new User();
        user.name = name;
        user.pw = pw;

        // User 객체를 JSON으로 직렬화
        string json = JsonUtility.ToJson(user);

        // POST 요청을 위한 UnityWebRequest 생성
        StartCoroutine(SendRegistrationRequest(json));
    }

    // 로그인 요청 함수
    public void SigninUser(string name, string pw)
    {
        User user = new User();
        user.name = name;
        user.pw = pw;

        // User 객체를 JSON으로 직렬화
        string json = JsonUtility.ToJson(user);

        // POST 요청을 위한 UnityWebRequest 생성
        StartCoroutine(SendSigninRequest(json));
    }

    // POST 요청을 보내는 코루틴
    private IEnumerator SendRegistrationRequest(string json)
    {
        // UnityWebRequest로 POST 요청 생성
        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json);  // JSON 데이터를 바이트 배열로 변환
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);  // 업로드할 데이터 설정
        request.downloadHandler = new DownloadHandlerBuffer();    // 다운로드 핸들러 설정
        request.SetRequestHeader("Content-Type", "application/json"); // Content-Type 설정

        // 서버로 요청 보내기
        yield return request.SendWebRequest();

        // 응답 처리
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
            //  로그인으로 이동
            OnToSigninBtnClick();

            // 정보 지우기
            ClearInputText();

        }
        else
        {
            Debug.LogError("회원가입 실패: " + request.error);
        }
    }

    private IEnumerator SendSigninRequest(string json)
    {
        // UnityWebRequest로 POST 요청 생성
        UnityWebRequest request = new UnityWebRequest(serverUrl2, "POST");
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json);  // JSON 데이터를 바이트 배열로 변환
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);  // 업로드할 데이터 설정
        request.downloadHandler = new DownloadHandlerBuffer();    // 다운로드 핸들러 설정
        request.SetRequestHeader("Content-Type", "application/json"); // Content-Type 설정

        // 서버로 요청 보내기
        yield return request.SendWebRequest();

        // 응답 처리
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            if (!string.IsNullOrEmpty(responseText))
            {
                UserDTO response = JsonUtility.FromJson<UserDTO>(responseText);
                // 예: PlayerId와 이름을 저장
                PlayerPrefs.SetInt("PlayerId", (int)response.id);
                PlayerPrefs.SetString("PlayerName", response.name);
                PlayerPrefs.SetInt("BestScore", (int)response.bestScore);

                SceneManager.LoadScene("IngameScene");
            }

            // 정보 지우기
            ClearInputText();
        }
        else
        {
            Debug.LogError("로그인 실패: " + request.error);
        }
    }

    public void ClearInputText()
    {
        usernameInput_Signin.text = string.Empty;
        usernameInput_Signup.text = string.Empty;

        passwordInput_Signin.text= string.Empty;
        passwordInput_Signup.text= string.Empty;
    }
}
