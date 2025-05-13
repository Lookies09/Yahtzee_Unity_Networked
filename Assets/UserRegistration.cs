using UnityEngine;
using UnityEngine.UI;  // UI ���� Ŭ���� ����
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

    [Header("�α��� UI")]
    public GameObject signinPage;
    public InputField usernameInput_Signin;
    public InputField passwordInput_Signin;
    public Button SigninButton;
    public Button toSignupButton;

    // UI InputField
    [Header("ȸ������ UI")]
    public GameObject signupPage;
    public InputField usernameInput_Signup;
    public InputField passwordInput_Signup;
    public Button signupButton;
    public Button toSigninButton;    

    // ���� URL
    private string serverUrl = "http://localhost:8080/unity/signup";

    private string serverUrl2 = "http://localhost:8080/unity/signin";

    private void Start()
    {
        // ��ư Ŭ�� �� RegisterUser �޼��� ȣ��
        signupButton.onClick.AddListener(OnRegisterButtonClicked);
        SigninButton.onClick.AddListener(OnSigninButtonClick);

        toSigninButton.onClick.AddListener(OnToSigninBtnClick);
        toSignupButton.onClick.AddListener(OnToSignUpBtnClick);
    }

    // ��ư Ŭ�� �� ȣ��Ǵ� �޼���
    private void OnRegisterButtonClicked()
    {
        string name = usernameInput_Signup.text;
        string pw = passwordInput_Signup.text;

        // �̸��� ��й�ȣ�� �Էµ��� �ʾҴٸ� ó��
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pw))
        {
            Debug.LogError("�̸��� ��й�ȣ�� ��� �Է����ּ���!");
            return;
        }

        // �α��� ��û
        SignupUser(name, pw);
    }

    private void OnSigninButtonClick()
    {
        string name = usernameInput_Signin.text;
        string pw = passwordInput_Signin.text;
        // �̸��� ��й�ȣ�� �Էµ��� �ʾҴٸ� ó��
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pw))
        {
            Debug.LogError("�̸��� ��й�ȣ�� ��� �Է����ּ���!");
            return;
        }
        // ȸ������ ��û
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

    // ȸ������ ��û �Լ�
    public void SignupUser(string name, string pw)
    {
        User user = new User();
        user.name = name;
        user.pw = pw;

        // User ��ü�� JSON���� ����ȭ
        string json = JsonUtility.ToJson(user);

        // POST ��û�� ���� UnityWebRequest ����
        StartCoroutine(SendRegistrationRequest(json));
    }

    // �α��� ��û �Լ�
    public void SigninUser(string name, string pw)
    {
        User user = new User();
        user.name = name;
        user.pw = pw;

        // User ��ü�� JSON���� ����ȭ
        string json = JsonUtility.ToJson(user);

        // POST ��û�� ���� UnityWebRequest ����
        StartCoroutine(SendSigninRequest(json));
    }

    // POST ��û�� ������ �ڷ�ƾ
    private IEnumerator SendRegistrationRequest(string json)
    {
        // UnityWebRequest�� POST ��û ����
        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json);  // JSON �����͸� ����Ʈ �迭�� ��ȯ
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);  // ���ε��� ������ ����
        request.downloadHandler = new DownloadHandlerBuffer();    // �ٿ�ε� �ڵ鷯 ����
        request.SetRequestHeader("Content-Type", "application/json"); // Content-Type ����

        // ������ ��û ������
        yield return request.SendWebRequest();

        // ���� ó��
        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log(request.downloadHandler.text);
            //  �α������� �̵�
            OnToSigninBtnClick();

            // ���� �����
            ClearInputText();

        }
        else
        {
            Debug.LogError("ȸ������ ����: " + request.error);
        }
    }

    private IEnumerator SendSigninRequest(string json)
    {
        // UnityWebRequest�� POST ��û ����
        UnityWebRequest request = new UnityWebRequest(serverUrl2, "POST");
        byte[] jsonToSend = new UTF8Encoding().GetBytes(json);  // JSON �����͸� ����Ʈ �迭�� ��ȯ
        request.uploadHandler = new UploadHandlerRaw(jsonToSend);  // ���ε��� ������ ����
        request.downloadHandler = new DownloadHandlerBuffer();    // �ٿ�ε� �ڵ鷯 ����
        request.SetRequestHeader("Content-Type", "application/json"); // Content-Type ����

        // ������ ��û ������
        yield return request.SendWebRequest();

        // ���� ó��
        if (request.result == UnityWebRequest.Result.Success)
        {
            string responseText = request.downloadHandler.text;
            if (!string.IsNullOrEmpty(responseText))
            {
                UserDTO response = JsonUtility.FromJson<UserDTO>(responseText);
                // ��: PlayerId�� �̸��� ����
                PlayerPrefs.SetInt("PlayerId", (int)response.id);
                PlayerPrefs.SetString("PlayerName", response.name);
                PlayerPrefs.SetInt("BestScore", (int)response.bestScore);

                SceneManager.LoadScene("IngameScene");
            }

            // ���� �����
            ClearInputText();
        }
        else
        {
            Debug.LogError("�α��� ����: " + request.error);
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
