using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Security.Cryptography;
using UnityEditor.Build.Reporting;
using Unity.Plastic.Newtonsoft;

// 注意：BundleCombineConfig.json 中的配置，目录最后！不要！加上 '/'
public class AssetBundleBuilder
{
    private static string RES_TO_BUILD_PATH = "Assets/Res/";
    private static string MANIFEST_FILES_PATH = string.Format("{0}/../BundleManifest/", Application.dataPath);
    private static StringBuilder IndexFileContent = null;
    private static StringBuilder VersionFileContent = null;
    private static MD5 md5 = null;
    private static BuildAssetBundleOptions BuildOption = BuildAssetBundleOptions.ChunkBasedCompression |
                                                        BuildAssetBundleOptions.ForceRebuildAssetBundle;

    private static BundleCombineConfig combineConfig = null;
    private static Dictionary<string, int> combinePathDict = null;

    private static string version = "0.0.0";
    private static bool copyToStreaming = false;

    private static void InitBuilder()
    {
        IndexFileContent = new StringBuilder();
        VersionFileContent = new StringBuilder();
        md5 = new MD5CryptoServiceProvider();
        combineConfig = null;
        combinePathDict = new Dictionary<string, int>();
    }

    private static void WriteIndexFile(string key, string value)
    {
        IndexFileContent.AppendFormat("{0}:{1}", key, value);
        IndexFileContent.AppendLine();
    }

    private static void WriteVersionFile(string key, string value1, long value2)
    {
        VersionFileContent.AppendFormat("{0}:{1}:{2}", key, value1, value2);
        VersionFileContent.AppendLine();
    }

    private static long GetFileSize(string fileName)
    {
        try
        {
            FileInfo fileInfo = new FileInfo(fileName);
            return fileInfo.Length;
        }
        catch (Exception ex)
        {
            throw new Exception("GetFileSize() fail, error:" + ex.Message);
        }
    }

