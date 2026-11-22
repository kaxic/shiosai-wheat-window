using UnityEngine;

public sealed class StudioAmbientMix
{
    readonly AudioSource[] sources;

    public StudioAmbientMix(GameObject owner, string directory, params string[] names)
    {
        sources = new AudioSource[names.Length];
        for (int i = 0; i < names.Length; i++)
        {
            var source = owner.AddComponent<AudioSource>();
            source.playOnAwake = false; source.loop = true; source.volume = 0;
            source.clip = Resources.Load<AudioClip>(directory + "/" + names[i]);
            sources[i] = source;
        }
    }

    public void Set(int main, float volume, int overlay = -1, float overlayVolume = 0)
    {
        for (int i = 0; i < sources.Length; i++)
        {
            var source = sources[i];
            if (!source.clip) continue;
            float target = i == main ? volume : i == overlay ? overlayVolume : 0;
            if (target > 0 && !source.isPlaying) source.Play();
            source.volume = Mathf.MoveTowards(source.volume, target, Time.unscaledDeltaTime * .42f);
            if (source.isPlaying && source.volume <= 0 && target <= 0) source.Stop();
        }
    }
}
