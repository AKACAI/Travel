using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using System;

public class MoeVersionManager : MoeSingleton<MoeVersionManager>
{
    const string REMOTE_URL = "这里改成自己的CDN域名或IP";
    static string VERSION_FILE_DIR;
    static string VERSION_FILE_PATH;
    static string IN_VERSION_FILE_PATH;

    private MoeVersionInfo currVersionInfo = null;
    private MoeVersionInfo remoteVersionInfo = null;
    private UpdateInfo updateInfo = null;

    private static OnVersionStateParam versionStateParam = new OnVersionStateParam();
    private static OnUpdateProgressParam updateProgressParam = new OnUpdateProgressParam();
    private static OnVersionMsgBoxParam msgBoxParam = new OnVersionMsgBoxParam();

    private enum EnProcessType
    {
        Normal,
        Fix,
    }

    private Action<EnProcessType> actionTryUnCompress = null;
    private Action<EnProcessType> actionUpdateVersionFile = null;
    private Action<EnProcessType> actionUpdateBundles = null;
    private Action<EnProcessType> actionCheckAssets = null;
    private Action<EnProcessType> actionForceUpdateVersionFile = null;



    protected override void InitOnCreate()
    {
        VERSION_FILE_DIR = Application.persistentDataPath + "/Bundles/";
        VERSION_FILE_PATH = Application.persistentDataPath + "/Bundles/version.json";
        IN_VERSION_FILE_PATH = Application.streamingAssetsPath + "/Bundles/version.json";
        Debug.LogFormat("{0}", VERSION_FILE_PATH);
        InitProcessChain();
        StartNormalProcess();
    }


