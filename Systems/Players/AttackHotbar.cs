using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.IO;
using ATLAMod.Systems.Attacks;

namespace ATLAMod.Systems.Players
{
    public class AttackHotbar
    {
        public const int MAX_HOTBAR_SIZE = 8;
        public const int DEFAULT_SIZE = 2;

        private BaseAttack[] slots;
        private int selectedSlot;
        private int unlockedSlots;
        private Dictionary<string, int> attackCooldowns;
        private bool isExpanded;
        private float expansionAnimation;

        public AttackHotbar()
        {
            slots = new BaseAttack[MAX_HOTBAR_SIZE];
            selectedSlot = 0;
            unlockedSlots = DEFAULT_SIZE;
            attackCooldowns = new Dictionary<string, int>();
            isExpanded = false;
            expansionAnimation = 0f;
        }

        public BaseAttack GetSelectedAttack()
        {
            if (selectedSlot >= unlockedSlots)
            {
                return null;
            }
            return slots[selectedSlot];
        }

        public BaseAttack GetAttack(int slot)
        {
            if (slot < 0 || slot >= unlockedSlots)
            {
                return null;
            }
            return slots[slot];
        }

        public void SetAttack(int slot, BaseAttack attack)
        {
            if (slot < 0 || slot >= unlockedSlots)
            {
                return;
            }
            slots[slot] = attack;
        }

        public void ClearSlot(int slot)
        {
            if (slot < 0 || slot >= unlockedSlots)
            {
                return;
            }
            slots[slot] = null;
        }

        public int GetSelectedSlot()
        {
            return selectedSlot;
        }

        public void SetSelectedSlot(int slot)
        {
            if (slot >= 0 && slot < unlockedSlots)
            {
                selectedSlot = slot;
            }
        }

        public void NextSlot()
        {
            selectedSlot = (selectedSlot + 1) % unlockedSlots;
        }

        public void PreviousSlot()
        {
            selectedSlot = (selectedSlot - 1 + unlockedSlots) % unlockedSlots;
        }

        public int GetUnlockedSlots()
        {
            return unlockedSlots;
        }

        public void UnlockSlot()
        {
            if(unlockedSlots < MAX_HOTBAR_SIZE)
            {
                unlockedSlots++;
            }
        }

        public void SetUnlockSlots(int slots)
        {
            unlockedSlots = Math.Max(DEFAULT_SIZE, Math.Min(MAX_HOTBAR_SIZE, slots));

            if (selectedSlot >= unlockedSlots)
            {
                selectedSlot = 0;
            }
        }
        public bool CanUnlockMoreSlots()
        {
            return unlockedSlots < MAX_HOTBAR_SIZE;
        }

        //SELECTION/DESELECTION LOGIC HANDLING - expands hotbar when selected.
        public bool IsExpanded()
        {
            return isExpanded;
        }

        public void SetExpanded(bool expanded)
        {
            isExpanded = expanded;
        }
        public void ToggleExpanded()
        {
            isExpanded = !isExpanded;
        }

        public float GetExpansionAnimation()
        {
            return expansionAnimation;
        }

        public void UpdateAnimation(float deltaTime)
        {
            const float ANIMATION_SPEED = 2f;

            if (isExpanded)
            {
                expansionAnimation = Math.Min(1f, expansionAnimation + deltaTime * ANIMATION_SPEED);
            }
            else
            {
                expansionAnimation = Math.Max(0f, expansionAnimation - deltaTime * ANIMATION_SPEED);
            }
        }

        //cooldown management?

        public TagCompound Save()
        {
            TagCompound tag = new TagCompound();

            tag["selectedSlot"] = selectedSlot;
            tag["unlockedSlots"] = unlockedSlots;
            tag["isExpanded"] = isExpanded;

            for (int i = 0; i < unlockedSlots; i++)
            {
                if (slots[i] != null)
                {
                    tag[$"slot{i}"] = slots[i].Name;
                }
                else
                {
                    tag[$"slot{i}"] = "";
                }
            }

            return tag;
        }

        public void Load(TagCompound tag)
        {
            selectedSlot = tag.GetInt("selectedSlot");
            unlockedSlots = tag.GetInt("unloockedSlots");
            isExpanded = tag.GetBool("isExpanded");

            if (unlockedSlots < DEFAULT_SIZE)
                unlockedSlots = DEFAULT_SIZE;
            if(unlockedSlots > MAX_HOTBAR_SIZE)
                unlockedSlots = MAX_HOTBAR_SIZE;
            if (selectedSlot >= unlockedSlots)
                selectedSlot = 0;

            for (int i = 0; i < unlockedSlots; i++)
            {
                string attackName = tag.GetString($"slot{i}");
                if (!string.IsNullOrEmpty(attackName))
                {
                    slots[i] = AttackRegistry.GetAttackByName(attackName);
                }
                else
                {
                    slots[i] = null;
                }
            }

            expansionAnimation = isExpanded ? 1f : 0f;
        }

        //possibly useful helper methods
        public BaseAttack[] GetAllUnlockedSlots()
        {
            BaseAttack[] result = new BaseAttack[unlockedSlots];
            Array.Copy(slots, result, unlockedSlots);
            return result;
        }

        public bool HasAnyAttacks()
        {
            for (int i = 0; i < unlockedSlots; i++)
            {
                if (slots[i] != null)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetEmptySlotCount()
        {
            int count = 0;
            for (int i = 0; i < unlockedSlots; i++)
            {
                if (slots[i] == null)
                {
                    count++;
                }
            }
            return count;
        }

        public int GetFirstEmptySlot()
        {
            for (int i = 0; i < unlockedSlots; i++)
            {
                if (slots[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
