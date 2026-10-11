using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
namespace RisingLands;

public enum MenuPage { Closed, Main, Controls, Exit, Restart, Objectives, Save, Load, ConfirmOverwrite, Audio }
public sealed class PauseMenuState {
    public MenuPage Page{get;private set;}
    public int Choice{get;private set;}
    public bool Open=>Page!=MenuPage.Closed;
    public void Show(MenuPage page){Page=page;Choice=0;}
    public void Back()=>Show(Page==MenuPage.Main?MenuPage.Closed:MenuPage.Main);
    public void Move(int delta,int count)=>Choice=(Choice+delta+count)%count;
}

public sealed partial class ResearchGame {
    Texture2D nativeMenu=null!;
    readonly PauseMenuState pauseMenu=new();
    float MenuScale=>Math.Min(GraphicsDevice.Viewport.Width/800f,GraphicsDevice.Viewport.Height/600f);
    Rectangle NativeMenuBounds=>new((GraphicsDevice.Viewport.Width-(int)(800*MenuScale))/2,(GraphicsDevice.Viewport.Height-(int)(600*MenuScale))/2,(int)(800*MenuScale),(int)(600*MenuScale));
    Rectangle MenuRow(int index)=>new(NativeMenuBounds.X+(int)(200*MenuScale),NativeMenuBounds.Y+(int)((286+index*25)*MenuScale),(int)(400*MenuScale),(int)(22*MenuScale));
    void MenuText(string value,int y,Color color,float size=1){var scale=MenuScale*.8f*size;var width=font.MeasureString(value).X*scale;Text(value,(int)(NativeMenuBounds.Center.X-width/2),(int)(NativeMenuBounds.Y+y*MenuScale),color,scale);}
    string menuStatus="";
    Rectangle MenuPanel=>pauseMenu.Page==MenuPage.Objectives?new((GraphicsDevice.Viewport.Width-680)/2,(GraphicsDevice.Viewport.Height-650)/2,680,650):new((GraphicsDevice.Viewport.Width-600)/2,(GraphicsDevice.Viewport.Height-580)/2,600,580);
    Rectangle MenuButton(int index)=>pauseMenu.Page==MenuPage.Main?MainMenuButton(index):MenuRow(index);
    string[] MenuLabels=>pauseMenu.Page switch{MenuPage.Main=>new[]{"Continuar","Salvar partida","Carregar partida","Objetivo da missao","Controles","Reiniciar partida","Sair do jogo","Ajustes de som"},MenuPage.Restart=>new[]{"Voltar sem reiniciar","Reiniciar do inicio"},MenuPage.Exit=>new[]{"Salvar e sair","Sair sem salvar","Voltar"},MenuPage.ConfirmOverwrite=>new[]{"Cancelar","Substituir partida salva"},MenuPage.Save or MenuPage.Load=>slotLabels,_=>new[]{"Voltar"}};
static readonly string[] ControlLines={"Clique esquerdo: selecionar / ordenar","Arrastar: selecionar grupo | Shift: adicionar","Clique direito: cancelar / desfazer selecao","Ctrl mantido: fonte e depois entrega","Soltar Ctrl antes da entrega: cancelar","Carga: Ctrl + destino para redirecionar","Fonte: comida, pedra ou engrenagens por oficio","Armazem: Ctrl para retirar o recurso do oficio","Colono: clique no predio para entrar em grupo","Duplo clique no predio: retirar um colono","Construtor + clique na obra: ajudar a construir","B: construcoes | R: retomar obra","Muralha: segure esquerdo e arraste o trecho","M: escolher mina | E: armazem | L: chao","WASD, setas ou bordas do cenario: camera","Roda: zoom | Home: centralizar selecionado","Segurar roda e arrastar: mover camera","Minimapa: direito + arrastar para reposicionar","Santuario: personagem entra e volta a colono","Slider: velocidade de 1x a 3x","Tab: mostrar ou esconder minimapa","Pesquisa concluida: clique / Enter / Esc","So combate: quadrado / linha / triangulo","Formacao: maior alcance fica no centro","Backspace: parar | Espaco: ver vida","Ctrl+1-9: gravar grupo | 1-9: recuperar","Shift+1-9: recuperar e centralizar","G/H: formar fazendeiro / construtor | T: pesquisa","F5: escolher slot para salvar | F9: carregar","X: ultimo ataque | Pause: pausar | F11: tela cheia","F2/F3: treino | PageUp/PageDown: trocar mapa","Biblioteca: clique no icone para aprender feitico","Druida: icone do feitico + clique no alvo","Cristal: aproxima o druida para recuperar mana","Mensageiro: mercado negocia; santuario faz alianca","Cartola animada: abrir negociacao recebida","Comercio: icone, quantidade e confirmar","Trocas de tropas: enviar indicados para a Bolsa","Reaper: Ctrl coleta comida e entrega no armazem","Helipede e balao: voo sobre agua e obstaculos","Missao 8: colono ocupa heliporto e recebe balao","Ordem para outra ilha: transporte automatico","Balao: ate cinco passageiros; fila e desembarque","Tropa + clique no balao: embarcar","Balao carregado + clique no terreno: voar e desembarcar","Retratos a bordo: clique para desembarcar essa classe","Esc: cancelar ordem / abrir menu / voltar"};
    int controlsPage;int ControlPageCount=>(ControlLines.Length+11)/12;
    Rectangle ControlPageButton(bool next)=>new(NativeMenuBounds.X+(int)((next?420:200)*MenuScale),NativeMenuBounds.Y+(int)(486*MenuScale),(int)(180*MenuScale),(int)(22*MenuScale));
    string? captureSlotDirectory;int pendingSaveSlot;bool exitAfterSave;string[] slotLabels=Array.Empty<string>();
    SaveSlots SlotStore {
     get {if(captureSlotDirectory!=null)return new SaveSlots(captureSlotDirectory,simulation.World.MapName,shared:true);var researchRoot=PackageLayout.UserRoot(root);var releases=Path.Combine(researchRoot,"releases");var legacy=Directory.Exists(releases)?Directory.GetDirectories(releases).OrderByDescending(d=>Directory.GetLastWriteTimeUtc(d)).Select(d=>Path.Combine(d,"PrototypeSaves")):Enumerable.Empty<string>();return new SaveSlots(captureSlotDirectory??Path.Combine(researchRoot,"UserSaves"),simulation.World.MapName,new[]{saveRoot}.Concat(legacy),shared:true);}
    }
    void VerifyOverwriteCapture(){
     captureSlotDirectory=PackageLayout.FromBase("TestArtifacts","overwrite-"+Guid.NewGuid().ToString("N"));
     OpenSlots(false);ActivateMenu(0);var path=SlotStore.PathFor(1);string old=File.ReadAllText(path);
     if(!File.Exists(path)||pauseMenu.Page!=MenuPage.Save)throw new InvalidDataException("Empty slot must save directly");
     simulation.Step();ActivateMenu(0);
     if(pauseMenu.Page!=MenuPage.ConfirmOverwrite||File.ReadAllText(path)!=old)throw new InvalidDataException("Occupied slot needs confirmation before writing");
     ActivateMenu(0);if(pauseMenu.Page!=MenuPage.Save||File.ReadAllText(path)!=old)throw new InvalidDataException("Cancel must preserve save");
     ActivateMenu(0);ActivateMenu(1);if(File.ReadAllText(path)==old)throw new InvalidDataException("Confirmed replacement must write current game");
     ActivateMenu(0);UpdateMenu(new KeyboardState(Keys.Escape),new MouseState());if(pauseMenu.Page!=MenuPage.Save)throw new InvalidDataException("Escape must cancel replacement");
     ActivateMenu(0);Console.WriteLine("PASS: empty slot, confirmation, cancel, replacement and Escape preserve expected save behavior");
    }
    void OpenSlots(bool load,bool andExit=false){exitAfterSave=andExit;pauseMenu.Show(load?MenuPage.Load:MenuPage.Save);menuStatus="";RefreshSlots();}
    void RefreshSlots()=>slotLabels=Enumerable.Range(1,SaveSlots.Count).Select(SlotStore.Description).Concat(new[]{"Voltar"}).ToArray();
    bool SaveGame(int slot=1){try{SlotStore.Save(simulation,slot);message=menuStatus="Partida salva no slot "+slot+".";RefreshSlots();return true;}catch(Exception e)when(e is IOException or UnauthorizedAccessException){message=menuStatus="Nao foi possivel salvar. A partida continua aberta.";return false;}}
    bool LoadGame(int slot=1){try{string json=SlotStore.Read(slot);using var saveDoc=System.Text.Json.JsonDocument.Parse(json);if(saveDoc.RootElement.ValueKind!=System.Text.Json.JsonValueKind.Object||!saveDoc.RootElement.TryGetProperty("MapName",out var savedMap)||savedMap.ValueKind!=System.Text.Json.JsonValueKind.String)throw new InvalidDataException("Missing saved map");string map=savedMap.GetString()??"";int next=Array.FindIndex(maps,p=>Path.GetFileName(p).Equals(map,StringComparison.OrdinalIgnoreCase));if(next<0)throw new InvalidDataException("Saved map not found");
    var world=LegacyAssets.Map(maps[next]);visuals.Apply(world);CampaignProgress? starting=null;if(saveDoc.RootElement.TryGetProperty("Campaign",out var campaign)&&campaign.ValueKind==System.Text.Json.JsonValueKind.Object){starting=System.Text.Json.JsonSerializer.Deserialize<CampaignProgress>(campaign.GetRawText());starting!.TransitionOpen=false;}var restored=new Simulation(world,starting);restored.Restore(json);
    training=false;mapIndex=next;LoadMap(restored:restored);paused=false;resourceOrders.Cancel();SelectOnly(null);controlGroups.Clear();buildMode=false;constructionMenu=false;researchMenu=false;displayedResearchCount=simulation.MissionResearchCount;foodTargetMode=0;dragStart=null;destination=null;message=menuStatus="Slot "+slot+" carregado. Nenhum personagem selecionado.";return true;}catch(Exception e)when(e is IOException or UnauthorizedAccessException or System.Text.Json.JsonException or InvalidDataException){message=menuStatus="Nao foi possivel carregar. Partida atual preservada.";return false;}}
    void ActivateMenu(int index){
        if(pauseMenu.Page==MenuPage.ConfirmOverwrite){if(index==0){pendingSaveSlot=0;pauseMenu.Show(MenuPage.Save);menuStatus="Substituicao cancelada. Save preservado.";}else if(SaveGame(pendingSaveSlot)){pendingSaveSlot=0;if(exitAfterSave)Exit();else pauseMenu.Show(MenuPage.Save);}return;}
        if(pauseMenu.Page is MenuPage.Save or MenuPage.Load){
         if(index==SaveSlots.Count){if(exitAfterSave){exitAfterSave=false;pauseMenu.Show(MenuPage.Exit);}else pauseMenu.Back();return;}
         if(pauseMenu.Page==MenuPage.Save){if(SlotStore.Existing(index+1)!=null){pendingSaveSlot=index+1;pauseMenu.Show(MenuPage.ConfirmOverwrite);menuStatus="O slot "+pendingSaveSlot+" ja tem uma partida. Deseja substituir?";}else if(SaveGame(index+1)&&exitAfterSave)Exit();}else if(LoadGame(index+1))pauseMenu.Show(MenuPage.Closed);return;
        }
        if(pauseMenu.Page==MenuPage.Restart){if(index==1){simulation.Restart();controlGroups.Clear();buildMode=false;constructionMenu=false;researchMenu=false;displayedResearchCount=simulation.MissionResearchCount;foodTargetMode=0;dragStart=null;destination=null;SelectOnly(null);CenterStart();paused=false;message="Partida reiniciada. Saves existentes preservados.";pauseMenu.Show(MenuPage.Closed);}else pauseMenu.Back();return;}
        if(pauseMenu.Page==MenuPage.Controls){pauseMenu.Back();return;}
        if(pauseMenu.Page==MenuPage.Exit){if(index==0){OpenSlots(false,true);}else if(index==1)Exit();else pauseMenu.Back();return;}
        switch(index){case 0:pauseMenu.Show(MenuPage.Closed);break;case 1:OpenSlots(false);break;case 2:OpenSlots(true);break;case 3:OpenObjectives();break;case 4:pauseMenu.Show(MenuPage.Controls);controlsPage=0;menuStatus="";break;case 5:pauseMenu.Show(MenuPage.Restart);menuStatus="";break;case 6:pauseMenu.Show(MenuPage.Exit);menuStatus="";break;case 7:pauseMenu.Show(MenuPage.Audio);menuStatus="";break;}
    }
    void UpdateMenu(KeyboardState k,MouseState m){
        if(Press(k,Keys.Escape)){if(pauseMenu.Page==MenuPage.ConfirmOverwrite){ActivateMenu(0);return;}if(exitAfterSave&&pauseMenu.Page==MenuPage.Save){exitAfterSave=false;pauseMenu.Show(MenuPage.Exit);}else pauseMenu.Back();return;}
        if(pauseMenu.Page==MenuPage.Audio){UpdateAudioMenu(k,m);return;}
          if(pauseMenu.Page==MenuPage.Objectives){UpdateObjectives(k,m);return;}
        if(pauseMenu.Page==MenuPage.Controls){
         if(Press(k,Keys.Left)||Press(k,Keys.PageUp))controlsPage=Math.Max(0,controlsPage-1);
         if(Press(k,Keys.Right)||Press(k,Keys.PageDown))controlsPage=Math.Min(ControlPageCount-1,controlsPage+1);
         if(m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released){if(ControlPageButton(false).Contains(m.Position)){controlsPage=Math.Max(0,controlsPage-1);return;}if(ControlPageButton(true).Contains(m.Position)){controlsPage=Math.Min(ControlPageCount-1,controlsPage+1);return;}}
        }
        var labels=MenuLabels;
        if(Press(k,Keys.Up)){if(pauseMenu.Page==MenuPage.Main)MoveMainMenu(-1);else pauseMenu.Move(-1,labels.Length);}
        if(Press(k,Keys.Down)){if(pauseMenu.Page==MenuPage.Main)MoveMainMenu(1);else pauseMenu.Move(1,labels.Length);}
        if(Press(k,Keys.Enter)){ActivateMenu(pauseMenu.Choice);return;}
        if(m.LeftButton==ButtonState.Pressed&&oldMouse.LeftButton==ButtonState.Released){for(int i=0;i<labels.Length;i++){var rect=pauseMenu.Page==MenuPage.Controls?MenuRow(9):MenuButton(i);if(rect.Contains(m.Position)){ActivateMenu(i);break;}}}
    }
    void DrawMenu(){if(!pauseMenu.Open)return;
        if(pauseMenu.Page==MenuPage.Main){DrawMainPauseMenu();return;}
        var p=MenuPanel;
        if(pauseMenu.Page==MenuPage.Audio){DrawAudioMenu();return;}
          if(pauseMenu.Page==MenuPage.Objectives){DrawObjectives();return;}
        batch.Draw(white,GraphicsDevice.Viewport.Bounds,Color.Black);batch.Draw(nativeMenu,NativeMenuBounds,Color.White);MenuText(pauseMenu.Page==MenuPage.ConfirmOverwrite?"SUBSTITUIR SAVE?":pauseMenu.Page==MenuPage.Save?"SALVAR PARTIDA":pauseMenu.Page==MenuPage.Load?"CARREGAR PARTIDA":pauseMenu.Page==MenuPage.Controls?"CONTROLES":pauseMenu.Page==MenuPage.Restart?"REINICIAR MISSAO":pauseMenu.Page==MenuPage.Exit?"SAIR DO JOGO":"MENU",246,gold);
        if(pauseMenu.Page==MenuPage.Controls){
            var lines=ControlLines.Skip(controlsPage*12).Take(12).ToArray();

            for(int i=0;i<lines.Length;i++)MenuText(lines[i],280+i*17,Color.White,.64f);
            MenuText("Anterior      "+(controlsPage+1)+" / "+ControlPageCount+"      Proxima",486,gold,.65f);
            MenuText("Voltar",511,gold);
        }else{
            var labels=MenuLabels;for(int i=0;i<labels.Length;i++){var rect=MenuButton(i);bool active=i==pauseMenu.Choice||rect.Contains(Mouse.GetState().Position);MenuText(labels[i],286+i*25,active?gold:Color.White);}
            if(pauseMenu.Page==MenuPage.Restart)MenuText("O progresso nao salvo sera perdido.",465,muted,.7f);
            if(pauseMenu.Page==MenuPage.Exit)MenuText("Deseja salvar antes de fechar?",465,muted,.7f);
        }
        if(menuStatus.Length>0)MenuText(menuStatus,535,Color.LightGreen,.65f);
    }
}
