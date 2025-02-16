using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using DG.Tweening;
using UnityEditor;
using TMPro;

public class Level1Manager : MonoBehaviour
{
    public static Level1Manager instance;

    public List<GameObject> Walls;
    private int currentWallIndex;
    public ItemImageDatas imageDatas;
    [Header("遊戲控制")]
    public Button button_ToLeft;
    public Button button_ToRight;
    public Button button_Pause;
    public Button button_Back;
    public Button button_Quit;
    public Button button_Backpack;
    [Header("暫停面板")]
    public GameObject pausePanel;
    [Header("提示")]
    public GameObject hintPanel;
    public Button button_CloseHint;
    public TMP_Text hintText;
    [Header("背包")]
    public GameObject backPackObj;
    public GameObject backpackPanel;
    [Header("掛畫謎題")]
    public Color flower_White = new Color(255, 255, 255, 255);
    public Color flower_Red = new Color(231, 22, 0, 255);
    public Color flower_Blue = new Color(0, 140, 255, 255);
    public Color flower_Black = new Color(0, 0, 0, 255);
    public List<PaintingPuzzelImageData> paintingDatas;
    public List<PaintingPuzzel> paintingPuzzels;
    [Header("爬梯遊戲")]
    public GameObject graphic_LadderGame;
    [Header("滑塊遊戲")]
    public InteractableObjects button_OpenBlankMaze;
    public Button button_CloseBlankMaze;
    public GameObject BlankMazeObject;
    public List<GameObject> blankSlots;
    public List<blankSlotStateData> blankSlotDatas;
    private bool isMovingBlank;
    [Header("拼畫")]
    public GameObject bigPaintingObject;
    public Button button_CloseBigPainting;
    public GameObject fragmentPrefab;
    [Header("圖形鎖")]
    public InteractableObjects patternLockObject;
    public GameObject patternLockPanel;
    public Button button_ClosePatternLock;
    public List<DataBase.patternLockData> patternLockDatas;
    public GameObject wineCabinetDoor;
    [Header("電子鎖")]
    public InteractableObjects electricLockObject;
    public GameObject electricLockPanel;
    public List<Button> button_PadNumbers;
    public Button button_ConfirmPassword;
    public Button button_CloseElectricLock;
    public TMP_Text lockText;
    public GameObject foodBoxDoor;
    [Header("計時器")]
    public Text GameTime;
    public GameObject CountDownBG;
    public Image CountDown_Number;
    public GameObject CountDown_Start;
    int Time_Minute;
    int Time_Second;
    int CountDownTime = 5;
    public Sprite CountDown_3;
    public Sprite CountDown_2;
    public Sprite CountDown_1;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        BindEvents();
        currentWallIndex = 0;
        ChangeWall(0);
        InitPaintings();
        InitBlankMaze();

