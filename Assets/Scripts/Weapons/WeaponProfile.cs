using UnityEngine;

public enum WeaponClass { Pistol, Shotgun, SMG, Rifle, Sniper, Energy, Heavy, Exotic, Melee }

// Как выглядит пуля оружия и как оно "ведёт себя" в руках (отдача, взмах и т.д.)
public class WeaponProfile
{
    public WeaponClass cls;
    public BulletStyle style = BulletStyle.Round;
    public Color color = new Color(1f, 0.9f, 0.3f);
    public float lifetime = 2f;
    public float aoeRadius = 0f;
    public bool boomerang;
    public float wobble;        // синусоидальное колебание траектории (молнии)
    public float kick = 0.15f;  // отдача оружия назад
    public float rise = 10f;    // подброс ствола, градусы
    public float recover = 14f; // скорость возврата
    public bool trail;

    public static WeaponClass GetClass(string w)
    {
        switch (w)
        {
            case "Pistol": case "Dual": case "Revolver": case "DesertEagle": case "GoldenGun": return WeaponClass.Pistol;
            case "Shotgun": case "SuperShotgun": case "TacticalSG": case "Blunderbuss": return WeaponClass.Shotgun;
            case "Uzi": case "Mac10": case "Thompson": case "Vector": return WeaponClass.SMG;
            case "AK": case "LMG": case "M4": case "Famas": case "SCAR": return WeaponClass.Rifle;
            case "Sniper": case "AWP": case "Crossbow": case "Railgun": return WeaponClass.Sniper;
            case "LaserRifle": case "Laser": case "Plasma": case "Shock": case "Thunder": case "IceGun": case "PoisonGun": case "ArcRifle": case "VoidBeam": return WeaponClass.Energy;
            case "Rocket": case "Flamethrower": case "HolyGrenade": case "GrenadeLauncher": case "Minigun": return WeaponClass.Heavy;
            case "Boomerang": case "Star Wand": case "FairyGun": case "Chakram": case "ShurikenFan": return WeaponClass.Exotic;
            case "Sword": case "Katana": case "Mace": case "Axe": case "Scythe": case "Spear": case "Dagger": case "Hammer": return WeaponClass.Melee;
            default: return WeaponClass.Pistol;
        }
    }

