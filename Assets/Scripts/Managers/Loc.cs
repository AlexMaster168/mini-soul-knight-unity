using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

// Украинская локализация: весь текст игры собирается на английском (id предметов, оружия и т.д. остаются
// английскими), а перед отрисовкой Localizer прогоняет каждый Text / TextMesh через этот словарь.
public static class Loc
{
    static readonly string[] pairs =
    {
        // ---------- HUD ----------
        "WASD Move | Click Shoot | Space Dash | 1-8 Weapons | 9 Blade | Q Next | E Shop | Tab Stats | B Abilities",
        "WASD рух | Клік - стрільба | Пробіл - ривок | 1-8 зброя | 9 клинок | Q далі | E крамниця | Tab стати | B здібності",
        "Floor", "Поверх", "Lobby", "Лобі", "Score", "Рахунок", "Gold", "Золото", "Armor", "Броня",
        "NO ENERGY!", "НЕМАЄ ЕНЕРГІЇ!", "Low Energy!", "Мало енергії!", "PLAYER STATS", "ХАРАКТЕРИСТИКИ",
        "HP:", "Здоров'я:", "Energy:", "Енергія:", "Armor:", "Броня:", "Damage:", "Шкода:", "Fire Rate:", "Скорострільність:",
        "Fire rate:", "Скорострільність:", "Projectiles:", "Снаряди:", "Energy Cost:", "Витрата енергії:", "Speed:", "Швидкість:",
        "Weapon:", "Зброя:", "Arsenal:", "Арсенал:", "Gold:", "Золото:", "Floor:", "Поверх:", "Score:", "Рахунок:", "DPS:", "ШКД:",
        "weapons", "од. зброї", "READY", "ГОТОВО",
        "METEORS", "МЕТЕОРИ", "ANNIHILATE", "ЗНИЩЕННЯ", "ASCEND", "ВОЗНЕСІННЯ", "HEAL", "ЛІКУВАННЯ", "STORM", "ШТОРМ",
        "FREEZE", "ЗАМОРОЗКА", "SHIELD", "ЩИТ", "OVERDRIVE", "ФОРСАЖ",

        // ---------- баннеры / конец забега ----------
        "FLOOR CLEARED", "ПОВЕРХ ОЧИЩЕНО", "Step into the portal", "Зайди в портал", "FLOOR", "ПОВЕРХ", "BOSS", "БОС",
        "LOBBY", "ЛОБІ", "Pick your hero, then enter the portal", "Обери героя й зайди в портал",
        "YOU DIED", "ТИ ЗАГИНУВ", "YOU WIN!", "ПЕРЕМОГА!", "Boss defeated!", "Боса переможено!",
        "Reached Floor", "Досягнуто поверху", "crystals", "кристалів", "Complete!", "пройдено!",
        "RESTART [R]", "ЗАНОВО [R]", "PLAY AGAIN [R]", "ГРАТИ ЗНОВУ [R]",
        "Frozen Catacombs", "Крижані катакомби", "Sunken Abyss", "Затоплена безодня", "Void Sanctum", "Святилище порожнечі",
        "Infernal Citadel", "Пекельна цитадель", "Forgotten Crypt", "Забутий склеп",
        "The Crowned Boar", "Коронований вепр", "The Necromancer", "Некромант", "Ancient Dragon", "Стародавній дракон",
        "The Void Eye", "Око порожнечі", "The Golem King", "Король големів", "The Spider Queen", "Королева павуків",

        // ---------- старый магазин ----------
        "WEAPON SHOP", "КРАМНИЦЯ ЗБРОЇ", "Random Weapon", "Випадкова зброя", "Health +50", "Здоров'я +50", "Energy +80", "Енергія +80",
        "Armor +30", "Броня +30", "Damage x2 (8s)", "Шкода x2 (8с)", "Speed x1.5 (8s)", "Швидкість x1.5 (8с)",
        "FULL Health", "ПОВНЕ здоров'я", "FULL Energy", "ПОВНА енергія", "FULL Armor", "ПОВНА броня", "FULL RESTORE (all)", "ПОВНЕ ВІДНОВЛЕННЯ (усе)",
        "FULL", "ПОВНЕ", "Up/Down select  |  Left/Right page  |  Enter buy  |  F full restore  |  E close",
        "Вгору/Вниз - вибір  |  Вліво/Вправо - сторінка  |  Enter - купити  |  F - повне відновлення  |  E - закрити",
        "Press E to close", "Натисни E, щоб закрити", "[E] Shop", "[E] Крамниця",

        // ---------- торговец ----------
        "MERCHANT", "ТОРГОВЕЦЬ", "Weapons", "Зброя", "Super Abilities", "Супер-здібності", "Upgrades", "Покращення", "Robots", "Роботи",
        "Supplies", "Припаси", "Health", "Здоров'я", "Energy", "Енергія", "Page", "Стор.",
        "Up/Down select  |  Left/Right page/amount  |  T next tab  |  Enter buy  |  F buy max  |  E close",
        "Вгору/Вниз - вибір  |  Вліво/Вправо - сторінка/кількість  |  T - вкладка  |  Enter - купити  |  F - купити макс.  |  E - закрити",
        "[E] Merchant - weapons, SUPER ABILITIES, upgrades, robots", "[E] Торговець - зброя, СУПЕР-ЗДІБНОСТІ, покращення, роботи",
        "OWNED", "Є", "MAX", "МАКС", "EQUIPPED", "ОДЯГНЕНО", "Not enough gold", "Не вистачає золота",
        "You already own this weapon", "Ця зброя вже у тебе", "Bought", "Куплено", "You already have this", "Це вже у тебе",
        "Already at maximum level", "Вже максимальний рівень", "upgraded", "покращено", "levels", "рівнів",
        "Learned", "Вивчено", "joins you!", "приєднується до тебе!", "Already yours", "Вже твоє",
        "Maximum level reached", "Досягнуто максимального рівня", "Already full", "Вже повне",
        "Already in your arsenal", "Вже в твоєму арсеналі", "Slots full: replaces", "Слоти заповнені: замінить",
        "Ability slots are full", "Слоти здібностей заповнені", "Robots are sold from dungeon floor", "Роботи продаються з поверху",
        "(all the gold you had)", "(усе золото, що було)", "Price:", "Ціна:",
        "Supply", "Припас", "G per point", "G за очко", "Restores armor.", "Відновлює броню.", "Restores health.", "Відновлює здоров'я.",
        "Restores energy.", "Відновлює енергію.", "Amount:", "Кількість:", "Points:", "Очок:", "missing", "бракує", "Cost:", "Вартість:", "of", "з",
        "Left / Right - change amount", "Вліво / Вправо - змінити кількість", "F - buy everything at once (100%)", "F - купити все одразу (100%)",
        "Super Ability", "Супер-здібність", "cooldown", "перезарядка",
        "Activate with Z / X / C (in order of purchase). You can carry up to", "Активуй на Z / X / C (за порядком покупки). Можна мати до",
        "abilities.", "здібностей.", "Permanent upgrade", "Постійне покращення", "level", "рівень", "Lasts for the whole run.", "Діє до кінця забігу.",
        "Combat robot", "Бойовий робот", "dmg every", "шкоди кожні", "Fights on its own and follows you between floors.", "Б'ється сам і слідує за тобою між поверхами.",

        // ---------- оружие: редкость, класс, статы ----------
        "Legendary", "Легендарна", "Epic", "Епічна", "Rare", "Рідкісна", "Common", "Звичайна",
        "Pistol", "Пістолет", "Shotgun", "Дробовик", "SMG", "Пістолет-кулемет", "Rifle", "Гвинтівка", "Sniper", "Снайперка",
        "Heavy", "Важка", "Exotic", "Екзотична", "Melee", "Ближній бій",
        "per shot", "за постріл", "none", "немає",
        "- Pierces enemies", "- Пробиває ворогів", "- Explodes (radius", "- Вибухає (радіус", "- Returns after being thrown", "- Повертається після кидка",
        "- Projectiles zigzag", "- Снаряди рухаються зигзагом", "projectiles per shot", "снарядів за постріл", "- Uses no energy", "- Не витрачає енергію",
        "Reliable sidearm. Accurate and cheap on energy.", "Надійний пістолет. Точний і економний на енергію.",
        "Fires a spread of pellets. Devastating up close, weak at range.", "Стріляє віялом картечі. Нищівний зблизька, слабкий здалеку.",
        "Very fast fire rate, low damage per bullet. Sprays the room.", "Дуже висока скорострільність, мала шкода від кулі. Заливає кімнату свинцем.",
        "Steady automatic fire with good range and accuracy.", "Стабільний автоматичний вогонь, добра дальність і точність.",
        "Slow, precise, high-damage shots that fly far.", "Повільні, точні постріли з величезною шкодою, що летять далеко.",
        "Energy weapon with unusual projectiles.", "Енергетична зброя з незвичайними снарядами.",
        "Heavy ordnance. Big impact, big energy bill.", "Важке озброєння. Сильний удар, але й енергії тратить багато.",
        "Strange weapon with special behaviour.", "Дивна зброя з особливою поведінкою.",
        "Melee weapon: costs no energy and hits everything in front of you.", "Зброя ближнього бою: не витрачає енергію і б'є все перед тобою.",
        "Blessed explosive. Enormous blast that wipes out whole groups.", "Освячена вибухівка. Величезний вибух знищує цілі групи.",
        "Short-range cone of fire. Melts anything that gets close.", "Конус вогню на короткій відстані. Плавить усе, що підійшло близько.",
        "Endless stream of bullets. Drains energy fast.", "Нескінченний потік куль. Швидко висмоктує енергію.",
        "Explosive rocket with splash damage.", "Вибухова ракета зі шкодою по області.",
        "Charged slug that pierces every enemy in a line.", "Заряджений снаряд пробиває всіх ворогів на лінії.",
        "Thrown blade that flies out and returns to your hand.", "Метальне лезо, що летить уперед і повертається до руки.",
        "Razor ring that slices through enemies and comes back.", "Бритвене кільце розрізає ворогів і повертається.",
        "Slow ground-pound. Smashes an area on impact.", "Повільний удар по землі. Трощить усе навколо при влучанні.",
        "Lightning-fast stabs at very short range.", "Блискавичні удари на дуже малій відстані.",
        "Long reach thrust that pierces enemies in a line.", "Довгий випад, що пробиває ворогів на лінії.",
        "Crackling stars that zigzag toward enemies.", "Тріскучі зірки, що летять до ворогів зигзагом.",
        "Spread of lightning bolts that arc unpredictably.", "Віяло блискавок, що б'ють непередбачувано.",
        "Fires twin ice shards.", "Стріляє подвійними крижаними уламками.",
        "Spits toxic globs in a wide spread.", "Плюється токсичними згустками широким віялом.",
        "Nine pellets of pure chaos. Recoil will hurt your pride.", "Дев'ять шматків чистого хаосу. Віддача вдарить по самолюбству.",
        "Shiny, accurate, and expensive-looking.", "Блискуча, точна і дорога на вигляд.",
        "Emergency blade. Free to use when you run out of energy.", "Аварійний клинок. Безкоштовний, коли закінчилась енергія.",
        "You already own this - picking it up gives 25 gold", "Вона вже є - підбір дасть 25 золота",
        "Step closer to pick it up", "Підійди ближче, щоб підняти", "[E] Swap with current weapon", "[E] Замінити поточну зброю",
        "[E] swap current weapon for", "[E] замінити поточну зброю на",

        // ---------- названия оружия ----------
        "Dual", "Двійники", "Revolver", "Револьвер", "DesertEagle", "Дезерт Іґл", "SuperShotgun", "Супердробовик", "TacticalSG", "Тактичний дробовик",
        "Uzi", "Узі", "Thompson", "Томпсон", "AK", "АК", "LMG", "Ручний кулемет", "Famas", "Фамас", "AWP", "АВП", "Crossbow", "Арбалет",
        "LaserRifle", "Лазерна гвинтівка", "Laser", "Лазер", "Plasma", "Плазма", "Shock", "Шок", "Thunder", "Грім",
        "IceGun", "Крижана гармата", "PoisonGun", "Отруйна гармата", "Rocket", "Ракета", "Flamethrower", "Вогнемет",
        "HolyGrenade", "Свята граната", "GrenadeLauncher", "Гранатомет", "Minigun", "Мінігун", "Boomerang", "Бумеранг",
        "Star Wand", "Зіркова паличка", "FairyGun", "Феєрична гармата", "Sword", "Меч", "Katana", "Катана", "Mace", "Булава",
        "Axe", "Сокира", "Scythe", "Коса", "RustyBlade", "Іржавий клинок", "GoldenGun", "Золота гармата", "Vector", "Вектор",
        "SCAR", "СКАР", "Blunderbuss", "Мушкетон", "Railgun", "Рейкган", "ArcRifle", "Дугова гвинтівка", "VoidBeam", "Промінь порожнечі",
        "Spear", "Спис", "Dagger", "Кинджал", "Hammer", "Молот", "Chakram", "Чакрам", "ShurikenFan", "Віяло сюрікенів",

        // ---------- супер-способности ----------
        "Meteor Storm", "Метеорний шторм", "Annihilation", "Знищення", "Ascension", "Вознесіння", "Nova", "Нова",
        "Life Surge", "Сплеск життя", "Bullet Storm", "Шторм куль", "Time Freeze", "Зупинка часу", "Guardian Shield", "Щит охоронця", "Overdrive", "Форсаж",
        "Calls down 16 meteors over 3 seconds on the enemies' heads. Each blast deals 260 damage in a wide area.",
        "Скидає 16 метеоритів за 3 секунди прямо на голови ворогів. Кожен вибух завдає 260 шкоди по великій області.",
        "Wipes out every enemy of the current wave. Bosses lose 30% of their max HP.", "Знищує всіх ворогів поточної хвилі. Боси втрачають 30% макс. здоров'я.",
        "10 seconds of godhood: invulnerable, x3 damage, free shots.", "10 секунд божественності: невразливість, x3 шкоди, безкоштовні постріли.",
        "Shockwave that blasts every enemy around you for heavy damage and wipes all enemy bullets.", "Ударна хвиля кидає всіх ворогів навколо, завдає важкої шкоди і стирає всі ворожі кулі.",
        "Instantly restores 80 HP and grants 40 armor.", "Миттєво відновлює 80 здоров'я і дає 40 броні.",
        "Fires a ring of 24 shots of your current weapon in every direction.", "Випускає кільце з 24 пострілів поточної зброї в усі боки.",
        "Freezes every enemy in place for 4 seconds and clears enemy bullets.", "Заморожує всіх ворогів на місці на 4 секунди і стирає ворожі кулі.",
        "Total invulnerability for 5 seconds.", "Повна невразливість на 5 секунд.",
        "For 6 seconds weapons cost no energy and fire 60% faster.", "На 6 секунд зброя не витрачає енергію і стріляє на 60% швидше.",

        // ---------- улучшения и роботы ----------
        "Armor Plating", "Броньована пластина", "Vitality", "Життєвість", "Firepower", "Вогнева міць",
        "Reinforced protection. Each level: +30 max armor (filled instantly) and -6% damage from every hit.",
        "Посилений захист. Кожен рівень: +30 макс. броні (заповнюється одразу) і -6% шкоди від кожного удару.",
        "Each level: +30 max HP and heals you for 30.", "Кожен рівень: +30 макс. здоров'я і лікує на 30.",
        "Each level: +15% damage with every weapon, robots included.", "Кожен рівень: +15% шкоди з будь-якої зброї, роботів теж.",
        "Scout Bot", "Розвідник", "Gunner Bot", "Стрілець", "Plasma Bot", "Плазмобот",
        "Small hover drone. Follows you and automatically shoots the nearest enemy.", "Маленький дрон. Летить за тобою і сам стріляє в найближчого ворога.",
        "Rapid-fire turret drone. Melts anything that gets close to you.", "Скорострільний дрон-турель. Плавить усе, що наблизилось до тебе.",
        "Heavy drone. Its plasma orbs pierce through whole lines of enemies.", "Важкий дрон. Плазмові кулі пробивають цілі шеренги ворогів.",

        // ---------- лобби, герои, скины ----------
        "[E] Hero Master - heroes & skins", "[E] Майстер героїв - герої та скіни", "Step into the portal to START THE RUN", "Зайди в портал, щоб ПОЧАТИ ЗАБІГ",
        "HERO MASTER", "МАЙСТЕР ГЕРОЇВ", "Heroes", "Герої", "Skins", "Скіни", "Crystals:", "Кристали:", "Hero  -  female", "Героїня", "Hero  -  male", "Герой",
        "Skin - works on every hero", "Скін - підходить кожному герою", "Changes the colors of your hero.", "Змінює кольори твого героя.", "Boys get different trousers,", "Хлопці отримують інші штани,", "girls get different hair and a different skirt.", "дівчата - інше волосся й іншу спідницю.",
        "Equipped", "Одягнено", "Press Enter to equip", "Натисни Enter, щоб одягнути", "(Enter to buy)", "(Enter - купити)",
        "Not enough crystals (earn them by playing runs)", "Не вистачає кристалів (заробляй їх у забігах)", "Unlocked", "Відкрито",
        "Up/Down select  |  T heroes / skins  |  G boy / girl  |  Enter buy or equip  |  E close", "Вгору/Вниз - вибір  |  T - герої / скіни  |  G - хлопець / дівчина  |  Enter - купити чи одягнути  |  E - закрити",
        "Boy", "Хлопець", "Girl", "Дівчина",
        "Start armor:", "Початкова броня:", "Damage taken: -", "Отримувана шкода: -", "Companion:", "Компаньйон:", "Energy regen", "Відновлення енергії",
        "Dash cooldown", "Перезарядка ривка", "Starting weapon:", "Стартова зброя:", "Speed", "Швидкість", "/s", "/с",
        "Knight", "Лицар", "Rogue", "Розбійник", "Mage", "Маг", "Tank", "Танк", "Engineer", "Інженер",
        "Valkyrie", "Валькірія", "Assassin", "Асасинка", "Sorceress", "Чарівниця", "Berserker", "Берсерка", "Mechanic", "Механікиня",
        "Ranger", "Рейнджер", "Huntress", "Мисливиця", "Pyromancer", "Піромант", "Ember", "Жарина", "Samurai", "Самурай", "Sakura", "Сакура",
        "Sharpshooter: +10% damage, quick on his feet. Starts with a Crossbow.", "Стрілець: +10% шкоди, моторний. Стартує з арбалетом.",
        "Fleet-footed huntress: +15% damage, faster energy regeneration. Starts with a Sniper rifle.", "Прудка мисливиця: +15% шкоди, швидше відновлює енергію. Стартує зі снайперкою.",
        "Fire mage: +20% damage and a big energy pool. Starts with a Flamethrower.", "Вогняний маг: +20% шкоди і великий запас енергії. Стартує з вогнеметом.",
        "Burning sorceress: +25% damage, huge energy pool. Starts with a Grenade Launcher.", "Палаюча чарівниця: +25% шкоди, величезний запас енергії. Стартує з гранатометом.",
        "Disciplined blade master: 180 HP, 30 armor, fast dash, +15% damage. Starts with a Katana.", "Дисциплінований майстер клинка: 180 здоров'я, 30 броні, швидкий ривок, +15% шкоди. Стартує з катаною.",
        "Graceful warrior: very fast dash and +20% damage. Starts with a Chakram.", "Витончена воїтелька: дуже швидкий ривок і +20% шкоди. Стартує з чакрамом.",
        "Cleric", "Клірик", "Saint", "Свята", "Pirate", "Пірат", "Corsair", "Корсарка", "Shinobi", "Шинобі", "Kunoichi", "Кунойчі",
        "Holy warrior: 190 HP, 30 armor, -12% damage taken, fast energy regeneration. Starts with a Mace.", "Святий воїн: 190 здоров'я, 30 броні, -12% отримуваної шкоди, швидке відновлення енергії. Стартує з булавою.",
        "Sea dog: 170 HP, +15% damage and a blunderbuss that clears the deck.", "Морський вовк: 170 здоров'я, +15% шкоди і мушкетон, що розчищає палубу.",
        "Silent shadow: the fastest runner, very short dash cooldown. Starts with a Shock star gun.", "Безшумна тінь: найшвидший бігун, дуже короткий час перезарядки ривка. Стартує зі Шок-зорями.",
        "Radiant priestess: huge energy pool, -6% damage taken, +10% damage. Starts with an IceGun.", "Сяюча жриця: величезний запас енергії, -6% отримуваної шкоди, +10% шкоди. Стартує з крижаною гарматою.",
        "Daring pirate captain: quick dash, +20% damage. Starts with a DesertEagle.", "Відчайдушна піратська капітанка: швидкий ривок, +20% шкоди. Стартує з Дезерт Іґлом.",
        "Deadly shadow dancer: top speed, dash almost always ready, +15% damage. Starts with a ShurikenFan.", "Смертельна танцівниця тіней: максимальна швидкість, ривок майже завжди готовий, +15% шкоди. Стартує з віялом сюрікенів.",
        "Blossom", "Цвіт вишні", "Ocean", "Океан", "Forest", "Ліс", "Royal", "Королівський", "Lava", "Лава", "Snow", "Сніг", "Candy", "Цукерка", "Midnight", "Опівніч",
        "Schoolboy", "Школяр", "Schoolgirl", "Школярка",
        "Navy Blazer", "Синій піджак", "Sailor", "Матроска", "Gray Blazer", "Сірий піджак", "Sport", "Спорт", "Winter Coat", "Зимове пальто", "Cardigan", "Кардиган",
        "Cheerful student in a school uniform: quick, nimble. Starts with a Boomerang.", "Життєрадісний учень у шкільній формі: швидкий і моторний. Стартує з бумерангом.",
        "Clever student in a school uniform: quick, with extra energy. Starts with a Star Wand.", "Розумна учениця у шкільній формі: швидка, з додатковою енергією. Стартує із Зірковою паличкою.",
        "USED", "ВЖЕ", "Already used in this room", "Вже використано в цій кімнаті", "Can be used only once in each room.", "Можна використати лише один раз у кожній кімнаті.",
        "Unlocks from dungeon floor", "Відкривається з поверху",
        "Energy Core", "Енергоядро", "Recharge", "Підзарядка", "Swiftness", "Прудкість", "Dash Drive", "Привід ривка",
        "Each level: +60 max energy and restores 60 energy.", "Кожен рівень: +60 макс. енергії й відновлює 60 енергії.",
        "Each level: +1.5 energy regeneration per second.", "Кожен рівень: +1.5 відновлення енергії за секунду.",
        "Each level: +5% movement speed.", "Кожен рівень: +5% швидкості руху.",
        "Each level: the dash recharges 8% faster.", "Кожен рівень: ривок перезаряджається на 8% швидше.",
        "Clear all rooms on this floor to open the boss door", "Зачисти всі кімнати поверху, щоб відкрити двері боса", "The boss door is open!", "Двері боса відчинено!",
        "16 meteors on enemies", "16 метеоритів на ворогів", "Kills the current wave", "Знищує поточну хвилю", "Godmode for 10s", "Режим бога на 10с",
        "Blast and clear bullets", "Вибух і стирає кулі", "+80 HP and +40 armor", "+80 здоров'я і +40 броні", "Ring of 24 shots", "Кільце з 24 пострілів",
        "Freezes enemies 4s", "Заморожує ворогів на 4с", "Invulnerable for 5s", "Невразливість на 5с", "Free, fast fire for 6s", "Безкоштовна швидка стрільба 6с",
        "SUPER ABILITIES", "СУПЕР-ЗДІБНОСТІ", "B / Esc - close  |  Z / X / C - use  |  each ability works once per room", "B / Esc - закрити  |  Z / X / C - використати  |  кожна здібність діє раз на кімнату",
        "Empty slot", "Порожній слот", "Buy super abilities from a merchant (E). They cost a lot but are extremely powerful.", "Купуй супер-здібності в торговця (E). Вони дорогі, але надзвичайно потужні.",
        "Cooldown", "Перезарядка", "[E] Open the door", "[E] Відчинити двері",
        "Rampage!", "Несамовитий натиск!", "Army of the Dead!", "Армія мерців!", "Inferno!", "Пекло!", "Earthquake!", "Землетрус!", "Broodstorm!", "Буря виводку!", "Singularity!", "Сингулярність!",
        "The boss is enraged!", "Бос у люті!", "The boss grows stronger!", "Бос стає сильнішим!",
        "Classic", "Класика", "Crimson", "Багряний", "Emerald", "Смарагдовий", "Shadow", "Тінь", "Frost", "Іній", "Sunset", "Захід сонця", "Neon", "Неон", "Golden", "Золотий",
        "Balanced hero. Sturdy, reliable, no tricks.", "Збалансований герой. Міцний, надійний, без фокусів.",
        "Fast and slippery: quick dash, high speed, starts with a dagger.", "Швидкий і вертикий: швидкий ривок, висока швидкість, стартує з кинджалом.",
        "Huge energy pool with fast regeneration and +15% damage. Starts with a Star Wand.", "Величезний запас енергії зі швидким відновленням і +15% шкоди. Стартує із Зірковою паличкою.",
        "Slow but very tough: 260 HP, 60 armor, -10% damage taken. Starts with a Shotgun.", "Повільний, але дуже міцний: 260 здоров'я, 60 броні, -10% отримуваної шкоди. Стартує з дробовиком.",
        "Starts every run with a Scout Bot companion and a Laser.", "Кожен забіг стартує з роботом-розвідником і лазером.",
        "Warrior maiden: a bit faster and starts with a long-reach Spear.", "Дівчина-воїн: трохи швидша і стартує з довгим списом.",
        "Deadly and swift: the fastest dash in the game, +10% damage, starts with a Katana.", "Смертельна й швидка: найшвидший ривок у грі, +10% шкоди, стартує з катаною.",
        "Enormous energy pool, very fast regeneration, +20% damage. Starts with a FairyGun.", "Величезний запас енергії, дуже швидке відновлення, +20% шкоди. Стартує з феєричною гарматою.",
        "Tough fighter: 240 HP, 40 armor, -8% damage taken, +10% damage. Starts with an Axe.", "Витривала бійчиня: 240 здоров'я, 40 броні, -8% отримуваної шкоди, +10% шкоди. Стартує із сокирою.",
        "Starts every run with a rapid-fire Gunner Bot and a Plasma gun.", "Кожен забіг стартує зі скорострільним роботом-стрільцем і плазмовою зброєю.",
    };

