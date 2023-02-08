using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace Util
{
    public class MainMenuManager : MonoBehaviour
    {
        public TextMeshProUGUI fullScrenText;
        public TextMeshProUGUI resolutionText;
        public TextMeshProUGUI fpsText;

        public Resolution[] resolutions;
        public int[] fpsList;

        private Resolution menuResolution;
        private int numberResolution;
        private int menuFPS;
        private bool fullscren;

        private void Start() {
            GetPlayerOptions();
            SetMaxFrameRate(fpsList[menuFPS]);
        }

        public void GetPlayerOptions()
        {
            // ------------------------------- FullScreen -------------------------------------------
            if (!PlayerPrefs.HasKey("FullScreen")) PlayerPrefs.SetInt("FullScreen", 1);
            fullscren = PlayerPrefs.GetInt("FullScreen") == 1 ? true : false;
            SetFullScreen(fullscren);

            // ------------------------------- Resolution -------------------------------------------
            resolutions = Screen.resolutions;
            int actualResolution = -1;
            int hdResolution = -1;
            int iteration = 0;


            foreach (Resolution r in resolutions)
            {
                if(r.Equals(Screen.currentResolution))
                {
                    actualResolution = iteration;
                }
                else if(r.width == 1920 && r.height == 1080)//Else if perque ens es igual perque la de per defecte te priorita. aqyesta es basicament per si no hi ha valor per defecte basic
                {
                    hdResolution = iteration;
                }

                iteration++;
            }

            if (!PlayerPrefs.HasKey("Resolution"))
            {
                int intToSet = 0;
                if (actualResolution != -1) intToSet = actualResolution;
                else if (hdResolution != -1) intToSet = hdResolution;
                PlayerPrefs.SetInt("Resolution", intToSet);
            }

            numberResolution = PlayerPrefs.GetInt("Resolution");
            menuResolution = resolutions[numberResolution];
            Screen.SetResolution(menuResolution.width, menuResolution.height, fullscren);
            resolutionText.text = menuResolution.width.ToString() + "X" + menuResolution.height.ToString();


            // ------------------------------- FPS -------------------------------------------
            // PlayerPrefs.SetInt("FPS", 1);
            if (!PlayerPrefs.HasKey("FPS")) PlayerPrefs.SetInt("FPS", 1);
            menuFPS = PlayerPrefs.GetInt("FPS");
            fpsText.text = fpsList[menuFPS].ToString();

        }

        public void NetxInFPS()
        {
            menuFPS++;
            if (menuFPS >= fpsList.Length) menuFPS = 0;
            fpsText.text = fpsList[menuFPS].ToString();
        }

        public void NextInResolution()
        {
            numberResolution++;
            if (numberResolution >= resolutions.Length) numberResolution = 0;
            menuResolution = resolutions[numberResolution];
            resolutionText.text = menuResolution.width + "X" + menuResolution.height;

        }

        public void AplyChanges()
        {
            Screen.SetResolution(menuResolution.width, menuResolution.height, fullscren);
            PlayerPrefs.SetInt("Resolution", numberResolution);
            SetMaxFrameRate(fpsList[menuFPS]);
            PlayerPrefs.SetInt("FPS", menuFPS);
            SetFullScreen(fullscren);
            PlayerPrefs.SetInt("FullScreen", fullscren ? 1 : 0);
        }


        public void goToScene(string nexScene)
        {
            SceneManager.LoadScene(nexScene);

        }

        public void ExitGame()
        {
            Application.Quit();
        }

        public void SetFullScreen()
        {
            fullscren = !fullscren;
            fullScrenText.text = fullscren ? "ON" : "OFF";
        }

        public void SetFullScreen(bool input)
        {
            Screen.fullScreen = input;
            fullScrenText.text = Screen.fullScreen ? "ON" : "OFF";
        }

        public void SetResolution()
        {
            Screen.SetResolution(5,5,false);
        }

        public void SetMaxFrameRate(int f)
        {
            Debug.Log("Set frame rate: "+f);
            Application.targetFrameRate = f;
        }

    }
}
