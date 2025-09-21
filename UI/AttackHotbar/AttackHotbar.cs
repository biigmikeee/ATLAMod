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

        // Layout config
        private const int MaxSlots = 6;
        private const int SlotSize = 54;
        private const int SlotSpacing = 60;
        private const int SlotsLeftOffset = 22;
        private const int SlotsTopOffset = 4;
      
        private Vector2 AnchoredPos => new Vector2(660, 12);

        // Slot UI elements (for click + hover)
        private readonly UIElement[] slotImages = new UIElement[MaxSlots];

        // UI textures (loaded as Assets)
        private Asset<Texture2D> texScrollCollapsed;
        private Asset<Texture2D> texScrollOpen;
        private Asset<Texture2D> texSlotEmpty;
        private Asset<Texture2D> texSlotEmptyHover;
        private Asset<Texture2D> texSlotLocked;
        private Asset<Texture2D> texSlotLockedHover;
        private Asset<Texture2D> texSlotSelected;
        private Asset<Texture2D> texSlotHover;

        // Cache for per-move icons
        private readonly Dictionary<string, Asset<Texture2D>> _iconCache = new();

        private BendingPlayer.BendingStyle _lastStyleLoaded = BendingPlayer.BendingStyle.None;

        //yet ANOTHER possible fix - pixel snappers XD
        private static int Px(float v) => (int)System.MathF.Round(v);

        private Rectangle SnappedSlotRect(UIElement el, int w, int h)
        {
            var d = el.GetDimensions();
            return new Rectangle(Px(d.X), Px(d.Y), w, h);
        }

        private void DrawTex(SpriteBatch sb, Asset<Texture2D> tex, Rectangle r)
        {
            if (tex?.Value != null) sb.Draw(tex.Value, r, Color.White);
        }

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
                var el = new UIElement();                
                el.Width.Set(SlotSize, 0f);
                el.Height.Set(SlotSize, 0f);
                el.SetPadding(0); el.HAlign = 0f; el.VAlign = 0f;
                int capture = i;
                el.OnLeftClick += (_, __) => OnSlotClicked(capture);
                root.Append(el);
                slotImages[i] = el;                
            }
            ReflowSlots();
        }

        private void ReflowSlots()
        {            
            for (int i = 0; i < slotImages.Length; i++){
                int spacing = i * SlotSpacing;
                int x = SlotsLeftOffset + (spacing - (i * 2));
                int y = SlotsTopOffset + ((i % 2 == 1) ? 2 : 0);

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

        private void LoadAssetsForStyle(BendingPlayer.BendingStyle style)
        {
            
            string basePath = "ATLAMod/Assets/UITextures/attackHotbarUI";

            texScrollCollapsed = ModContent.Request<Texture2D>($"{basePath}/collapsedHotbar", AssetRequestMode.ImmediateLoad);
            texScrollOpen = ModContent.Request<Texture2D>($"{basePath}/expandedHotbar{style}", AssetRequestMode.ImmediateLoad);

            texSlotEmpty = ModContent.Request<Texture2D>($"{basePath}/slotEmpty", AssetRequestMode.ImmediateLoad);
            texSlotEmptyHover = ModContent.Request<Texture2D>($"{basePath}/slotEmptyHover", AssetRequestMode.ImmediateLoad);
            texSlotLocked = ModContent.Request<Texture2D>($"{basePath}/slotLocked", AssetRequestMode.ImmediateLoad);
            texSlotLockedHover = ModContent.Request<Texture2D>($"{basePath}/slotLockedHover", AssetRequestMode.ImmediateLoad);
            texSlotSelected = ModContent.Request<Texture2D>($"{basePath}/slotSelected", AssetRequestMode.ImmediateLoad);
            texSlotHover = ModContent.Request<Texture2D>($"{basePath}/slotDarken", AssetRequestMode.ImmediateLoad);
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
                var slot = bp.MoveSlots[i];
                var img = slotImages[i];
                bool isHover = img.IsMouseHovering;
                bool isSelected = (i == bp.SelectedSlotIndex);

                Rectangle slotRect = SnappedSlotRect(img, SlotSize, SlotSize);

                if (!slot.Unlocked) //locked slot
                {
                    DrawTex(spriteBatch, isHover ? texSlotLockedHover : texSlotLocked, slotRect);
                } else if (string.IsNullOrEmpty(slot.MoveId) && slot.Unlocked) // emptyslot
                {
                    bool useHoverIcon = isHover || isSelected;
                    DrawTex(spriteBatch, useHoverIcon ? texSlotEmptyHover : texSlotEmpty, slotRect);
                }
                else //attack filled slot
                {
                    var move = MoveRegistry.Get(slot.MoveId);
                    if (move != null)
                    {
                        var icon = GetMoveIcon(move.IconPath).Value;
                        if (icon != null)
                        {
                            //centering icon
                            float sx = SlotSize / (float)icon.Width;
                            float sy = SlotSize / (float)icon.Height;
                            float scale = System.MathF.Min(sx, sy);
                            var size = new Vector2(icon.Width, icon.Height) * scale;

                            int ix = Px(slotRect.X + (slotRect.Width - size.X) * 0.5f);
                            int iy = Px(slotRect.Y + (slotRect.Height - size.Y) * 0.5f);
                            var iconRect = new Rectangle(ix, iy, Px(size.X), Px(size.Y));
                            spriteBatch.Draw(icon, iconRect, Color.White);
                        }
                    }
                }

                if (isHover) //hover border
                {
                    DrawTex(spriteBatch, texSlotHover, slotRect);
                }
                 
                if (isSelected) //selectedslot
                {
                    DrawTex(spriteBatch, texSlotSelected, slotRect);
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
                    Main.instance.MouseText($"{move.Name} - Uses {move.Cost} {resourceWord} \n {move.BaseDamage} Firebending Damage");
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
