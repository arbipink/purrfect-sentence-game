using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public class Sentence
    {
        [Tooltip("Full text of the sentence")]
        [FormerlySerializedAs("teksLengkap")]
        public string fullText;
        
        [Tooltip("Word fragments in the correct order to be carried by enemies")]
        [FormerlySerializedAs("potonganKataBenar")]
        public List<string> correctWordFragments;
    }

    [Header("List of Sentences in this Level")]
    [FormerlySerializedAs("daftarKalimat")]
    public List<Sentence> sentences;
}

