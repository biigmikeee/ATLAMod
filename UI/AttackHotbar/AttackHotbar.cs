using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using ATLAMod.Systems.Bending;
using ATLAMod.Systems.Players;
using BPStyle = ATLAMod.Systems.Players.BendingPlayer.BendingStyle;


namespace ATLAMod.UI.AttackHotbar
{
    public class AttackHotbar : UIState
    {
        private UIElement root;

        // Layout config (tweak to taste)
        private const int MaxSlots = 6;
        private const int SlotSize = 64;
        private const int SlotSpacing = 72;
        private const int SlotsLeftOffset = 20;
        private const int SlotsTopOffset = 20;

        // Anchor (bottom-left-ish); you can move this anywhere
        private Vector2 AnchoredPos => new Vector2(300, Main.screenHeight - 140);

        // Slot UI elements (for click + hover)
        private readonly UIImage[] slotImages = new UIImage[MaxSlots];

        // UI textures (loaded as Assets)
        private Asset<Texture2D> texScrollCollapsed;
        private Asset<Texture2D> texScrollOpen;
        private Asset<Texture2D> texSlotEmpty;
        private Asset<Texture2D> texSlotLocked;
        private Asset<Texture2D> texSlotSelected;

        // Cache for per-move icons
        private readonly Dictionary<string, Asset<Texture2D>> _iconCache = new();

        private BPStyle _lastStyleLoaded = BPStyle.None;

        public override void OnInitialize()
        {
            root = new UIElement();
            root.Left.Set(AnchoredPos.X, 0f);
            root.Top.Set(AnchoredPos.Y, 0f);
            // Reasonable default size; background draw uses real texture size anyway
            root.Width.Set(520, 0f);
            root.Height.Set(120, 0f);
            Append(root);

            // Load initial assets (default to Fire if we can't read player yet)            
            LoadAssetsForStyle(BPStyle.Fire);
            _lastStyleLoaded = BPStyle.Fire;

            // Create slot images
            for (int i = 0; i < MaxSlots; i++)
            {
                var img = new UIImage(texSlotEmpty);
                img.Left.Set(SlotsLeftOffset + i * SlotSpacing, 0f);
                img.Top.Set(SlotsTopOffset, 0f);
                img.Width.Set(SlotSize, 0f);
                img.Height.Set(SlotSize, 0f);

                int capture = i;
                img.OnLeftClick += (_, __) => OnSlotClicked(capture);
                img.OnMouseOver += (_, __) => OnSlotHover(capture);

                root.Append(img);
                slotImages[i] = img;
            }
        }

