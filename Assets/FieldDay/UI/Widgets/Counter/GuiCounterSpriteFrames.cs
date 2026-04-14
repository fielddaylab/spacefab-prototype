using BeauUtil.Debugger;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FieldDay.UI.Widgets {
    public sealed class GuiCounterSpriteFrames : GuiCounter.Style {
        public SpriteRenderer Sprite;
        [FormerlySerializedAs("Target")] public Image GuiSprite;
        public Sprite[] Values;

        public override void Populate(in int data, GuiWidgetUpdateFlags flags) {
            Assert.True(data >= 0 && data < Values.Length);
            Sprite spr = Values[data];
            if (Sprite) {
                Sprite.sprite = spr;
            }
            if (GuiSprite) {
                GuiSprite.sprite = spr;
            }
        }
    }
}