    private void InitProcessChain()
    {
        this.actionTryUnCompress = (EnProcessType param) =>
        {
            Debug.LogFormat("Action>>> 解压: {0}", param);
            this.currVersionInfo = LoadVersionInfo(VERSION_FILE_PATH);
            // if (!CheckBundleCorrect())
            if (currVersionInfo == null)
            {
                UpdateUIState("正在解压资源");
                UnCompressBundle();
                this.currVersionInfo = LoadVersionInfo(VERSION_FILE_PATH);
            }
            else
            {
                // 判断是不是更新包，也就是StreamingAssets里的版本是否比Persistent版本高，如果高的话，再次解压Bundle
                MoeVersionInfo inVersionInfo = LoadVersionInfo(IN_VERSION_FILE_PATH);
                if (inVersionInfo != null)
                {
                    int[] inVersionDigit = inVersionInfo.GetVersionDigitArray();
                    int[] currVersionDigit = this.currVersionInfo.GetVersionDigitArray();
                    // if (inVersionInfo.GetVersionLong() > this.currVersionInfo.GetVersionLong())
                    if (inVersionDigit[0] > currVersionDigit[0] ||
                       inVersionDigit[1] > currVersionDigit[1] ||
                       inVersionDigit[2] > currVersionDigit[2])
                    {
                        // 包里的版本比Persistent的版本高，可能玩家进行了大版本更新，重新解压
                        Debug.LogFormat("包里的Bundle版本 > Persistent Bundle 版本，重新解压");
                        UpdateUIState("正在解压资源");
                        UnCompressBundle();
                        this.currVersionInfo = LoadVersionInfo(VERSION_FILE_PATH);
                    }
                    else
                    {
                        Debug.LogFormat("包里Bundle版本 <= Persistent Bundle版本，无需解压~");
                    }
                }
                else
                {
                    Debug.LogErrorFormat("逻辑错误，从StreamingAssets 中加载VersionInfo文件失败");
                }
            }
        };

        this.actionUpdateVersionFile = (EnProcessType param) =>
        {
            Debug.LogFormat("Action>>> 获取远程版本文件: {0}", param);
            StartCoroutine(TryUpdateVersion((bool ok, bool majorUpdate) =>
            {
                if (ok)
                {
                    if (majorUpdate)
                    {
                        // 调用商店
                        OnMsgBox("新的大版本已更新，请下载最新安装包！", "确定", () =>
                        {
                            JumpToDownloadMarket();
                        });
                    }
                    else
                    {
                        // 成功了，接下来更新Bundle
                        this.actionUpdateBundles?.Invoke(param);
                    }
                }
                else
                {
                    // 版本文件更新失败，弹窗询问
                    OnMsgBox("版本信息获取失败，请检查网络连接！", "重试", () =>
                    {
                        this.actionUpdateVersionFile?.Invoke(param);
                    });
                }
            }));
        };

        this.actionForceUpdateVersionFile = (EnProcessType param) =>
        {
            Debug.LogFormat("Action>>> 强制获取远程版本文件: {0}", param);
            TryDeleteBundleDir();
            TryCreateBundleDir();
            StartCoroutine(TryUpdateVersion((bool ok, bool majorUpdate) =>
            {
                if (ok)
                {
                    if (majorUpdate)
                    {
                        // 调用商店
                        OnMsgBox("新的大版本已更新，请下载最新安装包！", "确定", () =>
                        {
                            JumpToDownloadMarket();
                        });
                    }
                    else
                    {
                        // 成功了，接下来更新Bundle
                        this.actionUpdateBundles?.Invoke(param);
                    }
                }
                else
                {
                    // 版本文件更新失败，弹窗询问
                    OnMsgBox("版本信息获取失败，请检查网络连接！", "重试", () =>
                    {
                        this.actionForceUpdateVersionFile?.Invoke(param);
                    });
                }
            }, true));
        };

        this.actionUpdateBundles = (EnProcessType param) =>
        {
            Debug.LogFormat("Action>>> 更新Bundle: {0}", param);
            StartCoroutine(TryUpdateBundle((bool ok) =>
            {
                if (ok)
                {
                    // 成功了，接下来检查资源，
                    this.actionCheckAssets?.Invoke(param);
                }
                else
                {
                    OnMsgBox("资源下载失败，请检查网络连接！", "重试", () =>
                    {
                        this.actionUpdateBundles(param);
                    });
                }
            }));
        };

        this.actionCheckAssets = (EnProcessType param) =>
        {
            Debug.LogFormat("Action>>> 校对资源: {0}", param);
            if (!CheckBundleCorrect())
            {
                // 更新完了，本地Bundle还是不对
                Debug.LogFormat("更新完Bundle后，发现文件不对");
                if (param == EnProcessType.Normal)
                {
                    OnMsgBox("资源有错误，请修复客户端！", "修复", () =>
                    {
                        this.actionForceUpdateVersionFile?.Invoke(EnProcessType.Fix);
                    });
                }
                else
                {
                    OnMsgBox("客户端修复失败，请重新下载安装包！", "确定", () =>
                    {
                        JumpToDownloadMarket();
                    });
                }
            }
            else
            {
                UpdateUIState("进入游戏");
                MoeEventManager.Inst.SendEvent(EventID.Event_OnUpdateEnd);
            }
        };
    }



    // 跳转到下载商店
    private void JumpToDownloadMarket()
    {
        Application.OpenURL("https://taptap.com");
    }

    private void StartNormalProcess()
    {
        TryCreateBundleDir();
        this.actionTryUnCompress?.Invoke(EnProcessType.Normal);
        this.actionUpdateVersionFile?.Invoke(EnProcessType.Normal);
    }

    private void StartFixProcess()
    {
        this.actionForceUpdateVersionFile(EnProcessType.Fix);
    }