        Time_Second = 0;
        Time_Minute = 0;
        CountDownBG.SetActive(true);
        CountDown_Number.gameObject.SetActive(true);
        InvokeRepeating("CountDown", 0, 1);
    }

    private void BindEvents()
    {
        //暫停遊戲
        button_Pause.onClick.AddListener(delegate { ToggleGameStatus(0); });
        button_Back.onClick.AddListener(delegate { ToggleGameStatus(1); });
        button_Quit.onClick.AddListener(delegate { ToggleGameStatus(1); DataBuffer.instance.BackToMap(); });
        //切換牆面
        button_ToLeft.onClick.AddListener(delegate { ChangeWall(-1); });
        button_ToRight.onClick.AddListener(delegate { ChangeWall(1); });
        //開關背包
        button_Backpack.onClick.AddListener(delegate {ToggleBackPack(); });
        //翻轉掛畫
        foreach (var data in paintingPuzzels)
        {
            data.flipButton.SetPaintingData(data);
        }
        //關閉提示面板
        button_CloseHint.onClick.AddListener(delegate { hintPanel.SetActive(false); });
        //關閉滑塊遊戲面板
        button_CloseBlankMaze.onClick.AddListener(delegate { BlankMazeObject.SetActive(false); });
        //關閉拼畫面板
        button_CloseBigPainting.onClick.AddListener(delegate { bigPaintingObject.SetActive(false); });
        //關閉圖形鎖面板
        button_ClosePatternLock.onClick.AddListener(delegate { patternLockPanel.SetActive(false); });
        //初始化&綁定圖形鎖按鈕
        foreach (var data in patternLockDatas)
        {
            data.lockButton.onClick.AddListener(delegate 
            { 
                ChangePatternLockNumber(data);
                if (TaskCompleteCheck(DataBase.TaskIDs.level1_patternLock))
                {
                    StartCoroutine(PatternLockComplete());
                }
            });
        }
        //關閉電子鎖面板
        button_CloseElectricLock.onClick.AddListener(delegate { electricLockPanel.SetActive(false); });
        //綁定電子鎖按鈕
        for (int i = 0; i < button_PadNumbers.Count; i++)
        {
            int index = i;
            button_PadNumbers[i].onClick.AddListener(delegate { EnterElectricLockNumber(index); });
        }
        button_ConfirmPassword.onClick.AddListener(delegate 
        {
            if (TaskCompleteCheck(DataBase.TaskIDs.level1_electricLock))
            {
                StartCoroutine(ElectricLockComplete());
            }
        });
    }

    private void ToggleGameStatus(int timeValue)
    {
        pausePanel.SetActive(timeValue == 0);
        Time.timeScale = timeValue;
    }

    public void ChangeWall(int value)
    {
        currentWallIndex += value;

        if (currentWallIndex >= Walls.Count) currentWallIndex = 0;
        else if (currentWallIndex < 0) currentWallIndex = Walls.Count - 1;

        for (int i = 0; i < Walls.Count; i++)
        {
            if(i == currentWallIndex)
            {
                Walls[i].SetActive(true);
            }
            else
            {
                Walls[i].SetActive(false);
            }
        }
    }

    private void ToggleBackPack()
    {
        if (!backpackPanel.activeSelf)
        {
            backpackPanel.SetActive(true);
            button_Backpack.interactable = false;
            backPackObj.transform.DOLocalMoveY(-368, 0.7f).OnComplete(delegate { button_Backpack.interactable = true; });
        }
        else
        {
            button_Backpack.interactable = false;
            backPackObj.transform.DOLocalMoveY(-584, 0.7f).OnComplete(delegate { backpackPanel.SetActive(false); button_Backpack.interactable = true; });
        }
    }

    #region 掛畫謎題
    private void InitPaintings()
    {
        foreach (var data in paintingPuzzels)
        {
            SetPaintingImage(data);
        }
    }

    public void FlipPainting(PaintingPuzzel data)
    {
        foreach (var value in paintingPuzzels)
        {
            value.flipButton.isInteractable = false;
        }

        GameObject flipTarget = data.flipTarget;
        flipTarget.transform.DOLocalRotate(new Vector3(0, -90, 0), 0.3f).SetEase(Ease.Linear).OnComplete(
            delegate {
                //設定新圖片
                SetPaintingImage(data);
                //翻轉圖片
                flipTarget.transform.Rotate(0, 180, 0);
                flipTarget.transform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.Linear).OnComplete(
                    delegate {
                        if (TaskCompleteCheck(DataBase.TaskIDs.level1_paintingPuzzel) == false)
                        {
                            foreach (var value in paintingPuzzels)
                            {
                                value.flipButton.isInteractable = true;
                            }
                        }
                    });
            });
    }

    private void SetPaintingImage(PaintingPuzzel data)
    {
        PaintingPuzzelImageData imageData = GetPaintingImageData(data.colorDatas[data.currentDataIndex].flowerCount);
        data.targetImage.sprite = imageData.MainSprite;
        data.blossomImage.sprite = imageData.BlossomSprite;
        data.blossomImage.color = GetFlowerColor(data.colorDatas[data.currentDataIndex].targetColor);
    }

    private PaintingPuzzelImageData GetPaintingImageData(int flowerCount)
    {
        PaintingPuzzelImageData returnValue = new PaintingPuzzelImageData();

        foreach (var value in paintingDatas)
        {
            if (value.flowerCount == flowerCount)
            {
                returnValue = value;
            }
        }

        return returnValue;
    }

    private Color GetFlowerColor(DataBase.PaintingFlowerColor color)
    {
        Color returnValue = new Color();

        switch (color)
        {
            case DataBase.PaintingFlowerColor.white:
                returnValue = flower_White;
                break;
            case DataBase.PaintingFlowerColor.red:
                returnValue = flower_Red;
                break;
            case DataBase.PaintingFlowerColor.blue:
                returnValue = flower_Blue;
                break;
            case DataBase.PaintingFlowerColor.black:
                returnValue = flower_Black;
                break;
        }

        return returnValue;
    }

    private IEnumerator ShowHiddenCabinet()
    {
        yield return new WaitForSeconds(0.3f);

        foreach (var value in paintingPuzzels)
        {
            value.flipTarget.transform.DOLocalRotate(new Vector3(0, -90, 0), 0.3f).SetEase(Ease.Linear).OnComplete(
                delegate {
                    //顯示暗櫃
                    if (value.hiddenCabinet != null)
                    {
                        value.hiddenCabinet.transform.Rotate(0, 90, 0);
                        value.hiddenCabinet.SetActive(true);
                        value.hiddenCabinet.transform.DOLocalRotate(Vector3.zero, 0.3f).SetEase(Ease.Linear);
                    }
                });
        }
    }
    #endregion

    #region 移格遊戲
    public void InitBlankMaze()
    {
        for (int i = 0; i < blankSlots.Count; i++)
        {
            blankSlotStateData data = new blankSlotStateData();
            List<int> availableSlots = new List<int>();
            switch ((i+1)%4)
            {
                case 1:
                    availableSlots.Add(i + 1);
                    break;
                case 0:
                    availableSlots.Add(i - 1);
                    break;
                default:
                    availableSlots.Add(i + 1);
                    availableSlots.Add(i - 1);
                    break;
            }
            if (i - 4 >= 0)
            {
                availableSlots.Add(i - 4);
            }
            if (i + 4 < 12)
            {
                availableSlots.Add(i + 4);
            }

            data.index = i;
            data.currentBlank = blankSlots[i].GetComponentInChildren<InteractableObjects>();
            data.availableSlots = availableSlots;

            blankSlotDatas.Add(data);
        }
    }

    public void MoveBlank(InteractableObjects blank)
    {
        if (isMovingBlank)
        {
            return;
        }
        isMovingBlank = true;

        blankSlotStateData slotData = GetSlotData(blank);
        GameObject moveTarget = null;
        foreach(int slotIndex in slotData.availableSlots)
        {
            if (blankSlotDatas[slotIndex].currentBlank == null)
            {
                moveTarget = blankSlots[slotIndex];
                slotData.currentBlank = null;
                blankSlotDatas[slotIndex].currentBlank = blank;
                break;
            }
        }
        if(moveTarget != null)
        {
            blank.gameObject.transform.DOMove(moveTarget.transform.position, 0.5f, true).OnComplete(
                delegate
                {
                    if (TaskCompleteCheck(DataBase.TaskIDs.level1_blankMaze))
                    {
                        foreach (blankSlotStateData value in blankSlotDatas)
                        {
                            if (value.currentBlank != null)
                            {
                                value.currentBlank.isInteractable = false;
                            }
                        }
                        electricLockObject.isInteractable = true;

                        StartCoroutine(BlankMazeComplete());
                    }
                    blank.gameObject.transform.SetParent(moveTarget.transform, true);
                    isMovingBlank = false;
                });
        }
        else
        {
            blank.gameObject.transform.DOPunchPosition(Vector3.right * 20, 0.5f, 10).OnComplete(
                delegate
                {
                    isMovingBlank = false;
                });
        }
    }

    public blankSlotStateData GetSlotData(InteractableObjects blank)
    {
        foreach (var slot in blankSlotDatas)
        {
            if(slot.currentBlank == blank)
            {
                return slot;
            }
        }
        return null;
    }

    public void CheckBlankFillState()
    {
        bool allBlanksFilled = true;
        foreach (blankSlotStateData value in blankSlotDatas)
        {
            if (value.currentBlank != null && value.currentBlank.itemID != DataBase.ItemID.Blank_Normal)
            {
                if (value.currentBlank.type != DataBase.InteractType.Interact)
                {
                    allBlanksFilled = false;
                    break;
                }
            }
        }
        //格子都放好才改狀態
        if (allBlanksFilled)
        {
            foreach (blankSlotStateData value in blankSlotDatas)
            {
                if (value.currentBlank != null)
                {
                    value.currentBlank.isInteractable = true;
                }
            }
        }
    }

    public IEnumerator BlankMazeComplete()
    {
        yield return new WaitForSeconds(1);

        BlankMazeObject.gameObject.SetActive(false);
        button_OpenBlankMaze.isInteractable = false;

        yield return new WaitForSeconds(1);

        button_OpenBlankMaze.transform.DOMoveY(2.3f, 0.5f);
    }

    #endregion

    #region 圖形鎖

    public void ChangePatternLockNumber(DataBase.patternLockData lockData)
    {
        lockData.index = lockData.index == 9 ? 0 : lockData.index + 1;
        lockData.lockText.text = lockData.index.ToString();
    }

    public IEnumerator PatternLockComplete()
    {
        foreach (var data in patternLockDatas)
        {
            data.lockButton.interactable = false;
        }
        button_ClosePatternLock.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        patternLockPanel.gameObject.SetActive(false);
        wineCabinetDoor.gameObject.SetActive(false);
    }

    #endregion

    #region 電子鎖

    public void EnterElectricLockNumber(int number)
    {
        if (lockText.text.Length >= 4)
        {
            return;
        }

        lockText.text += number.ToString();
    }

    public IEnumerator ElectricLockComplete()
    {
        foreach (var data in button_PadNumbers)
        {
            data.interactable = false;
        }
        button_ConfirmPassword.interactable = false;
        button_CloseElectricLock.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        electricLockPanel.gameObject.SetActive(false);
        foodBoxDoor.gameObject.SetActive(false);
    }

    #endregion

    public bool TaskCompleteCheck(DataBase.TaskIDs task)
    {
        switch (task)
        {
            case DataBase.TaskIDs.level1_paintingPuzzel:
                //答案=4134
                string answer = "";
                foreach (var value in paintingPuzzels)
                {
                    answer += (value.currentDataIndex + 1).ToString();
                }

                if (answer == "4134")
                {
                    graphic_LadderGame.SetActive(true);
                    patternLockObject.isInteractable = true;
                    StartCoroutine(ShowHiddenCabinet());
                    return true;
                }
                else
                {
                    return false;
                }
            case DataBase.TaskIDs.level1_blankMaze:
                if (blankSlotDatas[4].currentBlank != null && blankSlotDatas[4].currentBlank.itemID == DataBase.ItemID.Blank_White &&
                    blankSlotDatas[5].currentBlank != null && blankSlotDatas[5].currentBlank.itemID == DataBase.ItemID.Blank_Red &&
                    blankSlotDatas[6].currentBlank != null && blankSlotDatas[6].currentBlank.itemID == DataBase.ItemID.Blank_Blue &&
                    blankSlotDatas[7].currentBlank != null && blankSlotDatas[7].currentBlank.itemID == DataBase.ItemID.Blank_Black)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            case DataBase.TaskIDs.level1_patternLock:
                string lockAnswer = "5350";
                string lockNumber = "";

                for (int i = 0; i < patternLockDatas.Count; i++)
                {
                    lockNumber += patternLockDatas[i].lockText.text;
                }

                return lockAnswer == lockNumber;
            case DataBase.TaskIDs.level1_electricLock:
                if (lockText.text == "3574")
                {
                    return true;
                }
                else
                {
                    lockText.text = "";
                    return false;
                }
            default:
                return false;
        }
    }
    public Sprite GetItemImage(DataBase.ItemID id, bool isMiniSprite)
    {
        foreach (var data in imageDatas.imageDatas)
        {
            if (data.itemID == id)
            {
                if (isMiniSprite && data.itemSprite_Mini != null)
                {
                    return data.itemSprite_Mini;
                }
                else
                {
                    return data.itemSprite;
                }
            }
        }

        return null;
    }
    public void CountDown()
    {
        CountDownTime--;
        switch (CountDownTime)
        {
            case 4:CountDown_Number.sprite = CountDown_3;break;
            case 3:CountDown_Number.sprite = CountDown_2;break;
            case 2:CountDown_Number.sprite = CountDown_1;break;
            case 1:
                {
                    CountDown_Number.sprite = CountDown_3;
                    CountDown_Number.gameObject.SetActive(false);
                    CountDown_Start.SetActive(true);
                }
                break;
            case 0:
                {
                    CountDownBG.SetActive(false);
                    CountDown_Start.SetActive(false);
                    CancelInvoke("CountDown");
                    InvokeRepeating("Timer", 0, 1);
                }
                break;
        }
    }
    public void Timer()
    {
        string minute = "";
        string second = "";
        Time_Second++;
        if (Time_Second == 60)
        {
            Time_Second = 0;
            Time_Minute++;
        }
        if (Time_Minute < 10) minute = "0" + Time_Minute;
        else minute = Time_Minute.ToString();
        if (Time_Second < 10) second = "0" + Time_Second;
        else second = Time_Second.ToString();
        GameTime.text = minute + ":" + second;
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void BackToMap()
    {
        SceneManager.LoadScene(1);
    }
}

//掛畫謎題結構
[Serializable]
public struct PaintingPuzzelImageData
{
    public int flowerCount;
    public Sprite MainSprite;
    public Sprite BlossomSprite;
}

[Serializable]
public class PaintingPuzzel
{
    public InteractableObjects flipButton;
    public GameObject flipTarget;
    public SpriteRenderer targetImage;
    public SpriteRenderer blossomImage;
    public int currentDataIndex;
    public GameObject hiddenCabinet;
    public List<paintingColorDatas> colorDatas;
}
[Serializable]
public struct paintingColorDatas
{
    public int flowerCount;
    public DataBase.PaintingFlowerColor targetColor;
}

//滑塊遊戲結構
[Serializable]
public class blankSlotStateData
{
    public int index;
    public InteractableObjects currentBlank;
    public List<int> availableSlots;
}