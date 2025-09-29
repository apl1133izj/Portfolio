using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class WorldCreated : MonoBehaviour
{
    public GameObject[] lobyWindow; //0:StartWindow 1:CharacterWindow 2:SELECT DIFFICULTYWindow 3:LoadWindow

    [Header("캐릭터 생성")]
    public InputField inputCharacterName;
    public Image[] selectCharacterImage;
    public GameObject[] selectCharacter;
    public GameObject inputCharacterWindow;
    public string[] name;
    public string[] jobType;
    public string jobPriview;
    public Text[] nameText;
    public Text[] jobText;
    public Text jobPriviewtext;
    public int inputCharacterNum;
    public GameObject destoryCharacterWindow;
    int deledtNum;
    [Header("캐릭터 로드")]
    public GameObject[] loadMapGameObject;
    public Text[] difficultyText;
    public Text[] dateText;
    public Text[] playerTimeText;
    public Text[] lastPlayerTimeText;
    public string[] difficultyLoad;
    public string[] dateLoad;
    public string[] playerTimeLoad;
    public string[] lastPlayerTimeLoad;
    int loadstate;
    public bool[] loadBool;
    [Header("난이도 설정")]
    public string[] difficulty;
    public GameObject sELECTDIFFICULTYWindow;
    public bool[] difficultyBool;
    bool workingButtonBool;
    [Header("오류 메세지")]
    public GameObject errorMessageWindow;
    public Text errorMessageText;
    public GameObject previewFolder;
    public int worldCount;



    //게임 시작버튼 
    public void StartWindowButton(bool state)
    {
        lobyWindow[1].SetActive(state);
    }

    /************************************************************CharacterWindowStart************************************************************/
    //캐릭터 생성 창 비활성화
    public void InputCharacterWindow(bool state)
    {
        inputCharacterWindow.gameObject.SetActive(state);
    }
    public void InputCharacterNum(int num)
    {
        inputCharacterNum = num;
        SelectColor(inputCharacterNum);
    }
    //이름한글자 이상,난이도 선택 확인
    //월드 이름-생성시간-난이도
    public void InputWorldSetting()
    {
        if (string.IsNullOrEmpty(inputCharacterName.text))
        {
            errorMessageText.text = "이름을 입력하세요.";
            errorMessageWindow.SetActive(true);
        }
        else if (jobPriview.Length < 0)
        {
            errorMessageText.text = "직업을 선택하세요.";
            errorMessageWindow.SetActive(true);
        }
        if (!string.IsNullOrEmpty(inputCharacterName.text) && jobPriview.Length > 0)
        {
            name[inputCharacterNum] = inputCharacterName.text;
            nameText[inputCharacterNum].text = name[inputCharacterNum];
            selectCharacter[inputCharacterNum].SetActive(true);

        }
    }
    public void SelectColor(int state)
    { 
        for (int i = 0; i < selectCharacterImage.Length; i++)
        {
            Color32 color32 = selectCharacterImage[i].color;
            if (state == i)
            {
                color32 = new Color32(0, 168, 255, 255);
                selectCharacterImage[i].color = color32;
            }
            else
            {
                color32 = new Color32(255, 255, 255, 255);
                selectCharacterImage[i].color = color32;
            }
        }


    }
    public void NewGameButton()
    {
        if (selectCharacter[inputCharacterNum].activeSelf)
        {
            sELECTDIFFICULTYWindow.SetActive(true);
        }
        else
        {
            errorMessageText.text = "캐릭터 를 선택하세요";
            errorMessageWindow.SetActive(true);
        }
    }
    public void DeleteChanracter()
    {
        selectCharacter[deledtNum].gameObject.SetActive(false);
        name[deledtNum] = "";
        jobPriview = "";
        Color32 color32 = selectCharacterImage[inputCharacterNum].color;
        color32 = new Color32(255, 255, 255, 255);
        selectCharacterImage[inputCharacterNum].color = color32;
    }
    public void JonSelectButton(int _jobNum)
    {
        jobPriview = jobType[_jobNum];
        jobText[inputCharacterNum].text = jobType[_jobNum];
    }
    public void DeledtNum(int _deledtNum)
    {
        deledtNum = _deledtNum;
    }
    public void DestoryCharacterWindow(bool state)
    {
        destoryCharacterWindow.gameObject.SetActive(state);
    }
    /************************************************************CharacterWindowEnd************************************************************/
    /*난이도*/
    public void DifficultyButton(int difficultyNum)
    {
        loadBool[inputCharacterNum] = true;
        DateTime now = DateTime.Now;
        string currentTime = now.ToString("yyyy-MM-dd HH:mm:ss");
        dateLoad[inputCharacterNum] = currentTime;
        loadMapGameObject[inputCharacterNum].SetActive(true);
        difficultyBool[difficultyNum] = true;
    }

    public void GameStartButton_MoveScene()
    {
        bool isDifficultySelected = false;

        for (int i = 0; i < difficultyBool.Length; i++)
        {
            if (difficulty[i].Length > 0)
            {
                isDifficultySelected = true;
                break; // 하나라도 true면 더 확인할 필요 없음
            }
        }

        if (isDifficultySelected)
        {
            GameManager.GetGameManager.inGame = true;
            DataManager.GetDataManager.playerName = name[inputCharacterNum];
            SceneManager.LoadScene("World");
        }
        else
        {
            errorMessageText.text = "난이도를 선택하세요.";
            errorMessageWindow.SetActive(true);
        }
    }
    public void LoadWinDowButton(bool state)
    {
        Debug.Log("?");
        lobyWindow[3].SetActive(state);
    }
    public void LoadGameStartButton_MoveScene()
    {
        if (loadBool[loadstate])
        {
            GameManager.GetGameManager.inGame = true;
            DataManager.GetDataManager.playerName = name[inputCharacterNum];
            SceneManager.LoadScene("World");
        }
        else
        {
            errorMessageText.text = "데이터를 선택하세요";
            errorMessageWindow.SetActive(true);
        }
    }
    public void NotDataMessageButton()
    {
        errorMessageText.text = "저장된 데이터가 없습니다";
        errorMessageWindow.SetActive(true);
    }
    public void LoadGameStartNumButton(int state)
    {
        loadBool[state] = true;
        loadstate = state;
    }
    //작업 중입니다 메세지창
    public void WorkingWindow()
    {
        workingButtonBool = !workingButtonBool;
    }
    //난이도
    public void Difficulty(string difficultys)
    {
        difficulty[inputCharacterNum] = difficultys;
    }
    public void ErrorMessageWindowFalse()
    {
        jobPriview = "";
        errorMessageWindow.SetActive(false);
    }

}