        /// <summary>
        /// Plug your actual asset paths here. You can branch on style to load themed scrolls.
        /// </summary>
        private void LoadAssetsForStyle(BPStyle style)
        {
            // Example base; change to your real paths. You can do a switch(style) for per-style atlases.
            string basePath = "ATLAMod/Assets/UITextures/attackHotbarTest";

            texScrollCollapsed = ModContent.Request<Texture2D>($"{basePath}/collapsedTest", AssetRequestMode.ImmediateLoad);
            texScrollOpen = ModContent.Request<Texture2D>($"{basePath}/expandedTest{style}", AssetRequestMode.ImmediateLoad);

            texSlotEmpty = ModContent.Request<Texture2D>($"{basePath}/slotEmptyTest", AssetRequestMode.ImmediateLoad);
            texSlotLocked = ModContent.Request<Texture2D>($"{basePath}/slotLockedTest", AssetRequestMode.ImmediateLoad);
            texSlotSelected = ModContent.Request<Texture2D>($"{basePath}/slotSelectedTest", AssetRequestMode.ImmediateLoad);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            var lp = Main.LocalPlayer;
            if (lp != null && lp.active)
            {
                var bp = lp.GetModPlayer<BendingPlayer>();

                // If the style changed (e.g., after choosing in the Bending Scroll), reload themed assets
                if (bp.chosenStyle != _lastStyleLoaded)
                {
                    LoadAssetsForStyle(bp.chosenStyle);
                    _lastStyleLoaded = bp.chosenStyle;
                }

                // Capture mouse ONLY when hovering the hotbar area (so left-click outside can fire moves)
                if (bp.HotbarExpanded)
                {
                    var bg = texScrollOpen;
                    var area = GetBackgroundRect(bg);
                    if (area.Contains(Main.mouseX, Main.mouseY))
                    {
                        Main.LocalPlayer.mouseInterface = true; // let slot clicks select, not fire
                    }
                }
                else
                {
                    // Collapsed: only show hint on hover; no need to capture clicks here
                    // (We keep mouseInterface false so normal gameplay continues)
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Main.gameMenu) return;

            var lp = Main.LocalPlayer;
            var bp = lp?.GetModPlayer<BendingPlayer>();
            var pos = new Vector2(root.GetInnerDimensions().X, root.GetInnerDimensions().Y);

            // Draw background (collapsed vs expanded)
            var bg = bp.HotbarExpanded ? texScrollOpen : texScrollCollapsed;
            spriteBatch.Draw(bg.Value, pos, Color.White);

            // Collapsed hover hint
            if (!bp.HotbarExpanded)
            {
                var area = GetBackgroundRect(bg);
                if (area.Contains(Main.mouseX, Main.mouseY))
                {
                    Main.instance.MouseText($"Press [{KeybindLabel()}] to access Moveset");
                }
                return; // no slots drawn when collapsed
            }

            // Expanded — draw slots and icons
            for (int i = 0; i < MaxSlots; i++)
            {
                var slotState = bp.MoveSlots[i];
                var img = slotImages[i];

                // Pick slot frame
                Asset<Texture2D> frame;
                if (!slotState.Unlocked) frame = texSlotLocked;
                else if (i == bp.SelectedSlotIndex) frame = texSlotSelected;
                else frame = texSlotEmpty;

                img.SetImage(frame);

                // Draw move icon if assigned
                if (slotState.Unlocked && !string.IsNullOrEmpty(slotState.MoveId))
                {
                    var move = MoveRegistry.Get(slotState.MoveId);
                    if (move != null)
                    {
                        var iconAsset = GetMoveIcon(move.IconPath);
                        DrawIconCentered(spriteBatch, iconAsset.Value, img.GetDimensions().ToRectangle());
                    }
                }
            }

            base.Draw(spriteBatch);
        }

        private void OnSlotClicked(int index)
        {
            var bp = Main.LocalPlayer.GetModPlayer<BendingPlayer>();
            if (!bp.HotbarExpanded) return;
            bp.SelectSlot(index);
        }

        private void OnSlotHover(int index)
        {
            var bp = Main.LocalPlayer.GetModPlayer<BendingPlayer>();
            var slot = bp.MoveSlots[index];

            if (!slot.Unlocked)
            {
                Main.instance.MouseText("Locked — unlock more slots through progression.");
                return;
            }

            if (string.IsNullOrEmpty(slot.MoveId))
            {
                Main.instance.MouseText("Empty slot — assign a move using the Bending Scroll.");
                return;
            }

            var move = MoveRegistry.Get(slot.MoveId);
            if (move != null)
            {
                string resourceWord = bp.chosenStyle switch
                {
                    BendingPlayer.BendingStyle.Fire => "Breath",
                    BendingPlayer.BendingStyle.Water => "Water",
                    BendingPlayer.BendingStyle.Earth => "Stamina",
                    BendingPlayer.BendingStyle.Air => "Chi",
                    _ => "Cost"
                };
                Main.instance.MouseText($"{move.Name} — {move.Cost} {resourceWord}");
            }
        }

        // --- helpers ---

        private Rectangle GetBackgroundRect(Asset<Texture2D> bg)
        {
            var x = (int)root.GetInnerDimensions().X;
            var y = (int)root.GetInnerDimensions().Y;
            int w = bg?.Value?.Width ?? 500;
            int h = bg?.Value?.Height ?? 100;
            return new Rectangle(x, y, w, h);
        }

        private string KeybindLabel()
        {
            // Show first assigned key for the toggle bind (defaults to "F")
            var keys = ATLAMod.ToggleAttackHotbar?.GetAssignedKeys();
            if (keys != null && keys.Count > 0) return keys[0];
            return "F";
        }

        private Asset<Texture2D> GetMoveIcon(string iconPath)
        {
            if (string.IsNullOrEmpty(iconPath))
                return Terraria.GameContent.TextureAssets.MagicPixel;

            if (_iconCache.TryGetValue(iconPath, out var cached))
                return cached;

            var loaded = ModContent.Request<Texture2D>(iconPath, AssetRequestMode.ImmediateLoad);
            _iconCache[iconPath] = loaded;
            return loaded;
        }

        private void DrawIconCentered(SpriteBatch sb, Texture2D tex, Rectangle slotRect)
        {
            if (tex == null) return;
            float scale = System.MathF.Min(slotRect.Width / (float)tex.Width, slotRect.Height / (float)tex.Height);
            var size = new Vector2(tex.Width, tex.Height) * scale;
            var pos = new Vector2(
                slotRect.X + (slotRect.Width - size.X) * 0.5f,
                slotRect.Y + (slotRect.Height - size.Y) * 0.5f
            );
            sb.Draw(tex, pos, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}
