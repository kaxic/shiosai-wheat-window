using UnityEngine;

// The six loop names correspond to editable OGG files under Resources/Harvest/Sound.
public sealed class HarvestAmbience : MonoBehaviour
{
    StudioAmbientMix mix;
    HarvestGame game;

    void Start()
    {
        game = GetComponent<HarvestGame>();
        mix = new StudioAmbientMix(gameObject, "Harvest/Sound", "Spring", "Summer", "Autumn", "Winter", "Rain", "Hearth");
    }

    void Update()
    {
        if (!game) return;
        bool indoor = game.Mode == HarvestMode.Kitchen || game.Mode == HarvestMode.Cooking ||
            game.Mode == HarvestMode.Service || game.Mode == HarvestMode.Sleep;
        int active = indoor ? 5 : game.Weather == 1 ? 4 : Mathf.Clamp(game.Season, 0, 3);
        float volume = game.Playing ? game.EffectsVolume * .76f : 0;
        mix.Set(active, volume);
    }
}
