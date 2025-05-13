using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.SocialPlatforms.Impl;

public class EndSceneManager : MonoBehaviour
{
    [SerializeField] private Text totalScoreTxt;

    [SerializeField] private GameObject newBestScoreText;

    private string serverUrl = "http://localhost:8080/unity/setscore";

    // Start is called before the first frame update
    void Start()
    {
        totalScoreTxt.text = GameManager.totalScore.ToString();

        if (PlayerPrefs.GetInt("BestScore") < GameManager.totalScore)
        {
            newBestScoreText.SetActive(true);

            StartCoroutine(SendScoreToServer(PlayerPrefs.GetInt("PlayerId"), GameManager.totalScore));
        }
    }

    public void OnReplayBtnClick()
    {
        SceneManager.LoadScene("IngameScene");
    }

    private IEnumerator SendScoreToServer(long playerId, long score)
    {
        // FormData�� ������ �����͸� �غ�
        WWWForm form = new WWWForm();
        form.AddField("id", playerId.ToString());
        form.AddField("score", score.ToString());

        // UnityWebRequest�� POST ��û ����
        UnityWebRequest request = UnityWebRequest.Post(serverUrl, form);

        // ��û ������
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("���� ���� ����!");
        }
        else
        {
            Debug.LogError($"���� ���� ����: {request.error}");
        }
    }

}
