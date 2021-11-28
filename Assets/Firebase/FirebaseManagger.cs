using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using System.Threading.Tasks;
using System;
using System.Linq;

[Serializable]
public class ListData
{
    public List<string> data;
}

[Serializable]
public class StringData
{
    public string data;
}

[Serializable]
public class FloatData
{
    public float data;
}

public class FirebaseManagger
{
    public static string firebaseLink = "https://swingcourseservers-default-rtdb.firebaseio.com/";

    #region Set
    public static void SetStringData(string path, string data)
    {
        RestClient.Put(firebaseLink + path.ToLower() + ".json", new StringData { data = data });
    }

    public static void SetListData(string path, List<string> data)
    {
        RestClient.Put(firebaseLink + path.ToLower() + ".json", new ListData { data = data });
    }

    public static void SetIntData(string path, float data)
    {
        RestClient.Put(firebaseLink + path.ToLower() + ".json", new FloatData { data = data });
    }
    #endregion

    #region Get
    public static void GetStringData(string path, Action<string> onResolve)
    {
        RestClient.Get<StringData>(firebaseLink + path.ToLower() + ".json").Done(r =>
        {
            onResolve.Invoke(r.data);
        }, e =>
        {
            onResolve.Invoke("");
        });
    }

    public static void GetListData(string path, Action<List<string>> onResolve)
    {
        RestClient.Get<ListData>(firebaseLink + path.ToLower() + ".json").Done(r =>
        {
            onResolve.Invoke(r.data);
        }, e =>
        {
            onResolve.Invoke(new List<string>());
        });
    }

    public static void GetFloatData(string path, Action<float> onResolve)
    {
        RestClient.Get<FloatData>(firebaseLink + path.ToLower() + ".json").Done(r =>
        {
            onResolve.Invoke(r.data);
        }, e =>
        {
            onResolve.Invoke(float.NaN);
        });
    }
    #endregion

    #region List Operations
    public static void AddToList(string path, string data)
    {
        GetListData(path, ld =>
        {
            List<string> newList = ld;

            newList.Add(data);

            SetListData(path, newList);
        });
    }

    public static void RemoveFromList(string path, int index)
    {
        GetListData(path, ld =>
        {
            List<string> newList = ld;

            newList.RemoveAt(index);

            SetListData(path, newList);
        });
    }

    public static void RemoveFromList(string path, string name)
    {
        GetListData(path, ld =>
        {
            List<string> newList = ld;

            newList.RemoveAll(x => x == name);

            SetListData(path, newList);
        });
    }

    public static void RemoveFromListThenQuit(string path, string name)
    {
        GetListData(path, ld =>
        {
            List<string> newList = ld;

            newList.RemoveAll(x => x == name);

            SetListData(path, newList);

            Application.Quit();
        });
    }
    #endregion
}