    // Язык интерфейса: true - украинский (по умолчанию), false - английский
    public static bool Ukrainian
    {
        get { return PlayerPrefs.GetInt("lang_uk", 1) == 1; }
        set { PlayerPrefs.SetInt("lang_uk", value ? 1 : 0); PlayerPrefs.Save(); }
    }

    static Dictionary<string, string> map;
    static Regex rx;
    static Regex secRx;
    static readonly Dictionary<string, string> memo = new Dictionary<string, string>();

    static void Build()
    {
        map = new Dictionary<string, string>();
        for (int i = 0; i + 1 < pairs.Length; i += 2)
            map[pairs[i]] = pairs[i + 1];

        List<string> keys = new List<string>(map.Keys);
        keys.Sort((a, b) => b.Length.CompareTo(a.Length));
        StringBuilder sb = new StringBuilder();
        sb.Append("(?<![A-Za-z])(?:");
        for (int i = 0; i < keys.Count; i++)
        {
            if (i > 0) sb.Append('|');
            sb.Append(Regex.Escape(keys[i]));
        }
        sb.Append(")(?![A-Za-z])");
        rx = new Regex(sb.ToString(), RegexOptions.CultureInvariant);
        secRx = new Regex(@"(?<=\d)s(?![A-Za-z])", RegexOptions.CultureInvariant);
    }

