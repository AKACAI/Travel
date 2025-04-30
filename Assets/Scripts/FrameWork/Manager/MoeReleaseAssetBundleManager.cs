using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class MoeReleaseAssetBundleManager : IMoeResAgent
{
    const string INDEX_FILE = "index";
    private Dictionary<int, Object> _resources = new Dictionary<int, Object>();
    private Dictionary<int, AssetBundle> _bundles = new Dictionary<int, AssetBundle>();
    private Dictionary<int, string> _bundles_index = new Dictionary<int, string>();

    private AssetBundleManifest _manifest = null;

    public void Init()
    {
        InitAndLoadManifestFile();
        InitAndLoadIndexFile();
    }

    private void InitAndLoadIndexFile()
    {
        _bundles_index.Clear();
        AssetBundle indexBundle = LoadBundleSync(INDEX_FILE);
        TextAsset ta = indexBundle.LoadAsset<TextAsset>(INDEX_FILE);
        if (ta == null)
        {
            Debug.LogErrorFormat("Index 文件加载失败！");
            return;
        }

        string[] lines = ta.text.Split('\n');
        char[] trim = new char[] { '\r', '\n' };

        if (lines != null && lines.Length > 0)
        {
            for (int i = 0; i < lines.Length; ++i)
            {
                string line = lines[i].Trim(trim);
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                string[] pair = line.Split(':');
                if (pair.Length != 2)
                {
                    Debug.LogErrorFormat("Index 行数据有问题: {0}", line);
                    continue;
                }

                int hash = pair[0].GetHashCode();
                if (_bundles_index.ContainsKey(hash))
                {
                    Debug.LogErrorFormat("Index 文件中存在相同的路径: {0}", pair[0]);
                }
                else
                {
                    _bundles_index.Add(hash, pair[1]);
                }
            }
        }

        if (_bundles_index.Count != 0)
        {
            Debug.LogFormat("Bundle Index 初始化完成");
        }
        else
        {
            Debug.LogErrorFormat("Index 文件数据为空");
        }

        indexBundle.Unload(true);
        indexBundle = null;
    }

    private void InitAndLoadManifestFile()
    {
        AssetBundle manifestBundle = LoadBundleSync("Bundles");
        _manifest = manifestBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
        manifestBundle.Unload(false);
        manifestBundle = null;
    }

    public T LoadAsset<T>(string path) where T : UnityEngine.Object
    {
        UnityEngine.Object obj = Load(path);
        if (obj != null)
        {
            return obj as T;
        }

        return null;
    }

    public byte[] LoadLuaCode(string path)
    {
        string assetPath = string.Format("LuaScripts/{0}", path);
        TextAsset ta = LoadAsset<TextAsset>(assetPath);
        if (ta != null)
        {
            return ta.bytes;
        }
        return null;
    }



    private UnityEngine.Object Load(string assetPath)
    {
        if (string.IsNullOrEmpty(assetPath))
        {
            return null;
        }

        int pathHash = assetPath.GetHashCode();
        Object obj = null;
        if (_resources.TryGetValue(pathHash, out obj))
        {
            if (obj == null)
            {
                _resources.Remove(pathHash);
            }
            else
            {
                return obj;
            }
        }

        AssetLoadInfo loadInfo = GetAssetLoadInfo(assetPath);
        // 加载依赖Bundle

        for (int i = 0; i < loadInfo.dependencies.Length; ++i)
        {
            if (LoadBundleSync(loadInfo.dependencies[i]) == null)
            {
                Debug.LogErrorFormat("加载依赖Bundle出错，资源 {0}， 主Bundle：{1}， 依赖：{2}", assetPath, loadInfo.mainBundle, loadInfo.dependencies[i]);
                return null;
            }
        }

        AssetBundle mainBundle = LoadBundleSync(loadInfo.mainBundle);
        if (mainBundle == null)
        {
            Debug.LogErrorFormat("加载主Bundle出错，资源：{0}，主Bundle：{1}", assetPath, loadInfo.mainBundle);
            return null;
        }

        obj = mainBundle.LoadAsset(assetPath);

        if (obj == null)
        {
            Debug.LogErrorFormat("从Bundle加载资源失败，资源：{0}，主Bundle：{1}", assetPath, loadInfo.mainBundle);
            return null;
        }

        _resources.Add(pathHash, obj);
        return obj;
    }

    private AssetBundle LoadBundleSync(string bundleName)
    {
        int bundleHash = bundleName.GetHashCode();
        AssetBundle bundle = null;

        if (!_bundles.TryGetValue(bundleHash, out bundle))
        {
#if UNITY_EDITOR
            string rootPath = Application.dataPath + "/../streaming";
#else
            string rootPath = Application.persistentDataPath;
#endif
            string bundleLoadPath = System.IO.Path.Combine(rootPath, string.Format("Bundles/{0}", bundleName));
            Debug.LogFormat(">>>> 加载Bundle: {0}", bundleLoadPath);

            using (var fileStream = new AssetBundleStream(bundleLoadPath, FileMode.Open, FileAccess.Read, FileShare.None, 1024 * 4, false))
            {
                bundle = AssetBundle.LoadFromStream(fileStream);
            }

            // bundle = AssetBundle.LoadFromFile(bundleLoadPath);

            if (bundle != null)
            {
                _bundles.Add(bundleHash, bundle);
            }
            else
            {
                Debug.LogErrorFormat("Bundle 加载失败 {0}, LoadPath: {1}", bundleName, bundleLoadPath);
            }
        }
        else
        {
            // Debug.LogFormat("Bundle {0} 已加载，直接返回", bundleName);
        }

        return bundle;
    }

    private string GetAssetOfBundleFileName(string assetPath)
    {
        int assetHash = assetPath.GetHashCode();
        string bundleName;
        if (_bundles_index.TryGetValue(assetHash, out bundleName))
        {
            return bundleName;
        }

        return string.Empty;
    }

    private AssetLoadInfo GetAssetLoadInfo(string assetPath)
    {
        AssetLoadInfo loadInfo = new AssetLoadInfo();
        loadInfo.assetPath = assetPath;
        loadInfo.mainBundle = GetAssetOfBundleFileName(assetPath);
        loadInfo.dependencies = _manifest.GetAllDependencies(loadInfo.mainBundle);
        return loadInfo;
    }


    private class AssetLoadInfo
    {
        public string assetPath;
        public string mainBundle;
        public string[] dependencies;
    }
}