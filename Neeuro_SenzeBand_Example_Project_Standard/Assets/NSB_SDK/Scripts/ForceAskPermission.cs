using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif
using System;
using UnityEngine.Events;
using System.Linq;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles required ANDROID/iOS permissions for app use (mainly Bluetooth permissions)
/// </summary>
public class ForceAskPermission : MonoBehaviour
{
    public static ForceAskPermission instance = null;

    [SerializeField]
    private UnityEvent actionAfterAskAllPermission;

    [SerializeField]
    private GameObject enablePermissionsPopup_Android;
    [SerializeField]
    private GameObject enablePermissionsPopup_IOS;

    private Coroutine requestAllPermissionsCoroutine;
    private bool requestsOngoing;
    private bool goNextRequest;

    //temporary
    [SerializeField]
    private GameObject[] NSB_Objs;

#if UNITY_ANDROID && !UNITY_EDITOR
    List<bool> permissions = new List<bool>() { false, false, false, false, false, false };
    List<bool> permissionsAsked = new List<bool>() { false, false, false, false, false, false };
    List<Action> actions = new List<Action>();
#endif

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        RequestAllPermissions();
    }

    private void RequestAllPermissions()
    {
        if (requestAllPermissionsCoroutine != null)
            StopCoroutine(requestAllPermissionsCoroutine);
        requestAllPermissionsCoroutine = StartCoroutine(RequestAllPermissions_IEnum());
    }
    private IEnumerator RequestAllPermissions_IEnum()
    {
        requestsOngoing = true;
#if UNITY_ANDROID && !UNITY_EDITOR
        permissions = new List<bool>() { false, false, false, false, false };
        permissionsAsked = new List<bool>() { false, false, false, false, false };
        actions = new List<Action>()
    {
        new Action(() => {
            permissions[0] = Permission.HasUserAuthorizedPermission("android.permission.ACCESS_FINE_LOCATION");
            if (!permissions[0] && !permissionsAsked[0])
            {
                Permission.RequestUserPermission("android.permission.ACCESS_FINE_LOCATION");
                permissionsAsked[0] = true;
                return;
            }
        }),
        new Action(() => {
            permissions[1] = Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADMIN");
            if (!permissions[1] && !permissionsAsked[1])
            {
                Permission.RequestUserPermission("android.permission.BLUETOOTH_ADMIN");
                permissionsAsked[1] = true;
                return;
            }
        }),
        new Action(() => {
            permissions[2] = Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH");
            if (!permissions[2] && !permissionsAsked[2])
            {
                Permission.RequestUserPermission("android.permission.BLUETOOTH");
                permissionsAsked[2] = true;
                return;
            }
        }),
        new Action(() => {
            permissions[3] = Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN");
            if (!permissions[3] && !permissionsAsked[3])
            {
                Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");
                permissionsAsked[3] = true;
                return;
            }
        }),
        new Action(() => {
            permissions[4] = Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT");
            if (!permissions[4] && !permissionsAsked[4])
            {
                Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");
                permissionsAsked[4] = true;
                return;
            }
        })
    };
        int ctr = 0;
        for (int i = 0; i < permissionsAsked.Count;)
        {
            Debug.LogError("Permission Ask: " + i);
            if (getSDKInt() <= 30)
            {
                if (i == 1 || i == 3 || i == 4)
                {
                    permissions[i] = true;
                    ++i;
                    continue;
                }
            }
            else
            {
                //skip all other bluetooth request permissions if location permission was denied
                if (i > 0 && !permissions[0])
                {
                    permissions[i] = false;
                    permissionsAsked[i] = true;
                    ++i;
                    continue;
                }
            }

            actions[i].Invoke();
            ctr++;

            if (!minimized && ctr >= 5) //if not minimized after action invoke, it means that no permission popup appeared
            {
                goNextRequest = true;
            }


            if (goNextRequest || permissions[i])
            {
                ctr = 0;
                ++i;
                goNextRequest = false;
            }
            yield return new WaitForEndOfFrame();
        }

        if (Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADMIN"))
        {
            setBluetoothEnabled();
        }

        if(!PermissionCheck())
        {
            enablePermissionsPopup_Android.SetActive(true);
            yield break;
        }
#elif UNITY_IOS && !UNITY_EDITOR
        yield return new WaitForSeconds(0.2f);
        for(int i = 0; i<NSB_Objs.Length; i++)
        {
            NSB_Objs[i].SetActive(true);
        }
        yield return new WaitUntil(() => NSB_Manager.instance != null);
        yield return new WaitForSeconds(0.2f);
        if (!PermissionCheck())
        {
            enablePermissionsPopup_IOS.SetActive(true);

            yield break;
        }
#endif
        requestsOngoing = false;

        yield return null;
        actionAfterAskAllPermission.Invoke();
    }

    /// <summary>
    /// Determines if all required permissions for ANDROID or IOS are authorized by user
    /// </summary>
    /// <returns>TRUE if all required permissions are allowed by user (mainly bluetooth permissions) so user can continue using Bluetooth functionalities like SB connection</returns>
    public bool PermissionCheck()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bool allPermissionsEnabled = true;
        for (int i = 0; i < permissionsAsked.Count;)
        {
            if (getSDKInt() <= 30)
            {
                if (i == 1 || i == 3 || i == 4)
                {
                    permissions[i] = true;
                    ++i;
                    continue;
                }
            }

            actions[i].Invoke();
            if (permissions[i])
            {
                ++i;
            }
            else
            {
                allPermissionsEnabled = false;
                break;
            }
        }
        
        return allPermissionsEnabled;
#elif UNITY_IOS && !UNITY_EDITOR
        return NSB_Manager.instance.BTStatusFetched;

#else
        return true;
#endif
    }

    bool minimized = false;
    private void OnApplicationFocus(bool focus)
    {
        minimized = !focus;
        if (focus)
        {
            if (requestsOngoing)
                goNextRequest = true;
        }
    }

#if UNITY_ANDROID
    public void setBluetoothEnabled()
    {
        using (AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"))
        {
            try
            {
                using (var BluetoothManager = activity.Call<AndroidJavaObject>("getSystemService", "bluetooth"))
                {
                    using (var BluetoothAdapter = BluetoothManager.Call<AndroidJavaObject>("getAdapter"))
                    {
                        BluetoothAdapter.Call<bool>("enable");
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }

    static int getSDKInt()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
    }
#endif
}
