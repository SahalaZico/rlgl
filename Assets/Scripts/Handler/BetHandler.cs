using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BetHandler : Singleton<BetHandler>
{
    [Header("UI")]
    [SerializeField] GameObject parent;

    [Header("Sprite")]
    [SerializeField] Sprite[] chipSprite;
    [SerializeField] Sprite[] chipSelectionSprite;

    [Header("Highlight")]
    [SerializeField] List<GameObject> selectedSingleNumberHighlight;
    [SerializeField] List<GameObject> areaNumberHighlight;

    [Header("Button Numbers")]
    [SerializeField] List<GameObject> buttonNumbers;
    [SerializeField] List<GameObject> buttonTwoNumbers;
    [SerializeField] List<GameObject> buttonFourNumbers;
    [SerializeField] List<GameObject> buttonOddEven;
    [SerializeField] List<GameObject> buttonRedBlue;

    [Header("Chip")]
    [SerializeField] GameObject selectableChipHighlight;
    [SerializeField] List<GameObject> selectableChipObjects;
    [SerializeField] Vector3 selectedChipScale;

    [Header("Button")]
    [SerializeField] GameObject buttonClear;
    [SerializeField] GameObject buttonSubmit;

    private long playerBalance;
    private long tempPlayerBalance;
    private float[] chipValue = new float[] { 1, 5, 10, 50, 100, 500, 1000, 5000, 10000, 25000 };
    private float minimalBetNumber;
    private float maximalBetNumber;
    private float minimalBet50;
    private float maximalBet50;

    List<string> buttonTwoNumberValues = new List<string>();
    List<string> buttonFourNumberValues = new List<string>();

    int selectedChipIndex = 0;
    float[] chipBetOnNumbers = new float[12];
    float[] chipBetOnTwoNumbers;
    float[] chipBetOnFourNumbers;
    float[] chipBetOddEven = new float[2];
    float[] chipBetRedBlue = new float[2];

    private void Awake()
    {
        parent.SetActive(false);

        chipBetOnTwoNumbers = new float[buttonTwoNumbers.Count];
        buttonTwoNumbers.ForEach(button => {
            string values = button.name.Replace("Click area (", "").Replace(")", "");
            buttonTwoNumberValues.Add(values);
        });

        chipBetOnFourNumbers = new float[buttonFourNumbers.Count];
        buttonFourNumbers.ForEach(button => {
            string values = button.name.Replace("Click area (", "").Replace(")", "");
            buttonFourNumberValues.Add(values);
        });

        AssignButtonNumber();
        AssignButtonTwoNumber();
        AssignButtonFourNumber();
        AssignButtonOddEven();
        AssignButtonRedBlue();

        ClearChipSelection();

        SetChipSelection();
        OnChipSelected(0, false);

        AssignButtonClear();
        AssignButtonSubmit();
    }

    #region Public Function
    public void Show()
    {
        parent.SetActive(true);

        ClearChipSelection();
        OnChipSelected(0, false);
    }
    #endregion

    #region Private Function
    void SetChipSelection()
    {
        for (int i = 0; i < selectableChipObjects.Count; i++)
        {
            Button entry = selectableChipObjects[i].AddComponent<Button>();
            int index = i;
            selectableChipObjects[index].GetComponent<Image>().sprite = chipSelectionSprite[index];

            String chipText = chipValue[index].ToString() + "K";
            if (chipValue[i] >= 1000)
            {
                chipText = (chipValue[index] / 1000).ToString().Replace(".", ",") + "M";
            }
            selectableChipObjects[index].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = chipText;
            entry.onClick.AddListener(() => { OnChipSelected(index); });
        }
    }

    void ClearChipSelection(bool clearAll = true)
    {
        tempPlayerBalance = playerBalance;
        selectedSingleNumberHighlight.ForEach(x => x.SetActive(false));
        areaNumberHighlight.ForEach(x => x.SetActive(false));

        for (int i = 0; i < chipBetOnNumbers.Length; i++)
        {
            chipBetOnNumbers[i] = 0;
            Image imageChip = buttonNumbers[i].transform.GetChild(0).GetComponent<Image>();
            imageChip.gameObject.SetActive(false);
        }
        for (int i = 0; i < chipBetOnTwoNumbers.Length; i++)
        {
            chipBetOnTwoNumbers[i] = 0;
            Image imageChip = buttonTwoNumbers[i].transform.GetChild(0).GetComponent<Image>();
            imageChip.gameObject.SetActive(false);
        }
        for (int i = 0; i < chipBetOnFourNumbers.Length; i++)
        {
            chipBetOnFourNumbers[i] = 0;
            Image imageChip = buttonFourNumbers[i].transform.GetChild(0).GetComponent<Image>();
            imageChip.gameObject.SetActive(false);
        }
        for (int i = 0; i < chipBetRedBlue.Length; i++)
        {
            chipBetRedBlue[i] = 0;
            Image imageChip = buttonRedBlue[i].transform.GetChild(0).GetComponent<Image>();
            imageChip.gameObject.SetActive(false);
        }
        for (int i = 0; i < chipBetOddEven.Length; i++)
        {
            chipBetOddEven[i] = 0;
            Image imageChip = buttonOddEven[i].transform.GetChild(0).GetComponent<Image>();
            imageChip.gameObject.SetActive(false);
        }

        if (clearAll)
        {

        }

        CountTotalBet();
    }

    void CountTotalBet()
    {
        float total = 0;
        for (int i = 0; i < chipBetOnNumbers.Length; i++)
        {
            total += chipBetOnNumbers[i];
        }
        for (int i = 0; i < chipBetOnTwoNumbers.Length; i++)
        {
            total += chipBetOnTwoNumbers[i];
        }
        for (int i = 0; i < chipBetOnFourNumbers.Length; i++)
        {
            total += chipBetOnFourNumbers[i];
        }
        for (int i = 0; i < chipBetRedBlue.Length; i++)
        {
            total += chipBetRedBlue[i];
        }
        for (int i = 0; i < chipBetOddEven.Length; i++)
        {
            total += chipBetOddEven[i];
        }

        //txtTotalBet.text = StringHelper.MoneyFormat(total * 1000);
    }

    void AssignButtonNumber()
    {
        for (int i = 0; i < buttonNumbers.Count; i++)
        {
            EventTrigger eventTrigger = buttonNumbers[i].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            int index = i;
            entry.callback.AddListener((data) => { OnNumberButtonDown(index); });
            eventTrigger.triggers.Add(entry);
        }
    }

    void AssignButtonTwoNumber()
    {
        for (int i = 0; i < buttonTwoNumbers.Count; i++)
        {
            EventTrigger eventTrigger = buttonTwoNumbers[i].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            int index = i;
            entry.callback.AddListener((data) => { OnTwoNumberButtonDown(index); });
            eventTrigger.triggers.Add(entry);
        }
    }

    void AssignButtonFourNumber()
    {
        for (int i = 0; i < buttonFourNumbers.Count; i++)
        {
            EventTrigger eventTrigger = buttonFourNumbers[i].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            int index = i;
            entry.callback.AddListener((data) => { OnFourNumberButtonDown(index); });
            eventTrigger.triggers.Add(entry);
        }
    }

    void AssignButtonOddEven()
    {
        for (int i = 0; i < buttonOddEven.Count; i++)
        {
            EventTrigger eventTrigger = buttonOddEven[i].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            int index = i;
            entry.callback.AddListener((data) => { OnOddEvenButtonDown(index); });
            eventTrigger.triggers.Add(entry);
        }
    }

    void AssignButtonRedBlue()
    {
        for (int i = 0; i < buttonRedBlue.Count; i++)
        {
            EventTrigger eventTrigger = buttonRedBlue[i].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new()
            {
                eventID = EventTriggerType.PointerDown
            };
            int index = i;
            entry.callback.AddListener((data) => { OnRedBlueButtonDown(index); });
            eventTrigger.triggers.Add(entry);
        }
    }

    void AssignButtonClear()
    {
        EventTrigger eventTrigger = buttonClear.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entry.callback.AddListener((data) => {
            Debug.Log("Clear..");

            ClearChipSelection();
        });
        eventTrigger.triggers.Add(entry);
    }

    void AssignButtonSubmit()
    {
        EventTrigger eventTrigger = buttonSubmit.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new()
        {
            eventID = EventTriggerType.PointerDown
        };
        entry.callback.AddListener((data) => {
            Debug.Log("Submit..");

            parent.SetActive(false);
            StatusHandler.Instance.ChangeStatus(StatusHandler.STATUS.OnGameplay, true);
        });
        eventTrigger.triggers.Add(entry);
    }
    #endregion

    #region Listener Function
    void OnChipSelected(int index, bool checkBallance = true)
    {
        //if (checkBallance)
        //{
        //    if (tempPlayerBalance < chipValue[index] * 1000)
        //    {
        //        Debug.Log("Saldo anda tidak mencukupi!\nsilahkan top up terlebih dahulu");
        //        return;
        //    }
        //}

        selectableChipObjects.ForEach(c => c.transform.localScale = Vector3.one);
        selectedChipIndex = index;
        selectableChipObjects[index].transform.localScale = selectedChipScale;
        selectableChipHighlight.transform.position = selectableChipObjects[index].transform.position;
    }

    void OnNumberButtonDown(int index)
    {
        Debug.Log("button number " + index);
        
        //if (tempPlayerBalance < chipValue[selectedChipIndex] * 1000)
        //{
        //    Debug.Log("Saldo anda tidak mencukupi!\nsilahkan top up terlebih dahulu");
        //    return;
        //}

        chipBetOnNumbers[index] += chipValue[selectedChipIndex];
        bool isValid = chipBetOnNumbers[index] >= minimalBetNumber && chipBetOnNumbers[index] <= maximalBetNumber;
        chipBetOnNumbers[index] -= chipValue[selectedChipIndex];

        //if (!isValid)
        //{
        //    Debug.Log("Batas bet angka, minimum " + getMinNumberChipText() + " dan maximum " + getMaxNumberChipText());
        //    return;
        //}

        tempPlayerBalance -= long.Parse((chipValue[selectedChipIndex]).ToString()) * 1000;

        chipBetOnNumbers[index] += chipValue[selectedChipIndex];
        int skin = 0;
        for (int i = 0; i < chipValue.Length; i++)
        {
            if (chipBetOnNumbers[index] >= chipValue[i])
                skin = i;
        }

        Image imageChip = buttonNumbers[index].transform.GetChild(0).GetComponent<Image>();
        imageChip.sprite = chipSprite[skin];

        TextMeshProUGUI textChip = imageChip.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textChip.text = chipBetOnNumbers[index] + "K";
        if (chipBetOnNumbers[index] >= 1000)
            textChip.text = (chipBetOnNumbers[index] / 1000).ToString().Replace(".", ",") + "M";

        imageChip.gameObject.SetActive(chipBetOnNumbers[index] > 0);
        CountTotalBet();
    }

    void OnTwoNumberButtonDown(int index)
    {
        Debug.Log("button two number " + index);

        //if (tempPlayerBalance < chipValue[selectedChipIndex] * 1000)
        //{
        //    Debug.Log("Saldo anda tidak mencukupi!\nsilahkan top up terlebih dahulu");
        //    return;
        //}

        chipBetOnTwoNumbers[index] += chipValue[selectedChipIndex];
        bool isValid = chipBetOnTwoNumbers[index] >= minimalBetNumber && chipBetOnTwoNumbers[index] <= maximalBetNumber;
        chipBetOnTwoNumbers[index] -= chipValue[selectedChipIndex];

        //if (!isValid)
        //{
        //    Debug.Log("Batas bet angka, minimum " + getMinNumberChipText() + " dan maximum " + getMaxNumberChipText());
        //    return;
        //}

        tempPlayerBalance -= long.Parse((chipValue[selectedChipIndex]).ToString()) * 1000;

        chipBetOnTwoNumbers[index] += chipValue[selectedChipIndex];
        int skin = 0;
        for (int i = 0; i < chipValue.Length; i++)
        {
            if (chipBetOnTwoNumbers[index] >= chipValue[i])
                skin = i;
        }

        Image imageChip = buttonTwoNumbers[index].transform.GetChild(0).GetComponent<Image>();
        imageChip.sprite = chipSprite[skin];

        TextMeshProUGUI textChip = imageChip.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textChip.text = chipBetOnTwoNumbers[index] + "K";

        if (chipBetOnTwoNumbers[index] >= 1000)
            textChip.text = (chipBetOnTwoNumbers[index] / 1000).ToString().Replace(".", ",") + "M";

        imageChip.gameObject.SetActive(chipBetOnTwoNumbers[index] > 0);

        if (chipBetOnTwoNumbers[index] > 0)
        {
            string[] numString = buttonTwoNumberValues[index].Split(", ");
            for (int i = 0; i < numString.Length; i++)
            {
                areaNumberHighlight[int.Parse(numString[i]) - 1].SetActive(true);
            }
        }

        CountTotalBet();
    }

    void OnFourNumberButtonDown(int index)
    {
        Debug.Log("button four number " + index);

        //if (tempPlayerBalance < chipValue[selectedChipIndex] * 1000)
        //{
        //    Debug.Log("Saldo anda tidak mencukupi!\nsilahkan top up terlebih dahulu");
        //    return;
        //}

        chipBetOnFourNumbers[index] += chipValue[selectedChipIndex];
        bool isValid = chipBetOnFourNumbers[index] >= minimalBetNumber && chipBetOnFourNumbers[index] <= maximalBetNumber;
        chipBetOnFourNumbers[index] -= chipValue[selectedChipIndex];

        //if (!isValid)
        //{
        //    Debug.Log("Batas bet angka, minimum " + getMinNumberChipText() + " dan maximum " + getMaxNumberChipText());
        //    return;
        //}

        tempPlayerBalance -= long.Parse((chipValue[selectedChipIndex]).ToString()) * 1000;

        chipBetOnFourNumbers[index] += chipValue[selectedChipIndex];
        int skin = 0;
        for (int i = 0; i < chipValue.Length; i++)
        {
            if (chipBetOnFourNumbers[index] >= chipValue[i])
                skin = i;
        }

        Image imageChip = buttonFourNumbers[index].transform.GetChild(0).GetComponent<Image>();
        imageChip.sprite = chipSprite[skin];

        TextMeshProUGUI textChip = imageChip.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textChip.text = chipBetOnFourNumbers[index] + "K";

        if (chipBetOnFourNumbers[index] >= 1000)
            textChip.text = (chipBetOnFourNumbers[index] / 1000).ToString().Replace(".", ",") + "M";

        imageChip.gameObject.SetActive(chipBetOnFourNumbers[index] > 0);

        if (chipBetOnFourNumbers[index] > 0)
        {
            string[] numString = buttonFourNumberValues[index].Split(", ");
            for (int i = 0; i < numString.Length; i++)
            {
                areaNumberHighlight[int.Parse(numString[i]) - 1].SetActive(true);
            }
        }

        CountTotalBet();
    }

    void OnOddEvenButtonDown(int index)
    {
        Debug.Log("button odd even number " + index);

        //if (tempPlayerBalance < chipValue[selectedChipIndex] * 1000)
        //{
        //    Debug.Log("Saldo anda tidak mencukupi!\nsilahkan top up terlebih dahulu");
        //    return;
        //}

        chipBetOddEven[index] += chipValue[selectedChipIndex];
        bool isValid = chipBetOddEven[index] >= minimalBet50 && chipBetOddEven[index] <= maximalBet50;
        chipBetOddEven[index] -= chipValue[selectedChipIndex];

        //if (!isValid)
        //{
        //    Debug.Log("Batas bet even, odd, red atau blue, minimum " + getMin50ChipText() + " dan maximum " + getMax50ChipText());
        //    return;
        //}

        Image imageChip = buttonOddEven[0].transform.GetChild(0).GetComponent<Image>();
        imageChip.gameObject.SetActive(index == 0);

        imageChip = buttonOddEven[1].transform.GetChild(0).GetComponent<Image>();
        imageChip.gameObject.SetActive(index == 1);

        if (index == 0)
            chipBetOddEven[1] = 0;
        if (index == 1)
            chipBetOddEven[0] = 0;

        tempPlayerBalance -= long.Parse((chipValue[selectedChipIndex]).ToString()) * 1000;

        chipBetOddEven[index] += chipValue[selectedChipIndex];

        int skin = 0;
        for (int i = 0; i < chipValue.Length; i++)
        {
            if (chipBetOddEven[index] >= chipValue[i])
                skin = i;
        }

        imageChip = buttonOddEven[index].transform.GetChild(0).GetComponent<Image>();
        imageChip.sprite = chipSprite[skin];

        TextMeshProUGUI textChip = imageChip.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textChip.text = chipBetOddEven[index] + "K";
        if (chipBetOddEven[index] >= 1000)
            textChip.text = (chipBetOddEven[index] / 1000).ToString().Replace(".", ",") + "M";

        CountTotalBet();
    }

    void OnRedBlueButtonDown(int index)
    {
        Debug.Log("button red blue number " + index);

        //if (tempPlayerBalance < chipValue[selectedChipIndex] * 1000)
        //{
        //    Debug.Log("Saldo anda tidak mencukupi!\nsilahkan top up terlebih dahulu");
        //    return;
        //}

        chipBetRedBlue[index] += chipValue[selectedChipIndex];
        bool isValid = chipBetRedBlue[index] >= minimalBet50 && chipBetRedBlue[index] <= maximalBet50;
        chipBetRedBlue[index] -= chipValue[selectedChipIndex];

        //if (!isValid)
        //{
        //    Debug.Log("Batas bet even/odd & red/blue, minimum " + getMin50ChipText() + " dan maximum " + getMax50ChipText());
        //    return;
        //}

        Image imageChip = buttonRedBlue[0].transform.GetChild(0).GetComponent<Image>();
        imageChip.gameObject.SetActive(index == 0);

        imageChip = buttonRedBlue[1].transform.GetChild(0).GetComponent<Image>();
        imageChip.gameObject.SetActive(index == 1);

        if (index == 0)
            chipBetRedBlue[1] = 0;
        if (index == 1)
            chipBetRedBlue[0] = 0;

        tempPlayerBalance -= long.Parse((chipValue[selectedChipIndex]).ToString()) * 1000;

        chipBetRedBlue[index] += chipValue[selectedChipIndex];

        int skin = 0;
        for (int i = 0; i < chipValue.Length; i++)
        {
            if (chipBetRedBlue[index] >= chipValue[i])
                skin = i;
        }

        imageChip = buttonRedBlue[index].transform.GetChild(0).GetComponent<Image>();
        imageChip.sprite = chipSprite[skin];

        TextMeshProUGUI textChip = imageChip.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        textChip.text = chipBetRedBlue[index] + "K";
        if (chipBetRedBlue[index] >= 1000)
            textChip.text = (chipBetRedBlue[index] / 1000).ToString().Replace(".", ",") + "M";

        CountTotalBet();
    }
    #endregion
}