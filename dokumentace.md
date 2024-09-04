# Dokumentace: Simple Ecosystem Simulation
## Uživatelská část
### 1.1 Cíl a princip
Tento semestr jsem si jako zápočtový program vybral rozšíření mého minulého programu, tedy simulace ekosystému.
V minulém programu byl pouze jeden typ organizmu: poletující buňky s jednoduchým chováním. Zaměřil jsem se tedy na to, aby organizmů v nové verzi bylo více, a aby bylo jejich chování komplexnější.
### 1.2 Změny oproti minulému programu:
Zde je seznam hlavních věcí, které jsem rozšířil nebo zásadně změnil:
•	nahrazení jednoduchých buněk animovanými zajíci a liškami 
•	přepracování systému rozhodování o tom, co má zvíře dělat,
•	zavedení interakce mezi různými zvířaty: lišky loví zajíce a zajíci utíkají (tato změna umožňuje dynamičtější vývoj dědičných vlastností)
•	vylepšení systému těhotenství (předím bylo zrození instantní)
•	zavedení vizualizace dat: vykreslení grafů požadovaných vlastností za běhu simulace
•	obecné zlepšení systému jídla (předtím se stačilo jídla dotknout, aby bylo zkonzumováno, nyní to trvá nějaký čas, a stejný kus jídla může konzumovat i víc entit 
•	 využití pokročilejších konceptů k optimálnějšímu běhu
o	abstraktní třída Animal, ze které dědí Rabbit a Fox,
o	interface IEdible, který implementuje jak třída Food, tak Rabbit,
o	použití delegátů k hledání požadování entit (hledání partnera pro lišky/zajíce, nebo hledání kořisti pro lišky),
o	využití vnořených typů pro těhotenství atd.
o	Linq pro hledání podmnožiny entit, které splňují danou vlastnost, pomocí SQL-like dotazů.
•	Použití 3D modelů vygenerovaných v Blenderu (zvířata, jídlo, prostředí)
### 1.3 Stručný přehled skriptů
Projekt je vypracovaný v Unity 3D a má celkem 11 skriptů. Zde je jejich stručný přehled:
•	Animal.cs
Hlavní skript, který obsahuje abstraktní třídu Animal, ze které dědí třídy Rabbit a Fox. Třída obsahuje chování společné pro tyto třídy jako je stárnutí, rozmnožování, atd.
•	Rabbit.cs, Fox.cs
Obsahuje metody, které overridují abstraktní metody z Animal, které jsou specifické pro druh zvířete, například chování při hledání jídla, lovení kořisti, a prchání před predátorem.
•	Food.cs
Mimo parametry pro jídlo obsahuje také interface IEdible, které definuje kontrakt pro všechny třídy, které mohou být za určitých okolností považovány za jídlo.
•	EntityManager.cs
Účelem tohoto skriptu je spawnování a despawnování entit. Zajišťuje tedy:
•	spawnování jídla, zvířat a prvků prostředí na začátku simulace v zadaných počtech
•	spawnování zvířat po dokončení těhotenství
Jsou v něm uloženy seznamy referencí na zvířata a jídlo.
•	WindowGraph.cs, InputManager.cs
Vykreslování grafů a ostatních informací.
•	CameraMovement.cs (jediný nepsaný mnou, viz konec dokumentace), InputManager.cs
Pohyb s kamerou: buď pomocí myši, nebo automatický orbitující pohyb.
•	DataCollector.cs
Pořizování snapshotů simulace, ve kterých jsou uložené různé informace o počtech a vlastnostech.
•	BaseTraitSettings.cs
Nastavování výchozích hodnot dědičných vlastností.
•	Utilities.cs
Krátký skript pro často používané metody (nejen v tomto projektu).
•	AnimatorScript.cs
Správa animací.

### 1.4 Vlastnosti zvířat
Vlastnosti zvířat, u kterých není přesný význam zřejmý z názvu, jsou podrobněji vysvětleny ve skriptu Animal.cs.
### 1.5 Ovládání za běhu
Kamera
Uživatel může za běhu volně pohybovat s kamerou:
•	Pohyb do stran pomocí prostředního tlačítka
•	Rotace kamery posouváním myši se stisknutým pravým tlačítkem
•	Zoom pomocí scrollování
Grafy
Je možné spustit libovolnou množinu grafů najednou: každému grafu odpovídá jedno tlačítko s číslem, a jeho stisknutím se graf vypne nebo zapne.

### 1.6 Počáteční stav simulace a její spuštění
Zde je seznam počátečních podmínek simulace, které je možné nastavit:
•	počet zajíců, lišek, jídla, a prvků prostředí na začátku programu (v komponentu EntityManager objektu GroundEmpty)
•	průměrné hodnoty a standardní odchylka dědičných vlastností (ve skriptu BaseTraitSettings.cs)
•	interval spawnování jídla
•	nutriční hodnoty každého jedlé entity (jak dloho ji lze konzumovat, kolik přibude hunger za sekundu konzumace)
Nastavení podmínek
Na základě nastavených počátečních podmínek se vývoj simulací drasticky liší. Při nepromyšleném zvolení podmínek často dojde k přemnožení jednoho druhu a vymření druhého. V některých bězích vzniká cyklický vývoj populace, kdy dojde k přemnožení jednoho druhu, načež není dostatek jídla pro udržení tak velké populace, a počet opět rychle klesne. Populační výbuch zajíců je často následován populačním výbuchem lišek a vymřením zajíců.

 
## 2) Programátorská a technická část
### 2.1 Prohlášení o použití open-source objektů 
•	3D modely lišky, zajíce, a jejich animací jsou stažené z internetu (ostatní 3D modely jsou vytvořil sám, např. stromy a jídlo, kameny).
•	Obsah skriptu CameraMovement.cs jsem stáhnul z internetu, protože by mě jeho psaní jen zdrželo, a přitom bych u toho nevyužil žádný důležitý koncept, ani bych se nenaučil nic nového. Takže jsem ho nepočítal do finální souhrnné délky skriptů.
•	Na YouTube existuje video https://youtu.be/r_It_X7v-1E?si=lUKwCZ0rNCCmDCNu, kterým jsem se před asi 7 lety inspiroval k vytváření simulací. Stejně jako v tomto videu jsem zvolil jako zvířata lišky a zajíce, protože lišky mají atraktivní barvu a jejich nejčastější kořistí jsou zající, a navíc jsem jejich dobré 3D modely a animace. Kromě inspirace jsem ale video ani skript k němu přiložený vůbec nepoužil, a video jsem po několika letech zhlédnul až po dokončení programu. 
### 2.2 Jeden frame instance zvířete
Každá instance zvířete má k sobě připojený skript (buď Rabbit.cs nebo Fox.cs), který obsahuje stejnojmennou třídu, která dědí ze třídy Animal. Třída Animal á v sobě Unity built-in metodu Update(), která se volá každý frame. V této metodě se volají následující funkce:
•	HandleAgeing(), která má na starosti, že se zvířatům mění věk a velikost, a pokud věk překročí hranici maximumAge, zemřou.
•	HandleHunger(), která periodicky snižuje hodnotu fieldu hunger, a pokud klesne pod nulu, zvíře zemře.
•	UpdateState() je hlavní metoda, která definuje přechody zvířete ve finite state machine (enum AnimalState), tento stav určuje chování zvířete a bude vysvětlen níže zvlášť.
•	ActAccordingToState() vykoná nějakou elementární akci na základě stavu, který určila metoda UpdateState().
•	ResolvePregnancy() updatuje stav těhotenství pro samice 
•	UpdateAnimationState() podle současné hodnoty AnimalState currentState určí, jaký typ animace bude hrát.
### 2.3 Finite state machine
Každé zvíře má field currentState typu enum AnimalState. Zde je seznam možných stavů zvířat:
•	Idle: nečinné a bez pohybu
•	Roaming: dokud je zvíře v tomto stavu, nastaví si cíl, který je od něj vzdálený maximálně roamingRange, a snaží se ho dosáhnout; když ho dosáhne, nastaví si další, atd.
•	SearchingFood: zvíře hledá jídlo; pomocí metody FindClosestFoodInRange() ho snaží lokalizovat, a pokud se to podaří, nastaví si jeho lokaci jako cíl, pokud ne, zvolí si cíl náhodně ve vzdálenosti roamingRange
•	Hunting (pouze liška): hledá vhodného zajíce pomocí metody FindClosestAnimalSatisfyingRequirements, pokud je ve visionRange, nastavuje si ho jako cíl a snaží se ho ulovit
•	Eating: konzumuje jídlo uložené v targetFood, zvyšuje hunger na základě hodnoty nutritionPerSecond
•	EscapingPredator (pouze zajíc): pokud je v jeho visionRange liška, nastaví si jako svůj cíl bod v opačném směru
•	SearchingPartner: pomocí funkce FindClosestAnimalSatisfyingRequirements se snaží najít vhodného partnera, pokud ho najde, pošle mu request pomocí funkce SendPartnerProposal, a pokud je přijat, nastaví oba si uloží referenci na toho druhého do fieldu partner a nastavení společné pozice, kam budou směřovat
•	GoingToMatingSpot: pohyb do předem domluvené pozice s partnerem
•	WaitingForParther: pokud do dané pozice jeden partner dorazí dříve, čeká na druhého partnera
•	Mating: páření, na jehož konci vznikne nová instance Pregnancy a bude přiřazena samici
### 2.4 Chování na základě finite state machine
Metoda, která je zodpovědná za vykonávání akcí na základě stavu, se jmenuje ActAccordingToState(). Je v ní jeden dlouhý switch statement, který porovnává s hodnotou currentState. Některé akce jsou identické pro zajíce i lišku: například Idle, Roaming, a stavy spojené s rozmnožováním. I když je kód pro tyto akce stejný, chování se stejně bude lišit podle vlastností každé instance. 
Metody, které se liší mezi liškou a zajícem jsou SearchingForFood, protože liška je výhradně masožravá a zajíc je výhradně býložravý, takže se zavolá abstraktní metoda ActOutSearchingFood, která pro lišku spustí AnimalType.Hunting.

### 2.5 Hledání požadovaných entit pomocí delegátů
Během hledání partnera nebo hledání kořisti je třeba najít nejbližší instanci zvířete, která splňuje požadované podmínky. K tomu slouží metoda FindClosestAnimalSatisfyingRequirements, která jako vstup bere generického delegáta Predicate<Animal> MeetsRequirements. Pokud typ situace se tam dosadí buď PartnerRequirements nebo PredatorRequirements.

### 2.6 Systém jídla
Od minula jsem systém jídla úplně přepracoval, protože nově může být pohlíženo i na zajíce jako na jídlo. Zavedl jsem tedy interface IEdible, který implementují třídy Rabbit a Food. Kontrakt tedy definuje některé metody nutné pro to, aby se při jejich konzumaci chovaly instance těchto tříd unifromně. Kontrakt obsahuje mimo jiné následující metody:
•	 GetTimeCanBeEaten(), celkový počet zvířatosekund, během nich může být jídlo konzumováno, než bude vypotřebované
•	GetNutritionPerSecond(), o kolik se za sekundu zvýší hunger zvířat, které ho jedí
•	GetTimeHasBeenEatenTotal(), kolik sekund už jídlo bylo konzumováno
•	TimeEatenSinceLastBite(), jak dlouho uplynulo od minulého framu, kdy zvíře jedlo toto jídlo; 
•	ChangeScaleFood(): jídlo se zmenšuje podle toho, jak dlouho bylo konzumováno; nová velikost jídla se spočítá tak, že se původní velikost vynásobí podílem toho, jak dlouho už jídlo bylo jezeno, a jak dlouho může být jezeno; pokud je tento podíl menší nebo rovný nule, jídlo je snězeno a odstraněno ze scény




### 2.7 Genetika
Zvolil jsem následující vlastnosti, které je možné dědit:
•	PregnancyTime
•	ReproductionCooldown (doba, po kterou se nelze pářit po minulém páření)
•	MaximumAge
•	MaturityAge
•	VisionRange
•	RoamingRange
•	MovementSpeed
Zde je seznam tříd, které mají do činění s genetikou a jejich využití:
•	Třída BaseTraitSettings
Tato umožňuje uživateli nastavit pro každou vlastnost průměrnou hodnotu, a hodnotu standardní odchylky. Z těchto hodnot budou dědičné vlastnosti spočítány zvířatům, které byla instanciována na začátku, a nemají tedy rodiče. Tato třída je statická, protože se její hodnoty nastaví před spuštění simulace, a za běhu už se v ní nic nemění.
•	Třída HereditaryInformation
Obsahuje hodnoty všech dědičných vlastností, každé zvíře má v sobě uloženou jednu instanci.
•	Třída Pregnancy
Každá instance této třídy popisuje jednu instanci probíhajícího těhotenství. Když těhotenství vzniká, metoda této třídy CombineGenes z rodičovských instancí HereditaryInformation spočítá novou instanci HereditaryInformation, která bude nastavena dětem. Metoda CombineGenes bere jako parametr delegáta CalculateGenes, kam se může dosadit libovolná metoda, která z průměrné/standardní hodnoty a hodnoty odchylky spočítá novou hodnotu dané vlastnosti. Ve svém programu tam dosazuji Gaussovo rozdělení.



### 2.8 Vizualizace dat
Zatímco v minulé verzi bylo možné zobrazit jen okamžité hodnoty a počty, v té nové jsem zavedl užitečnější systém vizualizace dat, který navíc vykresluje za běhu grafy. K tomu slouží skripty DataCollector.cs a WindowGraph.cs. DataCollector definuje třídu Snapshot, jejíž instance popisuje stav simulace v konkrétním čase. Uživatel stanoví interval pořizování snapshotů a ty se ukládají do seznamu snapshots.
V konstrukturu třídy Snapshot je logika získávání dat, ve které jsou použité SQL-like dotazy z System.Linq.
### 2.9 Co jsem nestihl
Když jsem psal specifikaci, myslel jsem, že serializace bude relativně snadná. Ukázalo se ale, že Unity nepodporuje serializaci několika objektů, které jsem použil, například abstraktních tříd, a tříd, kde jsou cyklické reference (například při páření mají obě instance uloženou referenci na toho druhého). Refactorování kódu tak, aby serializace byla možná, by byl projekt hodný celého zápočtového programu, a tak jsem se rozhodl, že ji dělat nebudu, zvláště když rozsahem kód nad požadavkem.
Kvůli nedostatku času jsem nestihl naprogramovat popisky na osách grafu. Principiálně by to nebylo těžké z pohledu logiky, ale Unity má poměrně neintuitivní systém User Interface, se kterým jsem v tomto kontextu nepracoval. Zároveň by ta implementace nebyla zajímavá z pohledu C#, jen z pohledu práce s Unity, a tak jsem ji posunul níže na seznam priorit.

### 2.10 Shrnutí
Myslím, že se mi celkově podařilo dosáhnout toho, o co jsem usilovat: kompletní přepracování minulého programu tak, aby byl komplexnější, využíval pokročilejších konceptů, a zároveň byl vizuálně atraktivnější, než ten minulý. Zakončil jsem tím sérii tří zápočtových programů, které se věnovali vývoji organismů:
•	První byl v Pythonu v prostředí pygame, kde buňky byly pouze 2D
•	Druhý už byl v Unity, s

Porovnání
 
Program č. 1 (400 řádků, Python, pygame) 
 
Program č. 2 (900 řádků, Unity) 
 
 
