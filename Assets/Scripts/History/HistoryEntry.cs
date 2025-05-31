using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class HistoryEntry : MonoBehaviour
{
    [SerializeField] protected TMP_Text textDate = null;
    [SerializeField] protected TMP_Text textGame = null;
    [SerializeField] protected TMP_Text textBet = null;
    [SerializeField] protected TMP_Text textPayout = null;
    [SerializeField] protected TMP_Text textResult = null;

    protected HistoryJson.Data cacheData = null;

    public void SetData(HistoryJson.Data input)
    {
        string currency = UIGameplay.Instance.GetCurrency();
        cacheData = input;

        textBet.text = string.Format("{0}<br>{1}", currency.ToUpper(), StringUtility.ConvertDoubleToString(cacheData.data.detail_bet.amount, currency));
        try
        {
            DateTime parsedDate = DateTime.ParseExact(cacheData.created_date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            string formattedDate = parsedDate.ToString("dd MMMM yyyy");
            textDate.text = formattedDate;
        } catch (System.Exception ex)
        {
            Debug.Log("Error parse datetime history: " + ex.Message);
        }

        textGame.text = cacheData.round_id.ToString();
        textPayout.text = string.Format("{0}<br>{1}", currency.ToUpper(), StringUtility.ConvertDoubleToString(cacheData.data.total_win, currency));
        textResult.text = "";
    }

    public void ClearData()
    {
        cacheData = null;

        textBet.text = "";
        textDate.text = "";
        textGame.text = "";
        textPayout.text = "";
        textResult.text = "";
    }

    // Start is called before the first frame update
    void Start()
    {
        cacheData = null;
    }
}
