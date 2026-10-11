namespace RisingLands;
public sealed partial class ResearchGame {
 void PrepareSavePersistenceCapture(){
  captureSlotDirectory=Path.GetFullPath(PackageLayout.Asset(root,"../../reports/v57-menu-save-tests/"+Guid.NewGuid().ToString("N")));
  OpenSlots(false);ActivateMenu(0);if(SlotStore.Existing(1)==null)throw new InvalidDataException("UI save did not create slot one");string first=SlotStore.Read(1);
  mapIndex=Array.FindIndex(maps,p=>Path.GetFileName(p)=="MONDE02.ETA");LoadMap();for(int i=0;i<21;i++)simulation.Step();OpenSlots(false);ActivateMenu(1);string second=SlotStore.Read(2);
  mapIndex=Array.FindIndex(maps,p=>Path.GetFileName(p)=="MONDE01.ETA");LoadMap();OpenSlots(true);
  if(slotLabels.Take(2).Any(s=>s.Contains("Vazio"))||!LoadGame(2)||simulation.World.MapName!="MONDE02.ETA"||simulation.Tick!=21||selected!=null||selectedSlots.Count!=0)throw new InvalidDataException("UI must find and restore another mission's slot after restart");
  if(!LoadGame(1)||simulation.Serialize()!=first)throw new InvalidDataException("UI must restore first mission from same five global slots");
  if(SlotStore.Read(2)!=second)throw new InvalidDataException("Loading another mission must preserve second saved file");
  string before=simulation.Serialize();string path=SlotStore.PathFor(2);File.WriteAllText(path,"{}");if(LoadGame(2)||simulation.Serialize()!=before)throw new InvalidDataException("Invalid map metadata must preserve current game");File.WriteAllText(path,second);
  OpenSlots(true);Console.WriteLine("PASS: actual menu saves two missions and restores either from the same five slots without rewriting files");
 }
}
