using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public static class Utils
{
    private static MD5 md5 = new MD5CryptoServiceProvider();

    public static string GetMD5(string path)
    {
        byte[] md5Byte = md5.ComputeHash(Encoding.Default.GetBytes(path));
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < md5Byte.Length; i++)
        {
            sb.Append(md5Byte[i].ToString("x2"));
        }
        return sb.ToString();
    }

    public static string GetMD5HashFromFile(string fileName)
    {
        try
        {
            FileStream file = new FileStream(fileName, FileMode.Open);
            byte[] retVal = md5.ComputeHash(file);
            file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
        }
    }
}