    public static string T(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        if (map == null) Build();

        string r;
        if (memo.TryGetValue(s, out r)) return r;

        r = rx.Replace(s, m => map[m.Value]);
        r = secRx.Replace(r, "с");
        if (memo.Count > 4000) memo.Clear();
        memo[s] = r;
        return r;
    }
}

// Каждый кадр приводит все активные тексты к выбранному языку. Английский оригинал запоминается,
// поэтому язык можно переключать на лету, даже для надписей, которые игра задаёт один раз.
public class Localizer : MonoBehaviour
{
    private class Entry { public string orig, shown; }

    private readonly Dictionary<Object, Entry> entries = new Dictionary<Object, Entry>();
    private Text[] texts = new Text[0];
    private TextMesh[] meshes = new TextMesh[0];
    private float nextScan;

    // возвращает текст, который должен отображаться; orig обновляется, если игра сама поменяла надпись
    string Resolve(Object key, string current)
    {
        Entry e;
        if (!entries.TryGetValue(key, out e)) { e = new Entry(); entries[key] = e; e.orig = current; }
        else if (current != e.shown) e.orig = current;

        string want = Loc.Ukrainian ? Loc.T(e.orig) : e.orig;
        e.shown = want;
        return want;
    }

    void LateUpdate()
    {
        if (Time.unscaledTime >= nextScan)
        {
            nextScan = Time.unscaledTime + 0.4f;
            texts = FindObjectsByType<Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            meshes = FindObjectsByType<TextMesh>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (entries.Count > 2000)
            {
                List<Object> dead = new List<Object>();
                foreach (var kv in entries) if (kv.Key == null) dead.Add(kv.Key);
                foreach (Object o in dead) entries.Remove(o);
            }
        }

        for (int i = 0; i < texts.Length; i++)
        {
            Text t = texts[i];
            if (t == null || !t.gameObject.activeInHierarchy) continue;
            string s = t.text;
            string r = Resolve(t, s);
            if (r != s) t.text = r;
        }

        for (int i = 0; i < meshes.Length; i++)
        {
            TextMesh m = meshes[i];
            if (m == null) continue;
            string s = m.text;
            string r = Resolve(m, s);
            if (r != s) m.text = r;
        }
    }
}
