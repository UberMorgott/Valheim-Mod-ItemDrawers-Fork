using System.Collections.Generic;

namespace ItemDrawers.Core
{
    /// <summary>
    /// Every player-facing text of the mod, keyed by localization token (used in code as
    /// "$itemdrawers_*"). Pure data so it stays in Core and is unit-tested; the Game layer
    /// hands it to Jotunn. Markup and key labels stay in code, only words live here.
    /// The game localizes them where they surface: MessageHud.ShowMessage, the hover text
    /// and the piece name/description.
    /// </summary>
    public static class DrawerTexts
    {
        public static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            { "itemdrawers_wood", "Item Drawer" },
            { "itemdrawers_stone", "Stone Item Drawer" },
            { "itemdrawers_blackmarble", "Black Marble Item Drawer" },
            { "itemdrawers_description", "Holds a great many of a single item." },
            { "itemdrawers_store_item", "Store an item" },
            { "itemdrawers_take_stack", "Take stack" },
            { "itemdrawers_take_one", "Take one" },
            { "itemdrawers_unassign", "Unassign" },
            { "itemdrawers_store_all", "Store all" },
            { "itemdrawers_stored", "Stored" },
            { "itemdrawers_try_again", "Try again" },
            { "itemdrawers_stackable_only", "Drawers only hold stackable items" },
            { "itemdrawers_unknown_item", "Cannot identify this item" },
            { "itemdrawers_remove_failed", "Could not remove item" },
            { "itemdrawers_not_assigned", "This drawer has not been assigned" },
            { "itemdrawers_none_carried", "You have none of those" },
            { "itemdrawers_wrong_item", "This drawer holds a different item" },
            { "itemdrawers_full", "This drawer is full" },
            { "itemdrawers_drained", "This drawer has been drained" },
            { "itemdrawers_bad_capacity", "This drawer has an invalid capacity" },
            { "itemdrawers_not_empty", "Empty the drawer before clearing it" },
            { "itemdrawers_nothing", "Nothing to deposit" },
        };

        public static readonly Dictionary<string, string> Russian = new Dictionary<string, string>
        {
            { "itemdrawers_wood", "Ящик для предметов" },
            { "itemdrawers_stone", "Каменный ящик для предметов" },
            { "itemdrawers_blackmarble", "Ящик для предметов из черного мрамора" },
            { "itemdrawers_description", "Вмещает очень много предметов одного вида." },
            { "itemdrawers_store_item", "Положить предмет" },
            { "itemdrawers_take_stack", "Взять стопку" },
            { "itemdrawers_take_one", "Взять один" },
            { "itemdrawers_unassign", "Отвязать" },
            { "itemdrawers_store_all", "Положить все" },
            { "itemdrawers_stored", "Положено:" },
            { "itemdrawers_try_again", "Попробуйте еще раз" },
            { "itemdrawers_stackable_only", "В ящик можно класть только складываемые предметы" },
            { "itemdrawers_unknown_item", "Не удается распознать этот предмет" },
            { "itemdrawers_remove_failed", "Не удалось забрать предмет" },
            { "itemdrawers_not_assigned", "Этому ящику не назначен предмет" },
            { "itemdrawers_none_carried", "У вас нет таких предметов" },
            { "itemdrawers_wrong_item", "В этом ящике лежит другой предмет" },
            { "itemdrawers_full", "Этот ящик полон" },
            { "itemdrawers_drained", "Этот ящик опустел" },
            { "itemdrawers_bad_capacity", "У этого ящика неверная вместимость" },
            { "itemdrawers_not_empty", "Сначала опустошите ящик" },
            { "itemdrawers_nothing", "Нечего положить" },
        };
    }
}
