using UnityEngine;

[CreateAssetMenu(menuName="潮彩作品集/ゲーム設定")]
public sealed class StudioSkin : ScriptableObject
{
    public string key, title, english, product;
    [TextArea] public string introduction;
    [TextArea(2,8)] public string controls;
    public Color accent=new Color(.91f,.67f,.33f), ink=new Color(.10f,.15f,.18f), paper=new Color(.94f,.92f,.83f);
    public Texture2D cover;
    public Texture2D illustrations;
    public Texture2D specimenPlate;
    public AudioClip music;
    public AudioClip activityMusic;
    public AudioClip[] effects=new AudioClip[5];
}
