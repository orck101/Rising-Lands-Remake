namespace RisingLands;
public sealed partial class Simulation {
 // Original MAJSympathie events 10 + 6, followed by PlayerResetAllie.
 void RegisterAllianceAttack(int attacker,int victim){if(attacker!=0||victim is <1 or >3||!Allied(victim))return;Campaign.Goodwill[victim]=Math.Clamp(Goodwill(victim)-35,-50,50);Campaign.Alliances[victim]=false;UpdateVisibility();}
 // Historical unit penalty (-3) is disabled by the requested remake policy.
 void RegisterUnitDamage(int attacker,int victim){}
 // Historical global structure penalty (-50) is disabled, including unrelated clans.
 void RegisterStructureDamage(int attacker,Building victim,int before,int after){}
}
