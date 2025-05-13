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
        // FormData로 전달할 데이터를 준비
        WWWForm form = new WWWForm();
        form.AddField("id", playerId.ToString());
        form.AddField("score", score.ToString());

        // UnityWebRequest로 POST 요청 생성
        UnityWebRequest request = UnityWebRequest.Post(serverUrl, form);

        // 요청 보내기
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("점수 저장 성공!");
        }
        else
        {
            Debug.LogError($"점수 저장 실패: {request.error}");
        }
    }

}
