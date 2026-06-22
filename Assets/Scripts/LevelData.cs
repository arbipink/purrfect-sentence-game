using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [System.Serializable]
    public class Kalimat
    {
        [Tooltip("Teks lengkap kalimat")]
        public string teksLengkap;
        
        [Tooltip("Potongan kata sesuai urutan yang benar untuk dibawa musuh")]
        public List<string> potonganKataBenar;
    }

    [Header("Daftar Kalimat di Level Ini")]
    public List<Kalimat> daftarKalimat;

    [Header("Bank Kata Pengecoh")]
    [Tooltip("Kata-kata salah yang bakal dibawa jamur buat ngebingungin player")]
    public List<string> kataPengecoh;
}
