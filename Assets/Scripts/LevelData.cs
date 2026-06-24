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
        public string fullText;
        
        [Tooltip("Word fragments in the correct order to be carried by enemies")]
        public List<string> correctWordFragments;
    }

    [Header("List of Sentences in this Level")]
    public List<Sentence> sentences;
}

