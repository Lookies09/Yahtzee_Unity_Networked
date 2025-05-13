using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class GameManager : MonoBehaviour
{
    // �� ����
    public static int totalScore = 0;

    // ������ �ִ� �ֻ��� Ƚ��
    private int rolls = 3;

    // �ֻ��� Ŭ���� ����Ʈ (�������� ���� ��)
    [SerializeField] private List<Dice> dices = new List<Dice>();
    // �ֻ��� ��ư
    [SerializeField] private Button[] diceBtns;

    // �ֻ��� �������� ��
    [SerializeField] private Color pressedColor;
    // �ֻ��� ���� �� (�������� ������)
    private Color originalColor;

    // �������� �ؽ�Ʈ �迭
    [SerializeField] private Text[] singlesValueTxt;

    [SerializeField] private Text myNameText;

    // 3X �ؽ�Ʈ
    [SerializeField] private Text threeXTxt;
    // 4X �ؽ�Ʈ
    [SerializeField] private Text fourXTxt;
    // Ǯ�Ͽ콺 �ؽ�Ʈ
    [SerializeField] private Text fullHouseTxt;
    // ���� �ؽ�Ʈ
    [SerializeField] private Text yahtzeeTxt;
    // �����ؽ�Ʈ
    [SerializeField] private Text chanceTxt;

    // ������Ʈ����Ʈ �ؽ�Ʈ
    [SerializeField] private Text ssText;
    // ������Ʈ����Ʈ �ؽ�Ʈ
    [SerializeField] private Text lsText;

    // ��������Ʈ ��ü ���� �ؽ�Ʈ
    [SerializeField] private Text singles_All_ValueTxt;
    // ���ʽ����� �ؽ�Ʈ ���ӿ�����Ʈ
    [SerializeField] private GameObject bonusTxt;

    // �� ���� �ؽ�Ʈ
    [SerializeField] private Text totalPointTxt;

    // ����� ���� ��ư��(�������� ���� ��)
    private List<Button> specialTxtBtn = new List<Button>();

    // �ױװ� values�� ������Ʈ�� ����
    private GameObject[] valuesTexts;

    // ���� ��ư ���� Ƚ�� ����� �� (���� �� ���� ��ư�� ��: 13��)
    private int pressedBtn = 13;

    private void Start()
    {
        totalScore = 0;

        myNameText.text = PlayerPrefs.GetString("PlayerName");

        valuesTexts = GameObject.FindGameObjectsWithTag("Values");

        // ó�� �ֻ��� �÷� �����ϱ�
        originalColor = diceBtns[0].GetComponent<Image>().color;

        // ��ư �����ͼ� �����ϱ�
        specialTxtBtn.Add(threeXTxt?.GetComponentInParent<Button>());
        specialTxtBtn.Add(fourXTxt?.GetComponentInParent<Button>());
        specialTxtBtn.Add(fullHouseTxt?.GetComponentInParent<Button>());
        specialTxtBtn.Add(yahtzeeTxt?.GetComponentInParent<Button>());
        specialTxtBtn.Add(ssText?.GetComponentInParent<Button>());
        specialTxtBtn.Add(lsText?.GetComponentInParent<Button>());
    }

    // �ֻ��� ������
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

        // ���� �ֻ��� �� �ֱ�
        SetSingles(dices);

        // Ư�� �ֻ��� �� �ֱ�
        SetSpecialDice(dices);

        // �ֻ��� �������ִ� Ƚ�� ����
        rolls--;
    }

    // �ֻ��� ����
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

        // �� ���� �� �ֻ��� �ʱ�ȭ �ϱ�
        for (int i = 0; i < dices.Count; i++)
        {
            dices[i].IsTake = false;
            dices[i].Num = 0;
            dices[i].ResetDice();
            diceBtns[i].GetComponent<Image>().color = originalColor;
        }

        // �־�� ���� �ױ�
        foreach (GameObject t in valuesTexts)
        {
            if (t.GetComponentInParent<Button>().interactable)
                t.GetComponent<Text>().text = "0";
        }

        // �� ������ �ֻ��� ������ Ƚ�� �ʱ�ȭ
        rolls = 3;

        // ���� ��ư Ȯ���ϱ�
        pressedBtn--;
        // ���� ���� ó��
        if (pressedBtn == 0){
            totalScore = int.Parse(totalPointTxt.text);
            SceneManager.LoadScene("EndScene");
        }
    }

    // ���� �ֻ��� �ֱ�
    public void SetSingles(List<Dice> dices)
    {
        var result = CheckSingles(dices);

        // ���� �ؽ�Ʈ �� �ֱ�
        SetTextIfInteractable(chanceTxt, dices.Sum(x => x.Num).ToString());

        for (int i = 0; i < singlesValueTxt.Length; i++)
        {
            int key = i + 1;
            int count = result.ContainsKey(key) ? result[key] : 0;
            SetTextIfInteractable(singlesValueTxt[i], (count * key).ToString());
        }
    }

    // Ư�� �ֻ��� �� �ֱ�
    public void SetSpecialDice(List<Dice> dices)
    {
        var filtered = dices.GroupBy(d => d.Num).ToDictionary(g => g.Key, g => g.Count());

        // ��ư�� Ȱ��ȭ�Ǿ� �ִٸ� ��� 0���� �ʱ�ȭ
        foreach (Button btn in specialTxtBtn)
        {
            if (!btn.interactable) continue;
            btn.GetComponentInChildren<Text>().text = "0";
        }

        // ���ӵ� ���� ���� üũ (4�� �̻��̸� SS, 5���� LS)
        int consec = CheckConsecutive(dices);
        if (consec >= 4)
        {
            if (consec == 5) SetTextIfInteractable(lsText, "40");
            SetTextIfInteractable(ssText, "30");
        }

        // 3�� �̻� �ߺ��� ���ڰ� ������ �Ʒ� ���� ���� X
        if (filtered.Values.Max() < 3) return;

        // �� ��°�� ���� ������ ���� ���� (FullHouse �Ǵܿ�)
        int secondMax = filtered.Values
            .Where(x => x != filtered.Values.Max())
            .DefaultIfEmpty(0)
            .Max();

        int num = dices.Sum(x => x.Num);

        // ���� ���� 4�� �̻��̸� Four of a kind, 5���� Yahtzee
        if (filtered.Values.Max() >= 4)
        {
            if (filtered.Values.Max() == 5)
                SetTextIfInteractable(yahtzeeTxt, "50");

            SetTextIfInteractable(fourXTxt, num.ToString());
        }

        // 3�� + 2���� �� FullHouse ó��
        if (secondMax == 2)
            SetTextIfInteractable(fullHouseTxt, "25");

        // 3�� �̻��̸� Three of a kind ó��
        SetTextIfInteractable(threeXTxt, num.ToString());
    }

    // �� �ֻ����� � �ִ��� ���
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

    // �ֻ����� ��� ���ӵǴ��� Ȯ���ϱ�
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
