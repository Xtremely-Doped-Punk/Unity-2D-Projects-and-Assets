using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public class GameModes : MonoBehaviour
{
    [Header("Init Mode-Config")]
    [SerializeField] List<CinemachineVirtualCamera> LightModeCams = new();
    [SerializeField] List<Light> LightModeLights = new();
    [SerializeField] List<CinemachineVirtualCamera> DarkModeCams = new();
    [Tooltip("add the spot-light that follows the player in first index")]
    [SerializeField] List<Light> DarkModeLights = new();

    public enum Modes { Light, Dark }
    private Modes _mode; // public mode to switch
    public Modes mode; // private version of mode
    private Rocket Player;
    private readonly int[] view_idx = new int[2];

    /*[Tooltip("dont add any reference to this item directly, it will automatically find the invisible blocks in runtime")]
    [SerializeField]*/
    readonly List<MeshRenderer> ForthWalls = new();

    [Header("UI Mode-Config")]
    [SerializeField] Button ModeButtonUI;
    [SerializeField] Sprite LightModeBtnSprite;
    [SerializeField] Sprite DarkModeBtnSprite;
    void SetPlayerControls()
    {
        if (_mode == Modes.Light)
            Player.InvertCtrls = false;
        else if (_mode == Modes.Dark)
            Player.InvertCtrls = true;
    }

    void Toggle4thWall(bool state)
    {
        foreach (var wall in ForthWalls)
        {
            wall.enabled = state;
        }
    }

    void SetLightings()
    {
        if (_mode == Modes.Light)
        {
            foreach(var light in LightModeLights)
            {
                light.enabled = true;
            }
            foreach (var light in DarkModeLights)
            {
                light.enabled = false;
            }
        }
        else if (_mode == Modes.Dark)
        {
            foreach (var light in LightModeLights)
            {
                light.enabled = false;
            }
            foreach (var light in DarkModeLights)
            {
                light.enabled = true;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // init
        CheckInvisibleWalls();
        _mode = mode;
        Player = Rocket.GetInstance();
        
        // setups
        CameraSetup();
        SetPlayerControls();
        SetLightings();
        SetButtonSprites();
    }

    private void SetButtonSprites()
    {
        if (_mode == Modes.Light)
            ModeButtonUI.image.sprite = LightModeBtnSprite;
        else if (_mode == Modes.Dark)
            ModeButtonUI.image.sprite = DarkModeBtnSprite;
    }

    private void CameraSetup()
    {
        int cam_prior = int.MinValue;

        for (int i = 0; i < LightModeCams.Count; i++)
        {
            if (LightModeCams[i].Priority > cam_prior)
            {
                cam_prior = LightModeCams[i].Priority;
                view_idx[(int)Modes.Light] = i;
            }
            LightModeCams[i].Priority = 0;
            LightModeCams[i].Follow = Player.transform;
            LightModeCams[i].LookAt = Player.transform;
        }
        if (_mode == Modes.Light)
        {
            LightModeCams[view_idx[(int)Modes.Light]].Priority = 1;
            Toggle4thWall(false);
        }

        for (int i = 0; i < DarkModeCams.Count; i++)
        {
            if (DarkModeCams[i].Priority > cam_prior)
            {
                cam_prior = DarkModeCams[i].Priority;
                view_idx[(int)Modes.Dark] = i;
            }
            DarkModeCams[i].Priority = 0;
            DarkModeCams[i].Follow = Player.transform;
            DarkModeCams[i].LookAt = Player.transform;
        }
        if (_mode == Modes.Dark)
        {
            DarkModeCams[view_idx[(int)Modes.Dark]].Priority = 1;
            Toggle4thWall(true);
        }
    }

    private void CheckInvisibleWalls()
    {
        var walls_inactive = GameObject.FindGameObjectsWithTag("Warning"); // add the terrain with tag as 'Warning'
        foreach (var wall in walls_inactive)
        {
            wall.TryGetComponent<MeshRenderer>(out var mesh_comp);
            if ((mesh_comp != null) && (!mesh_comp.enabled))
                ForthWalls.Add(mesh_comp);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_mode != mode)
        {
            SwitchModes();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (_mode == Modes.Light)
            {
                LightModeCams[view_idx[(int)Modes.Light]].Priority = 0;
                view_idx[(int)Modes.Light] = ++view_idx[(int)Modes.Light] % LightModeCams.Count;
                LightModeCams[view_idx[(int)Modes.Light]].Priority = 1;
            }
            else if (_mode == Modes.Dark)
            {
                DarkModeCams[view_idx[(int)Modes.Dark]].Priority = 0;
                view_idx[(int)Modes.Dark] = ++view_idx[(int)Modes.Dark] % DarkModeCams.Count;
                DarkModeCams[view_idx[(int)Modes.Dark]].Priority = 1;
            }

        }

        // make the spot-light follow the camera pov
        DarkModeLights[0].transform.SetPositionAndRotation
            (DarkModeCams[view_idx[(int)Modes.Dark]].transform.position,
            DarkModeCams[view_idx[(int)Modes.Dark]].transform.rotation);
    }

    private void SwitchModes()
    {
        if (_mode == Modes.Light)
        {
            LightModeCams[view_idx[(int)Modes.Light]].Priority = 0;
        }
        else if (_mode == Modes.Dark)
        {
            DarkModeCams[view_idx[(int)Modes.Dark]].Priority = 0;
        }
        _mode = mode;
        SetPlayerControls(); SetLightings(); SetButtonSprites();
        if (_mode == Modes.Light)
        {
            LightModeCams[view_idx[(int)Modes.Light]].Priority = 1;
            Toggle4thWall(false);
        }
        else if (_mode == Modes.Dark)
        {
            DarkModeCams[view_idx[(int)Modes.Dark]].Priority = 1;
            Toggle4thWall(true);
        }
    }
    public void SwitchModeFn()
    {
        if (_mode == mode)
        {
            if (mode == Modes.Light)
                mode = Modes.Dark;
            else if (mode == Modes.Dark)
                mode = Modes.Light;
        }
    }
}
