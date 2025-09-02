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


namespace ATLAMod.UI.AttackHotbar
{
    public class AttackHotbar : UIState
    {
        private UIElement root;

        // Layout config (tweak to taste)
        private const int MaxSlots = 6;
        private const int SlotSize = 54;
        private const int SlotSpacing = 60;
        private const int SlotsLeftOffset = 20;
        private const int SlotsTopOffset = 2;

        // Anchor (bottom-left-ish); you can move this anywhere
        private Vector2 AnchoredPos => new Vector2(660, 12);

        // Slot UI elements (for click + hover)
        private readonly UIImage[] slotImages = new UIImage[MaxSlots];

        // UI textures (loaded as Assets)
        private Asset<Texture2D> texScrollCollapsed;
        private Asset<Texture2D> texScrollOpen;
        private Asset<Texture2D> texSlotEmpty;
        private Asset<Texture2D> texSlotLocked;
        private Asset<Texture2D> texSlotSelected;

        //possible slot misalign fix
        private Point _lastScreen;
        private float _lastUiScale;

        // Cache for per-move icons
        private readonly Dictionary<string, Asset<Texture2D>> _iconCache = new();

        private BendingPlayer.BendingStyle _lastStyleLoaded = BendingPlayer.BendingStyle.None;

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
            LoadAssetsForStyle(BendingPlayer.BendingStyle.Fire);
            _lastStyleLoaded = BendingPlayer.BendingStyle.Fire;

            // Create slot images
            for (int i = 0; i < MaxSlots; i++)
            {                
                var img = new UIImage(texSlotEmpty);                
                img.Width.Set(SlotSize, 0f);
                img.Height.Set(SlotSize, 0f);

                int capture = i;
                img.OnLeftClick += (_, __) => OnSlotClicked(capture);
                

                root.Append(img);
                slotImages[i] = img;
            }
            ReflowSlots();

            var currScreen = new Point(Main.screenWidth, Main.screenHeight);
            if (currScreen != _lastScreen || Main.UIScale != _lastUiScale)
            {
                _lastScreen = currScreen;
                _lastUiScale = Main.UIScale;
                ReflowSlots();
            }            
        }

        //realigning slots after drawing
        private void ReflowSlots()
        {
            for (int i = 0; i < slotImages.Length; i++)
            {
                int x = SlotsLeftOffset + i * SlotSpacing;
                int y = SlotsTopOffset + ((i % 2 == 1) ? 2 : 0); //odd slots get shifted down to align w/ hotbar background

                var img = slotImages[i];
                img.Left.Set(x, 0f);
                img.Top.Set(y, 0f);
                img.Width.Set(SlotSize, 0f);
                img.Height.Set(SlotSize, 0f);
                img.SetPadding(0);
                img.HAlign = 0f;
                img.VAlign = 0f;
                img.Recalculate();                
            }
        }

        /// <summary>
        /// Plug your actual asset paths here. You can branch on style to load themed scrolls.
        /// </summary>
        private void LoadAssetsForStyle(BendingPlayer.BendingStyle style)
        {
            // Example base; change to your real paths. You can do a switch(style) for per-style atlases.
            string basePath = "ATLAMod/Assets/UITextures/attackHotbarUI";

            texScrollCollapsed = ModContent.Request<Texture2D>($"{basePath}/collapsedHotbar", AssetRequestMode.ImmediateLoad);
            texScrollOpen = ModContent.Request<Texture2D>($"{basePath}/expandedHotbar{style}", AssetRequestMode.ImmediateLoad);

            texSlotEmpty = ModContent.Request<Texture2D>($"{basePath}/slotEmpty", AssetRequestMode.ImmediateLoad);
            texSlotLocked = ModContent.Request<Texture2D>($"{basePath}/slotLocked", AssetRequestMode.ImmediateLoad);
            texSlotSelected = ModContent.Request<Texture2D>($"{basePath}/slotSelected", AssetRequestMode.ImmediateLoad);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            

            var lp = Main.LocalPlayer;

            if (lp == null || !lp.active) return;

            var bp = lp.GetModPlayer<BendingPlayer>();

            //if no chosen style dont draw
            if (bp.chosenStyle == BendingPlayer.BendingStyle.None)
            {
                return;
            }

            if (lp != null && lp.active)
            {                

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

            //if no chosen style dont draw
            if (bp == null || bp.chosenStyle == BendingPlayer.BendingStyle.None)
            {
                return;
            }

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

            OnSlotHover();
            base.Draw(spriteBatch);
        }

        private void OnSlotClicked(int index)
        {
            var bp = Main.LocalPlayer.GetModPlayer<BendingPlayer>();
            if (!bp.HotbarExpanded) return;
            bp.SelectSlot(index);
        }

        private void OnSlotHover()
        {
            var lp = Main.LocalPlayer;
            if (lp == null || !lp.active) return;

            var bp = lp.GetModPlayer<BendingPlayer>();
            if (!bp.HotbarExpanded) return;

            for (int i = 0; i < MaxSlots; i++)
            {
                var el = slotImages[i];
                if (!el.IsMouseHovering) continue;

                var slot = bp.MoveSlots[i];
                if (!slot.Unlocked)
                {
                    Main.instance.MouseText("Locked - unlock more slots through progression.");
                    break;
                }

                if (string.IsNullOrEmpty(slot.MoveId))
                {
                    Main.instance.MouseText("Empty slot - assign in the Bending Scroll.");
                    break;
                }

                var move = MoveRegistry.Get(slot.MoveId);
                if (move != null)
                {
                    string resourceWord = bp.chosenStyle switch
                    {
                        BendingPlayer.BendingStyle.Fire => "breath",
                        BendingPlayer.BendingStyle.Water => "water",
                        BendingPlayer.BendingStyle.Earth => "stamina",
                        BendingPlayer.BendingStyle.Air => "chi",
                        _ => "Cost"
                    };
                    Main.instance.MouseText($"{move.Name} - Uses {move.Cost} {resourceWord}");
                }
                break;
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
