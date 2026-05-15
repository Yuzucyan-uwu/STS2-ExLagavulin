using Godot;
using Godot.Collections;
using MegaCrit.Sts2.Core.Saves;

namespace EXLagavulin.Core.Nodes.Vfx.Utilities;

[GlobalClass]
public partial class LocalizedTexture : Resource
{
    [Export(PropertyHint.None, "")]
    private Dictionary<string, Texture2D> _textures = new Dictionary<string, Texture2D>();

    public bool TryGetTexture(out Texture2D? texture)
    {
        texture = null;
        if (SaveManager.Instance.SettingsSave == null)
        {
            return false;
        }
        string language = SaveManager.Instance.SettingsSave.Language;
        if (string.IsNullOrEmpty(language))
        {
            return false;
        }
        if (!_textures.TryGetValue(language, out Texture2D value))
        {
            return false;
        }
        texture = value;
        return true;
    }
}