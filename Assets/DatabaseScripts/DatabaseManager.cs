using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Database;
using TMPro;

public class DatabaseManager : MonoBehaviour
{
    [SerializeField] TMP_InputField Name;
    [SerializeField] TMP_InputField ID;
    private string userID;
    private DatabaseReference databaseReference;
    // Start is called before the first frame update
    void Start()
    {
        userID = SystemInfo.deviceUniqueIdentifier;
        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void CreateUser()
    {
        User newUser = new User(Name.text, int.Parse(ID.text));
        string json = JsonUtility.ToJson(newUser);

        string key = databaseReference.Child("users").Push().Key;
        databaseReference.Child("users").Child(key).SetRawJsonValueAsync(json);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