    public static WeaponProfile Get(string w)
    {
        WeaponProfile p = new WeaponProfile { cls = GetClass(w) };
        switch (w)
        {
            case "Pistol": p.style = BulletStyle.Slug; p.kick = 0.14f; p.rise = 12f; break;
            case "Dual": p.style = BulletStyle.Slug; p.color = new Color(0.6f, 0.95f, 1f); p.kick = 0.1f; p.rise = 8f; break;
            case "Revolver": p.style = BulletStyle.Slug; p.color = new Color(1f, 0.7f, 0.3f); p.kick = 0.22f; p.rise = 20f; break;
            case "DesertEagle": p.style = BulletStyle.Slug; p.color = new Color(1f, 0.6f, 0.2f); p.kick = 0.3f; p.rise = 26f; break;

            case "Shotgun": p.color = new Color(1f, 0.75f, 0.3f); p.lifetime = 0.55f; p.kick = 0.35f; p.rise = 22f; p.recover = 9f; break;
            case "SuperShotgun": p.color = new Color(1f, 0.55f, 0.2f); p.lifetime = 0.5f; p.kick = 0.45f; p.rise = 28f; p.recover = 8f; break;
            case "TacticalSG": p.color = new Color(0.7f, 0.9f, 1f); p.lifetime = 0.6f; p.kick = 0.3f; p.rise = 16f; break;

            case "Uzi": p.style = BulletStyle.Slug; p.kick = 0.07f; p.rise = 4f; p.recover = 30f; break;
            case "Mac10": p.style = BulletStyle.Slug; p.color = new Color(1f, 0.95f, 0.5f); p.kick = 0.05f; p.rise = 3f; p.recover = 35f; break;
            case "Thompson": p.style = BulletStyle.Slug; p.color = new Color(1f, 0.8f, 0.4f); p.kick = 0.08f; p.rise = 5f; p.recover = 28f; break;

            case "AK": p.style = BulletStyle.Streak; p.color = new Color(1f, 0.8f, 0.3f); p.kick = 0.1f; p.rise = 7f; p.recover = 24f; break;
            case "LMG": p.style = BulletStyle.Streak; p.color = new Color(1f, 0.85f, 0.4f); p.kick = 0.09f; p.rise = 5f; p.recover = 26f; break;
            case "M4": p.style = BulletStyle.Streak; p.color = new Color(1f, 0.95f, 0.6f); p.kick = 0.08f; p.rise = 5f; p.recover = 26f; break;
            case "Famas": p.style = BulletStyle.Streak; p.color = new Color(0.8f, 1f, 0.6f); p.kick = 0.08f; p.rise = 4f; p.recover = 26f; break;

            case "Sniper": p.style = BulletStyle.Streak; p.color = new Color(1f, 1f, 0.85f); p.kick = 0.45f; p.rise = 18f; p.recover = 5f; p.lifetime = 1.2f; break;
            case "AWP": p.style = BulletStyle.Streak; p.color = new Color(0.7f, 1f, 0.7f); p.kick = 0.55f; p.rise = 24f; p.recover = 4f; p.lifetime = 1.2f; break;
            case "Crossbow": p.style = BulletStyle.Bolt; p.color = new Color(0.8f, 0.5f, 0.25f); p.kick = 0.2f; p.rise = 4f; p.recover = 7f; p.lifetime = 1.2f; break;

            case "LaserRifle": p.style = BulletStyle.Laser; p.color = new Color(0.3f, 0.85f, 1f); p.kick = 0.05f; p.rise = 2f; p.trail = true; break;
            case "Laser": p.style = BulletStyle.Laser; p.color = new Color(0.4f, 1f, 0.9f); p.kick = 0.03f; p.rise = 1f; break;
            case "Plasma": p.style = BulletStyle.Orb; p.color = new Color(0.6f, 0.3f, 1f); p.kick = 0.12f; p.rise = 6f; p.trail = true; break;
            case "Shock": p.style = BulletStyle.Star; p.color = new Color(1f, 0.95f, 0.3f); p.kick = 0.08f; p.rise = 4f; p.wobble = 8f; break;
            case "Thunder": p.style = BulletStyle.Streak; p.color = new Color(1f, 1f, 0.5f); p.kick = 0.12f; p.rise = 6f; p.wobble = 14f; p.trail = true; break;
            case "IceGun": p.style = BulletStyle.Shard; p.color = new Color(0.5f, 0.85f, 1f); p.kick = 0.06f; p.rise = 3f; p.trail = true; break;
            case "PoisonGun": p.style = BulletStyle.Drop; p.color = new Color(0.4f, 0.95f, 0.25f); p.kick = 0.06f; p.rise = 3f; p.wobble = 4f; break;

            case "Rocket": p.style = BulletStyle.Rocket; p.aoeRadius = 2.6f; p.kick = 0.5f; p.rise = 20f; p.recover = 6f; p.trail = true; break;
            case "Flamethrower": p.style = BulletStyle.Flame; p.color = new Color(1f, 0.55f, 0.1f); p.lifetime = 0.4f; p.kick = 0.03f; p.rise = 2f; p.trail = false; break;
            case "HolyGrenade": p.style = BulletStyle.Grenade; p.color = new Color(1f, 0.9f, 0.4f); p.aoeRadius = 4.2f; p.lifetime = 0.8f; p.kick = 0.4f; p.rise = 16f; p.recover = 6f; break;
            case "GrenadeLauncher": p.style = BulletStyle.Grenade; p.color = new Color(0.4f, 0.75f, 0.35f); p.aoeRadius = 3f; p.lifetime = 0.9f; p.kick = 0.35f; p.rise = 14f; p.recover = 7f; break;
            case "Minigun": p.style = BulletStyle.Slug; p.color = new Color(1f, 0.85f, 0.35f); p.kick = 0.04f; p.rise = 2f; p.recover = 40f; break;

            case "Boomerang": p.style = BulletStyle.Boomerang; p.color = new Color(1f, 0.6f, 0.15f); p.boomerang = true; p.lifetime = 1.3f; p.kick = 0f; p.rise = 0f; break;
            case "Star Wand": p.style = BulletStyle.Star; p.color = new Color(1f, 0.85f, 0.35f); p.kick = 0.05f; p.rise = 8f; p.trail = true; break;
            case "FairyGun": p.style = BulletStyle.Orb; p.color = new Color(1f, 0.6f, 0.9f); p.kick = 0.06f; p.rise = 6f; p.wobble = 5f; p.trail = true; break;

            case "Sword": p.style = BulletStyle.Slash; p.color = new Color(0.85f, 0.92f, 1f); p.lifetime = 0.16f; break;
            case "Katana": p.style = BulletStyle.Slash; p.color = new Color(0.7f, 1f, 1f); p.lifetime = 0.14f; break;
            case "Mace": p.style = BulletStyle.Slash; p.color = new Color(1f, 0.75f, 0.4f); p.lifetime = 0.2f; break;
            case "Axe": p.style = BulletStyle.Slash; p.color = new Color(1f, 0.6f, 0.4f); p.lifetime = 0.18f; break;
            case "Scythe": p.style = BulletStyle.Slash; p.color = new Color(0.8f, 0.5f, 1f); p.lifetime = 0.22f; break;

            case "GoldenGun": p.style = BulletStyle.Slug; p.color = new Color(1f, 0.85f, 0.2f); p.kick = 0.16f; p.rise = 12f; p.trail = true; break;
            case "Vector": p.style = BulletStyle.Slug; p.color = new Color(1f, 0.5f, 0.4f); p.kick = 0.04f; p.rise = 2f; p.recover = 40f; break;
            case "SCAR": p.style = BulletStyle.Streak; p.color = new Color(1f, 0.9f, 0.5f); p.kick = 0.1f; p.rise = 6f; p.recover = 24f; break;
            case "Blunderbuss": p.color = new Color(0.9f, 0.7f, 0.4f); p.lifetime = 0.5f; p.kick = 0.55f; p.rise = 32f; p.recover = 7f; break;
            case "Railgun": p.style = BulletStyle.Laser; p.color = new Color(0.6f, 1f, 1f); p.lifetime = 1f; p.kick = 0.6f; p.rise = 10f; p.recover = 4f; p.trail = true; break;
            case "ArcRifle": p.style = BulletStyle.Streak; p.color = new Color(0.5f, 0.8f, 1f); p.wobble = 12f; p.kick = 0.06f; p.rise = 3f; p.trail = true; break;
            case "VoidBeam": p.style = BulletStyle.Laser; p.color = new Color(0.7f, 0.3f, 1f); p.kick = 0.04f; p.rise = 2f; p.trail = true; break;
            case "Chakram": p.style = BulletStyle.Boomerang; p.color = new Color(0.8f, 0.9f, 1f); p.boomerang = true; p.lifetime = 1.0f; p.kick = 0f; p.rise = 0f; break;
            case "ShurikenFan": p.style = BulletStyle.Star; p.color = new Color(0.85f, 0.9f, 1f); p.kick = 0.06f; p.rise = 4f; break;

            case "Spear": p.style = BulletStyle.Shard; p.color = new Color(0.85f, 0.95f, 1f); p.lifetime = 0.24f; break;
            case "Dagger": p.style = BulletStyle.Slash; p.color = new Color(0.9f, 0.9f, 1f); p.lifetime = 0.1f; break;
            case "Hammer": p.style = BulletStyle.Slash; p.color = new Color(1f, 0.6f, 0.3f); p.lifetime = 0.2f; p.aoeRadius = 2.2f; break;
        }
        return p;
    }
}
