using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    // 총 점수
    public static int totalScore = 0;

    // 던지수 있는 주사위 횟수
    private int rolls = 3;

    // 주사위 클래스 리스트 (동적으로 생성 중)
    [SerializeField] private List<Dice> dices = new List<Dice>();
    // 주사위 버튼
    [SerializeField] private Button[] diceBtns;

    // 주사위 눌렀을때 색
    [SerializeField] private Color pressedColor;
    // 주사위 원래 색 (동적으로 참조중)
    private Color originalColor;

    // 단일점수 텍스트 배열
    [SerializeField] private Text[] singlesValueTxt;

    [SerializeField] private Text myNameText;

    // 3X 텍스트
    [SerializeField] private Text threeXTxt;
    // 4X 텍스트
    [SerializeField] private Text fourXTxt;
    // 풀하우스 텍스트
    [SerializeField] private Text fullHouseTxt;
    // 얏찌 텍스트
    [SerializeField] private Text yahtzeeTxt;
    // 찬스텍스트
    [SerializeField] private Text chanceTxt;

    // 스몰스트레이트 텍스트
    [SerializeField] private Text ssText;
    // 라지스트레이트 텍스트
    [SerializeField] private Text lsText;

    // 단일포인트 전체 점수 텍스트
    [SerializeField] private Text singles_All_ValueTxt;
    // 보너스점수 텍스트 게임오브젝트
    [SerializeField] private GameObject bonusTxt;

    // 총 점수 텍스트
    [SerializeField] private Text totalPointTxt;

    // 스페셜 점수 버튼들(동적으로 참조 중)
    private List<Button> specialTxtBtn = new List<Button>();

    // 테그가 values인 오브젝트들 참조
    private GameObject[] valuesTexts;

    // 저장 버튼 눌린 횟수 기억할 값 (현재 값 저장 버튼의 수: 13개)
    private int pressedBtn = 13;

    private void Start()
    {
        totalScore = 0;

        myNameText.text = PlayerPrefs.GetString("PlayerName");

        valuesTexts = GameObject.FindGameObjectsWithTag("Values");

        // 처음 주사위 컬러 저장하기
        originalColor = diceBtns[0].GetComponent<Image>().color;

        // 버튼 가져와서 저장하기
        specialTxtBtn.Add(threeXTxt?.GetComponentInParent<Button>());
        specialTxtBtn.Add(fourXTxt?.GetComponentInParent<Button>());
        specialTxtBtn.Add(fullHouseTxt?.GetComponentInParent<Button>());
        specialTxtBtn.Add(yahtzeeTxt?.GetComponentInParent<Button>());
        specialTxtBtn.Add(ssText?.GetComponentInParent<Button>());
        specialTxtBtn.Add(lsText?.GetComponentInParent<Button>());
    }

    // 주사위 굴리기
    public void Roll()
    {
        if (rolls <= 0) { return; }

        for (int i = 0; i < dices.Count; i++)
        {
            if (dices[i].IsTake) { continue; }
            int num = Random.Range(1, 7);
            dices[i].Num = num;
            dices[i].StartRollAnimation();         
        }

        // 단일 주사위 값 넣기
        SetSingles(dices);

        // 특별 주사위 값 넣기
        SetSpecialDice(dices);

        // 주사위 굴릴수있는 횟수 감소
        rolls--;
    }

    // 주사위 고정
    public void SetDice(int i)
    {
        if (dices[i].Num == 0) { return; }
        dices[i].IsTake = !dices[i].IsTake;
        diceBtns[i].image.color = dices[i].IsTake ? pressedColor : originalColor;        
    }

    public void SetValue(Button btn)
    {
        btn.interactable = false;
        btn.image.color = pressedColor;

        SetPoint();

        // 값 선택 후 주사위 초기화 하기
        for (int i = 0; i < dices.Count; i++)
        {
            dices[i].IsTake = false;
            dices[i].Num = 0;
            dices[i].ResetDice();
            diceBtns[i].GetComponent<Image>().color = originalColor;
        }

        // 넣어둔 값들 테그
        foreach (GameObject t in valuesTexts)
        {
            if (t.GetComponentInParent<Button>().interactable)
                t.GetComponent<Text>().text = "0";
        }

        // 값 선택후 주사위 굴리기 횟수 초기화
        rolls = 3;

        // 눌린 버튼 확인하기
        pressedBtn--;
        // 게임 종료 처리
        if (pressedBtn == 0){
            totalScore = int.Parse(totalPointTxt.text);
            SceneManager.LoadScene("EndScene");
        }
    }

    // 단일 주사위 넣기
    public void SetSingles(List<Dice> dices)
    {
        var result = CheckSingles(dices);

        // 찬스 텍스트 값 넣기
        SetTextIfInteractable(chanceTxt, dices.Sum(x => x.Num).ToString());

        for (int i = 0; i < singlesValueTxt.Length; i++)
        {
            int key = i + 1;
            int count = result.ContainsKey(key) ? result[key] : 0;
            SetTextIfInteractable(singlesValueTxt[i], (count * key).ToString());
        }
    }

    // 특별 주사위 값 넣기
    public void SetSpecialDice(List<Dice> dices)
    {
        var filtered = dices.GroupBy(d => d.Num).ToDictionary(g => g.Key, g => g.Count());

        // 버튼이 활성화되어 있다면 모두 0으로 초기화
        foreach (Button btn in specialTxtBtn)
        {
            if (!btn.interactable) continue;
            btn.GetComponentInChildren<Text>().text = "0";
        }

        // 연속된 수의 개수 체크 (4개 이상이면 SS, 5개면 LS)
        int consec = CheckConsecutive(dices);
        if (consec >= 4)
        {
            if (consec == 5) SetTextIfInteractable(lsText, "40");
            SetTextIfInteractable(ssText, "30");
        }

        // 3개 이상 중복된 숫자가 없으면 아래 로직 실행 X
        if (filtered.Values.Max() < 3) return;

        // 두 번째로 많이 등장한 수의 개수 (FullHouse 판단용)
        int secondMax = filtered.Values
            .Where(x => x != filtered.Values.Max())
            .DefaultIfEmpty(0)
            .Max();

        int num = dices.Sum(x => x.Num);

        // 같은 수가 4개 이상이면 Four of a kind, 5개면 Yahtzee
        if (filtered.Values.Max() >= 4)
        {
            if (filtered.Values.Max() == 5)
                SetTextIfInteractable(yahtzeeTxt, "50");

            SetTextIfInteractable(fourXTxt, num.ToString());
        }

        // 3개 + 2개일 때 FullHouse 처리
        if (secondMax == 2)
            SetTextIfInteractable(fullHouseTxt, "25");

        // 3개 이상이면 Three of a kind 처리
        SetTextIfInteractable(threeXTxt, num.ToString());
    }

    // 각 주사위가 몇개 있는지 계산
    public Dictionary<int, int> CheckSingles(List<Dice> dices)
    {
        Dictionary<int, int> countDict = new Dictionary<int, int>();

        foreach (var value in dices)
        {
            if (countDict.ContainsKey(value.Num))
            {
                countDict[value.Num]++;
            }
            else
            {
                countDict[value.Num] = 1;
            }
        }

        return countDict;

    }

    // 주사위가 몇번 연속되는지 확인하기
    public int CheckConsecutive(List<Dice> dices)
    {
        List<Dice> sortedList = dices.OrderBy(x => x.Num).ToList();

        int count = 1;

        int maxCount = 1;

        for (int i = 1; i < sortedList.Count; i++)
        {
            if (sortedList[i].Num-1 == sortedList[i-1].Num)
            {
                count ++;
                maxCount = Mathf.Max(count, maxCount);
            }
            else if (sortedList[i].Num == sortedList[i - 1].Num)
            {
                continue;
            }
            else
            {
                count = 1;
            }
        }
        return maxCount;        
    }

    public void SetPoint()
    {
        int totalPoint = 0;

        int value = 0;

        foreach (Text t in singlesValueTxt)
        {
            if (t.GetComponentInParent<Button>().interactable) { continue; }
            value += int.Parse(t.text);
        }

        singles_All_ValueTxt.text = $"{value} / 63";

        if (value >= 63)
        {
            if (!bonusTxt.activeSelf)
            {
                bonusTxt.SetActive(true);
            }
            totalPoint += 35;
        }

        foreach (Button b in specialTxtBtn)
        {
            if (b.interactable) { continue; }
            string v = b.GetComponentInChildren<Text>().text;
            totalPoint += int.Parse(v);
        }

        if (!chanceTxt.GetComponentInParent<Button>().interactable)
            totalPoint += int.Parse(chanceTxt.text);

        totalPoint += value;

        
        totalPointTxt.text = totalPoint.ToString();
    }

    private void SetTextIfInteractable(Text target, string value)
    {
        if (target.GetComponentInParent<Button>().interactable)
            target.text = value;
    }

}