    private static string GetMD5(byte[] retVal)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < retVal.Length; i++)
        {
            sb.Append(retVal[i].ToString("x2"));
        }
        return sb.ToString();
    }

    private static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            return GetMD5(retVal);
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
        }
    }

    static string GetBundleName(string path)
    {
        byte[] md5Byte = md5.ComputeHash(Encoding.Default.GetBytes(path));
        string str = GetMD5(md5Byte) + ".assetbundle";
        return str;
    }
    private class BuildBundleData
    {
        private AssetBundleBuild build = new AssetBundleBuild();
        private List<string> assets = new List<string>();
        private List<string> addresses = new List<string>();

        public BuildBundleData(string bundleName)
        {
            build.assetBundleName = bundleName;
        }

        public void AddAsset(string filePath)
        {
            string addressableName = GetAddressableName(filePath);
            assets.Add(filePath);
            addresses.Add(addressableName);
            WriteIndexFile(addressableName, build.assetBundleName);
        }

        public AssetBundleBuild Gen()
        {
            build.assetNames = assets.ToArray();
            build.addressableNames = addresses.ToArray();
            return build;
        }
    }

    private static string GetAddressableName(string file_path)
    {
        string addressable_name = file_path;
        addressable_name = addressable_name.Replace(RES_TO_BUILD_PATH, "");
        int dot_pos = addressable_name.LastIndexOf('.');
        if (dot_pos != -1)
        {
            int count = addressable_name.Length - dot_pos;
            addressable_name = addressable_name.Remove(dot_pos, count);
        }
        return addressable_name;
    }

    private static string[] GetTopDirs(string rPath)
    {
        return Directory.GetDirectories(rPath, "*", SearchOption.TopDirectoryOnly);
    }

    private static void CopyLuaDir()
    {
        // Copy Lua
        string luaOutPath = Application.dataPath + "/../LuaScripts";
        string luaInPath = Application.dataPath + "/Res/LuaScripts";

        DeleteLuaDir();

        MoeUtils.DirectoryCopy(luaOutPath, luaInPath, true, ".txt");
        AssetDatabase.Refresh();
    }

    private static void DeleteLuaDir()
    {
        string luaInPath = Application.dataPath + "/Res/LuaScripts";

        if (Directory.Exists(luaInPath))
        {
            Directory.Delete(luaInPath, true);
        }
    }

    public static void BuildBundleWithVersion(string v, bool copy)
    {
        version = v;
        copyToStreaming = copy;
        BuildAssetBundle();
    }

    [MenuItem("Tools/Build Bundles")]
    private static void BuildAssetBundle()
    {
        if (version == "0.0.0")
        {
            Debug.LogErrorFormat("请确认版本号");
            return;
        }
        CopyLuaDir();

        InitBuilder();
        LoadBundleCombineConfig();
        Dictionary<string, BuildBundleData> bundleDatas = new Dictionary<string, BuildBundleData>();
        IndexFileContent.Clear();
        VersionFileContent.Clear();

        List<DirBundleInfo> dirList = new List<DirBundleInfo>();

        // ============================
        Queue<DirBundleInfo> dirQueue = new Queue<DirBundleInfo>();
        dirQueue.Enqueue(new DirBundleInfo(RES_TO_BUILD_PATH));
        while (dirQueue.Count > 0)
        {
            DirBundleInfo rootDirInfo = dirQueue.Dequeue();
            if (rootDirInfo.dir != RES_TO_BUILD_PATH)
            {
                if (combinePathDict.ContainsKey(rootDirInfo.dir))
                {
                    rootDirInfo.combine2Dir = rootDirInfo.dir;
                }
                dirList.Add(rootDirInfo);
            }

            foreach (string subDir in GetTopDirs(rootDirInfo.dir))
            {
                DirBundleInfo subDirInfo = new DirBundleInfo(subDir);
                subDirInfo.combine2Dir = rootDirInfo.combine2Dir;
                dirQueue.Enqueue(subDirInfo);

                Debug.LogFormat("Dir: {0}, Combine2Dir: {1}", subDirInfo.dir, subDirInfo.combine2Dir);
            }
        }

        foreach (DirBundleInfo dirInfo in dirList)
        {
            string[] files = GetFiles(dirInfo.dir, SearchOption.TopDirectoryOnly);
            if (files.Length > 0)
            {
                Debug.LogFormat("Dir: {0}, FileCount: {1}", dirInfo.dir, files.Length);
                string bundleDirName = dirInfo.BundleDirName;
                BuildBundleData bbData = null;
                if (bundleDatas.ContainsKey(bundleDirName))
                {
                    bbData = bundleDatas[bundleDirName];
                }
                else
                {
                    bbData = new BuildBundleData(GetBundleName(bundleDirName));
                    bundleDatas.Add(bundleDirName, bbData);
                }

                foreach (string file in files)
                {
                    bbData.AddAsset(file);
                }
            }
        }

        List<AssetBundleBuild> bundleBuildList = new List<AssetBundleBuild>();
        foreach (BuildBundleData data in bundleDatas.Values)
        {
            bundleBuildList.Add(data.Gen());
        }

        string index_file_path = string.Format("{0}{1}.txt", RES_TO_BUILD_PATH, "index");
        File.WriteAllText(index_file_path, IndexFileContent.ToString());
        AssetDatabase.ImportAsset(index_file_path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        AssetBundleBuild indexBuild = new AssetBundleBuild();
        indexBuild.assetBundleName = "index";
        indexBuild.assetNames = new string[] { index_file_path };
        indexBuild.addressableNames = new string[] { "index" };
        bundleBuildList.Add(indexBuild);
        string bundleExportPath = string.Format("{0}/{1}/", Application.dataPath + "/../streaming", "Bundles");
        if (Directory.Exists(bundleExportPath))
        {
            Directory.Delete(bundleExportPath, true);
        }
        Directory.CreateDirectory(bundleExportPath);

        if (Directory.Exists(MANIFEST_FILES_PATH))
        {
            Directory.Delete(MANIFEST_FILES_PATH, true);
        }
        Directory.CreateDirectory(MANIFEST_FILES_PATH);

        BuildPipeline.BuildAssetBundles(bundleExportPath, bundleBuildList.ToArray(), BuildOption, EditorUserBuildSettings.activeBuildTarget);
        AssetDatabase.Refresh();
        DeleteLuaDir();
        AssetDatabase.Refresh();

        // VersionProfile

        List<VersionBundleInfo> versionBundleList = new List<VersionBundleInfo>();
        MoeVersionInfo versionInfo = new MoeVersionInfo();
        versionInfo.version = version;
        versionInfo.asset_date = DateTime.Now.ToString("yyyyMMddHHmm");
        string[] ab_files = Directory.GetFiles(bundleExportPath);
        foreach (string ab_file in ab_files)
        {
            if (Path.GetExtension(ab_file) == ".manifest")
            {
                string new_path = ab_file.Replace(bundleExportPath, MANIFEST_FILES_PATH);
                File.Move(ab_file, new_path);
            }
            else
            {

                Debug.LogFormat("BundleName: {0}", ab_file);
                var data = File.ReadAllBytes(ab_file);
                using (var abStream = new AssetBundleStream(ab_file, FileMode.Create))
                {
                    abStream.Write(data, 0, data.Length);
                }

                string md5 = GetMD5HashFromFile(ab_file);
                long size = GetFileSize(ab_file);
                string bundleName = string.Format("Bundles/{0}", Path.GetFileName(ab_file));
                VersionBundleInfo bInfo = new VersionBundleInfo();
                bInfo.bundle_name = bundleName;
                bInfo.md5 = md5;
                bInfo.size = size;
                versionBundleList.Add(bInfo);
            }
        }

        versionInfo.bundles = versionBundleList.ToArray();
        string versionInfoText = Newtonsoft.Json.JsonConvert.SerializeObject(versionInfo);

        File.WriteAllText(string.Format("{0}/{1}", bundleExportPath, "version.json"), versionInfoText);

        if (copyToStreaming)
        {
            CopyBundleToStreaming(bundleExportPath);
        }
        MoveToVersionDir(bundleExportPath, version);
        AssetDatabase.Refresh();
    }

    private static void MoveToVersionDir(string rootBundlePath, string version)
    {
        string destPath = rootBundlePath + "/" + version;
        Directory.CreateDirectory(destPath);
        destPath += "/Bundles";
        Directory.CreateDirectory(destPath);

        string[] files = GetFiles(rootBundlePath, SearchOption.TopDirectoryOnly);
        foreach (string file in files)
        {
            string fileName = System.IO.Path.GetFileName(file);
            string destFilePath = destPath + "/" + fileName;
            File.Move(file, destFilePath);
        }
    }

    private static void CopyBundleToStreaming(string bundleExportPath)
    {
        string destPath = Application.streamingAssetsPath + "/Bundles";
        if (Directory.Exists(destPath))
        {
            Directory.Delete(destPath, true);
        }

        MoeUtils.DirectoryCopy(bundleExportPath, destPath, true);
    }

    private static string[] GetFiles(string path, SearchOption so)
    {
        string[] files = Directory.GetFiles(path, "*", so);
        List<string> fileList = new List<string>();
        foreach (string file in files)
        {
            string ext = Path.GetExtension(file);
            if (ext == ".meta" || ext == ".DS_Store")
            {
                continue;
            }
            fileList.Add(file);
        }

        return fileList.ToArray();
    }

    class DirBundleInfo
    {
        public string dir;
        public string combine2Dir;

        public bool IsCombine
        {
            get
            {
                return !string.IsNullOrEmpty(combine2Dir);
            }
        }

        public string BundleDirName
        {
            get
            {
                if (IsCombine)
                {
                    return combine2Dir;
                }
                else
                {
                    return dir;
                }
            }
        }

        public DirBundleInfo(string dir, string combine2Dir = null)
        {
            this.dir = dir;
            this.combine2Dir = combine2Dir;
        }

    }

    class BundleCombineConfig
    {
        public string[] combieDirs;
    }

    private static void LoadBundleCombineConfig()
    {
        string path = Application.dataPath + RES_TO_BUILD_PATH.Replace("Assets", "") + "BundleCombineConfig.json";
        if (File.Exists(path))
        {
            string text = File.ReadAllText(path);
            if (!string.IsNullOrEmpty(text))
            {
                combineConfig = Newtonsoft.Json.JsonConvert.DeserializeObject<BundleCombineConfig>(text);
                if (combineConfig != null)
                {
                    Debug.LogFormat("Bundle合并配置成功！");
                    foreach (string cPath in combineConfig.combieDirs)
                    {
                        if (!combinePathDict.ContainsKey(cPath))
                        {
                            combinePathDict.Add(cPath, 0);
                        }
                    }
                }
            }
        }
    }
}