    private MoeVersionInfo LoadVersionInfo(string path)
    {
        Debug.LogFormat("加载 Version 文件: {0}", path);
        try
        {
            if (System.IO.File.Exists(path))
            {
                string text = System.IO.File.ReadAllText(path);
                if (!string.IsNullOrEmpty(text))
                {
                    MoeVersionInfo vInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MoeVersionInfo>(text);
                    if (vInfo != null)
                    {
                        Debug.LogFormat("Version 信息加载成功: {0}", vInfo.version);
                        return vInfo;
                    }
                }
                else
                {
                    Debug.LogFormat("Version 文件内容为空");
                }
            }
            else
            {
                Debug.LogFormat("Version 文件不存在");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogErrorFormat("读取Version文件出错: {0}", e.ToString());
        }

        return null;
    }

    /// <summary>
    /// 从 StreamingAssets 里将Bundle拷贝到 Persistent 目录里 
    /// </summary>
    private void UnCompressBundle()
    {
        TryDeleteBundleDir();
        TryCreateBundleDir();
        Debug.LogFormat("尝试从 Steaming 拷贝Bundle 到 Persistent");
        try
        {
            if (System.IO.File.Exists(IN_VERSION_FILE_PATH))
            {
                string text = System.IO.File.ReadAllText(IN_VERSION_FILE_PATH);
                Debug.LogFormat("Text: {0}", text);
                MoeVersionInfo inVersionInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MoeVersionInfo>(text);
                if (inVersionInfo != null)
                {
                    // 拷贝 Bundle
                    foreach (VersionBundleInfo bundleInfo in inVersionInfo.bundles)
                    {
                        string srcFilePath = string.Format("{0}/{1}", Application.streamingAssetsPath, bundleInfo.bundle_name);
                        string destFilePath = string.Format("{0}/{1}", Application.persistentDataPath, bundleInfo.bundle_name);
                        Debug.LogFormat("拷贝Bundle， {0} -> {1}", srcFilePath, destFilePath);
                        System.IO.File.Copy(srcFilePath, destFilePath, true);
                    }

                    // 拷贝 Version文件
                    System.IO.File.Copy(IN_VERSION_FILE_PATH, VERSION_FILE_PATH, true);
                }
            }
            else
            {
                Debug.LogErrorFormat("解压失败，StreamingAssets 中没有 Version 文件");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogErrorFormat("Bundle拷贝出错! {0}", e.ToString());
        }
    }

    public void TryCreateBundleDir()
    {
        if (!System.IO.Directory.Exists(VERSION_FILE_DIR))
        {
            Debug.LogFormat("创建 Persistent Bundle 目录");
            System.IO.Directory.CreateDirectory(VERSION_FILE_DIR);
        }
        else
        {
            Debug.LogFormat("Persistent Bundle 目录已存在，不需要创建");
        }
    }

    public void TryDeleteBundleDir()
    {
        if (System.IO.Directory.Exists(VERSION_FILE_DIR))
        {
            System.IO.Directory.Delete(VERSION_FILE_DIR, true);
        }
    }

    private string GetLocalBundleMD5(string bundle_name)
    {
        string bundleFilePath = string.Format("{0}/{1}", Application.persistentDataPath, bundle_name);
        if (System.IO.File.Exists(bundleFilePath))
        {
            string md5 = MoeUtils.GetMD5HashFromFile(bundleFilePath);
            return md5;
        }

        return null;
    }

    /// <summary>
    /// 检查当前的Bundle是否正确 
    /// </summary>
    /// <returns></returns>
    public bool CheckBundleCorrect()
    {
        if (currVersionInfo != null)
        {
            foreach (VersionBundleInfo bundleInfo in currVersionInfo.bundles)
            {
                string bundleFilePath = string.Format("{0}/{1}", Application.persistentDataPath, bundleInfo.bundle_name);
                bool matched = false;

                if (GetLocalBundleMD5(bundleInfo.bundle_name) == bundleInfo.md5)
                {
                    matched = true;
                }
                else
                {
                    Debug.LogErrorFormat("MD5 不匹配： {0}, FileMD5: {1}, bInfoMD5: {2}", bundleInfo.bundle_name, GetLocalBundleMD5(bundleInfo.bundle_name), bundleInfo.md5);
                }

                if (!matched)
                {
                    return false;
                }
            }

            Debug.LogFormat("本地Bundle文件检完全正确");
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="callback"><是否成功，是否是强更></param>
    /// <param name="force"></param>
    /// <returns></returns>
    private IEnumerator TryUpdateVersion(System.Action<bool, bool> callback, bool force = false)
    {
        UpdateUIState("正在检查更新");
        this.remoteVersionInfo = null;
        this.updateInfo = null;
        string remoteVersionUrl = REMOTE_URL + "/fishing/version.json";
        Debug.LogFormat("开始下载远程 Version 文件: {0}", remoteVersionUrl);
        HTTPRequest request = new HTTPRequest(new System.Uri(remoteVersionUrl), false, true, null).Send();

        while (request.State < HTTPRequestStates.Finished)
        {
            yield return new WaitForSeconds(0.1f);
        }

        if (request.State == HTTPRequestStates.Finished &&
         request.Response.IsSuccess)
        {
            string remoteVersionText = request.Response.DataAsText;
            if (!string.IsNullOrEmpty(remoteVersionText))
            {
                MoeVersionInfo remoteVersionInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<MoeVersionInfo>(remoteVersionText);
                if (remoteVersionInfo != null)
                {
                    Debug.LogFormat("远程 Version 文件解析成功, Version: {0}", remoteVersionInfo.version);
                    // 判断是否要更新

                    int appMajorVersion = AppConfig.Inst.GetMajorVersion();
                    // 判断是否要强更
                    int remoteMajor = remoteVersionInfo.GetMajorVersion();
                    if (remoteMajor > appMajorVersion)
                    {
                        // 这是一个需要强更的版本，需要提示用户去商店下载
                        Debug.LogFormat("发现强更版本，需要重新下包，进行大版本更新!");
                        callback?.Invoke(true, true);
                        callback = null;

                        UpdateUIState("新的大版本已更新，请下载最新安装包！");
                    }
                    else
                    {
                        // 强制修复
                        if (force)
                        {
                            this.remoteVersionInfo = remoteVersionInfo;
                            List<VersionBundleInfo> updateBundleList = new List<VersionBundleInfo>();
                            updateBundleList.AddRange(remoteVersionInfo.bundles);
                            // 有需要更新的包
                            this.updateInfo = new UpdateInfo();
                            this.updateInfo.remoteVersionInfo = remoteVersionInfo;
                            this.updateInfo.updateBundleList = updateBundleList;
                            Debug.LogFormat("强制更新，有需要更新的Bundle");
                            callback?.Invoke(true, false);
                            callback = null;
                        }
                        else
                        {
                            // 正常更新
                            int[] remoteVersionDigit = remoteVersionInfo.GetVersionDigitArray();
                            int[] currVersionDigit = this.currVersionInfo == null ? new int[] { 0, 0, 0 } : this.currVersionInfo.GetVersionDigitArray();
                            // if (this.currVersionInfo == null || remoteVersionInfo.GetVersionLong() > this.currVersionInfo.GetVersionLong())
                            if (remoteVersionDigit[0] > currVersionDigit[0] ||
                               remoteVersionDigit[1] > currVersionDigit[1] ||
                               remoteVersionDigit[2] > currVersionDigit[2])
                            {
                                Debug.LogFormat("这次需要热更新");
                                this.remoteVersionInfo = remoteVersionInfo;
                                List<VersionBundleInfo> updateBundleList = new List<VersionBundleInfo>();

                                foreach (VersionBundleInfo rBInfo in remoteVersionInfo.bundles)
                                {
                                    if (GetLocalBundleMD5(rBInfo.bundle_name) != rBInfo.md5)
                                    {
                                        updateBundleList.Add(rBInfo);
                                    }
                                }

                                // 有需要更新的包
                                this.updateInfo = new UpdateInfo();
                                this.updateInfo.remoteVersionInfo = remoteVersionInfo;
                                this.updateInfo.updateBundleList = updateBundleList;
                                Debug.LogFormat("有需要更新的Bundle");
                                callback?.Invoke(true, false);
                                callback = null;
                            }
                            else
                            {
                                Debug.LogFormat("远程版本号 {0} <= 本地版本号 {1}，无需更新!", remoteVersionInfo.version, this.currVersionInfo.version);
                                callback?.Invoke(true, false);
                                callback = null;
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogErrorFormat("远程 Version 文件反序列化失败: {0}", remoteVersionText);
                }
            }
            else
            {
                Debug.LogErrorFormat("远程 Version 文件内容为空");
            }
        }
        else
        {
            Debug.LogErrorFormat("远程 Version 文件下载失败: {0}, {1}", request.State, request.Response.StatusCode);
        }

        BestHTTP.PlatformSupport.Memory.BufferPool.Release(request.Response.Data);
        callback?.Invoke(false, false);
    }

    private IEnumerator TryUpdateBundle(System.Action<bool> callback)
    {
        if (this.remoteVersionInfo != null && this.updateInfo != null)
        {
            long totalSize = 0;
            foreach (VersionBundleInfo bInfo in this.updateInfo.updateBundleList)
            {
                totalSize += bInfo.size;
            }

            UpdateUIDownload(totalSize, 0);
            long downloadedSize = 0;
            bool hasError = false;

            foreach (VersionBundleInfo bInfo in this.updateInfo.updateBundleList)
            {
                Debug.LogFormat("Bundle信息 {0} | {1}", GetLocalBundleMD5(bInfo.bundle_name), bInfo.md5);
                if (GetLocalBundleMD5(bInfo.bundle_name) != bInfo.md5)
                {
                    string remoteBundleUrl = string.Format("{0}/fishing/{1}/{2}", REMOTE_URL, this.updateInfo.remoteVersionInfo.version, bInfo.bundle_name);
                    Debug.LogFormat("开始更新Bundle: {0}", remoteBundleUrl);
                    HTTPRequest request = new HTTPRequest(new System.Uri(remoteBundleUrl), false, true, null).Send();
                    while (request.State < HTTPRequestStates.Finished)
                    {

                        yield return new WaitForSeconds(0.1f);
                    }

                    if (request.State == HTTPRequestStates.Finished && request.Response.IsSuccess)
                    {
                        downloadedSize += bInfo.size;
                        string bundleWritePath = Application.persistentDataPath + "/" + bInfo.bundle_name;
                        // 写入Bundle文件
                        System.IO.File.WriteAllBytes(bundleWritePath, request.Response.Data);
                        Debug.LogFormat("{0} 更新完成", bInfo.bundle_name);
                        UpdateUIDownload(totalSize, downloadedSize);
                    }
                    else
                    {
                        Debug.LogErrorFormat("{0} 下载出错: {1}, {2}", bInfo.bundle_name, request.State, request.Response.IsSuccess);
                        callback?.Invoke(false);
                        callback = null;
                        hasError = true;
                        break;
                    }
                    yield return null;
                    BestHTTP.PlatformSupport.Memory.BufferPool.Release(request.Response.Data);
                }
                else
                {
                    Debug.LogFormat("!!!!!!!!!!! 本地已存在需要更新的 {0}，跳过下载", bInfo.bundle_name);
                    downloadedSize += bInfo.size;
                    UpdateUIDownload(totalSize, downloadedSize);
                }
            }

            if (!hasError)
            {
                Debug.LogFormat("写入远程 Version 文件");
                // 最后写入Version文件
                string versionText = Newtonsoft.Json.JsonConvert.SerializeObject(this.updateInfo.remoteVersionInfo);
                System.IO.File.WriteAllText(VERSION_FILE_PATH, versionText);
                yield return null;

                // 重新加载一遍本地文件
                this.currVersionInfo = LoadVersionInfo(VERSION_FILE_PATH);
                UpdateUIState("更新完成");
            }
        }
        else
        {
            Debug.LogFormat("无需要更新，前置数据不足: remoteVersionInfo is Null: {0}, updateInfo is Null: {1}", this.remoteVersionInfo == null, this.updateInfo == null);
        }
        callback?.Invoke(true);
    }

    private class UpdateInfo
    {
        public MoeVersionInfo remoteVersionInfo;
        public List<VersionBundleInfo> updateBundleList;
    }

    private void UpdateUIState(string msg)
    {
        versionStateParam.state = msg;
        MoeEventManager.Inst.SendEvent(EventID.Event_OnVersionState, versionStateParam);
    }

    private void UpdateUIDownload(long total, long downloaded)
    {
        updateProgressParam.totalUpdateSize = total;
        updateProgressParam.nowUpdatedSize = downloaded;
        MoeEventManager.Inst.SendEvent(EventID.Event_OnUpdateProgress, updateProgressParam);
    }

    private void OnMsgBox(string msg, string btnText, System.Action callback)
    {
        msgBoxParam.msg = msg;
        msgBoxParam.btnText = btnText;
        msgBoxParam.callback = callback;
        MoeEventManager.Inst.SendEvent(EventID.Event_OnVersionMsgBox, msgBoxParam);
    }
}