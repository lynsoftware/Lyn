# Calorie — MAUI-app

Lyn Calorie: lokal-først kaloriapp. .NET MAUI (`Calorie`) + klassebibliotek (`Calorie.Core`).
Lokal SQLite er sannheten på enheten; sync mot backend kommer i Fase 4D.
Roadmap: `C:\Users\fredr\Lyn\Documents\ROADMAP-Calorie-og-DB.md`.

## Arkitektur — MVVM (migrert juli 2026)

Hele appen følger MVVM med streng lagdeling:

```
Calorie.Core (net10.0 — MAUI-FRITT, testbart)
├── Common/
│   ├── Services/                  # INavigationService, IToastService, IAppearanceService
│   │                              #   (interfacene — implementasjonene bor i appens Common/Services/)
│   ├── Constants/                 # NavKeys — Shell-parameternøkler (delt kontrakt)
│   ├── Enums/                     # MealType, GoalMode (delte på tvers av features)
│   └── Extensions/                # MealTypeExtensions
├── Features/<Feature>/            # Models/Services/ViewModels-mønsteret (per feature):
│   ├── Models/                    # data — EF-entiteter OG projeksjoner (DayLog, GoalSettings ...)
│   ├── Services/                  # logikk — store-interface + store + rene regler (MealTypeSuggester)
│   └── ViewModels/                # presentasjon — VM-er + visningsrecords/enums (LibraryItem, DayBar ...)
└── Data/                          # LocalDbContext + migrasjoner (SQLite)

Calorie (MAUI-appen — kun Views)
├── Common/
│   ├── AppRoutes.cs               # Shell-rutenavn (kun kjent her + MauiProgram)
│   ├── Components/PageHeader      # Delt header (Title/ShowLogo/ShowCloseButton/CloseCommand)
│   └── Services/                  # ShellNavigationService, ToastService, AppearanceService
├── Features/<Feature>/{Pages,Components}
├── Infrastructure/                # ThemeService (statisk mekanisme), PreferenceKeys
└── MauiProgram.cs                 # DI + ruteregistrering
```

### Kritiske regler

- **ViewModels bor i `Calorie.Core` og er 100 % MAUI-frie.** Ingen `Page`, `Color`,
  `Preferences`, `Shell`, `IQueryAttributable` i Core. Alt plattformspesifikt går via
  interfaces i `Core/Common`, implementert i appens `Common/Services/`.
- **CommunityToolkit.Mvvm ligger KUN i Calorie.Core** (appen arver transitivt).
  CommunityToolkit.Maui (toasts/snackbar) ligger KUN i appen — aldri i Core.
- **All navigasjon er Shell-ruter.** `Navigation.PushAsync` og konstruktør-callbacks
  (`onSaved`/`onCustomized`) finnes ikke lenger — ikke gjeninnfør dem.
- **Resultater tilbake = Shell-parametere:** avsender kaller
  `INavigationService.GoBackAsync(NavKeys.X, verdi)`, mottakersiden fanger det i
  `ApplyQueryAttributes`. Nøkler er alltid `NavKeys`-konstanter — aldri rå strenger.
  NB retningspar: `MealToCustomize` (inn til byggeren) vs `MealCustomized` (resultat ut).
- **Store-DI (4B-2 pkt. 8 — utført):** storene er instansklasser bak interfaces
  (`ILibraryStore`/`IDayLogStore`/`IGoalStore`/`IStatsStore` — flatt i feature-mappen,
  metode-summaries bor i interfacet), registrert som singletons i `MauiProgram`;
  VM-ene tar dem via konstruktøren. `DayLogStore` tar `IGoalStore` inn (datert
  mål-oppslag). Aldri kall storene statisk igjen.
- **Klokka injiseres:** all "i dag"/klokkeslett-logikk går via `TimeProvider`
  (singleton `TimeProvider.System`; testene bruker `FakeTimeProvider`) — gjelder
  `MainPageViewModel`, `AddLogEntryViewModel`, `StatsViewModel` og `GoalStore`.
  Audit-stempler (`CreatedAtUtc`/`UpdatedAtUtc`) bruker fortsatt `DateTime.UtcNow`.
- **Mengder lagres alltid i gram** — stykk («2 stk») er ren inntastings-konvertering.

### Kos-andel (TreatPercent)

Valgfri kos-grense som PROSENT av dagsmålet: `TreatPercent` (int?, null = av) på
`UserGoalRecord`/backend `UserGoal` — prosent (ikke kcal) så grensen skalerer med
dagsmålet; datert historikk følger målet. Semantikk: kos teller i dagstotalen
(budsjett er linse, aldri sperre). Kos-regnskapet er avledet tilstand på `DayLog`
(`TreatCalories`/`TreatAllowanceCalories`/`TreatRemainingCalories`). Kos er tatt UT
av kcal-budsjettradene i Goals — prosenten er kos-grensens ENE hjem; `MealType.Treat`
skal ikke tilbake i `BudgetOptions`. Fargene `Treat` (innenfor) og `TreatOver` (over)
finnes i begge temaene og brukes konsistent: segmentert bar i `DaySummaryCard`
(sunt/kos-innenfor/kos-over; dagsoverskridelse gjør sunt-delen rød), stablede
ukessøyler i stats (kun `Treat` — bevisst uten over-splitting i små søyler) og
Kos-raden i fordelingen (beholdt i listen så summen er 100 %).
**Tidsregelen: fargen er tidløs, grensen er datert** — kos-segmentet vises for
alle dager med kos-logg, men over-splitting og «kos igjen»-linjen gjelder kun
fra dagen andelen ble satt. Tilbakevirkende grenser er bevisst valgt bort
(konsistens med målhistorikken, statistikk som står seg, sync-LWW).

### Dagslogg og datoer

Hovedsiden eier visningsdatoen (`CurrentDate` i `MainPageViewModel`) — og
**alt på dagsvisningen opererer på den viste dagen**: redigering, sletting og
logging (logge-flyten får `NavKeys.LogDate`; tittelen viser måldatoen ved
bakdatering). Fra sider uten dagskontekst logges til i dag. Fremtiden er
sperret i dato-navigasjonen, så `LoggedDate` kan aldri være frem i tid.

### Side-mønsteret (alle 8 sider følger dette)

```csharp
public partial class XxxPage : ContentPage, IQueryAttributable   // IQueryAttributable ved behov
{
    private readonly XxxViewModel _vm;

    public XxxPage(XxxViewModel vm)          // konstruktør-injeksjon fra DI
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()    // ved behov for (gjen)lasting
    {
        base.OnAppearing();
        _vm.LoadXxxCommand.Execute(null);
    }

    // Oversetter Shell-parametere til typede VM-kall (VM-en kan ikke se MAUI-typer)
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue(NavKeys.X, out var v) && v is T typed)
            _vm.SetX(typed);
    }
}
```

Registrering i `MauiProgram`: `AddTransient` for både VM og side (fersk instans per
navigering), `AddSingleton` for tjenester. `Routing.RegisterRoute` kjøres i
`CreateMauiApp` — IKKE i AppShell (den rebuildes ved tema-/språkbytte, og
dobbeltregistrering av ruter kaster).

### XAML-regler

- **Kompilerte bindinger overalt:** `x:DataType` på siden og i hver `DataTemplate`.
  Feil property-navn i binding = byggefeil (med vilje).
- **Bindinger fra en DataTemplate til VM-en** krever `RelativeSource AncestorType`
  + inline `x:DataType` i binding-uttrykket (malens kontekst er elementet, ikke VM-en):
  `Command="{Binding XxxCommand, Source={RelativeSource AncestorType={x:Type vm:XxxViewModel}}, x:DataType=vm:XxxViewModel}"`.
  NB: Rider viser en falsk «Unable to resolve property»-advarsel på denne syntaksen —
  ignorer den; `dotnet build` er fasiten.
- **Valgt-tilstand = DataTriggers** på bool-flagg fra VM-en (`IsSelected`,
  `IsLimitMode` ...) — aldri fargesetting i code-behind.
- **Dynamiske lister = `BindableLayout`** med visningsrecords/option-objekter fra
  VM-en (chips, budsjettrader, komponentrader, søyler) — aldri bygg views i løkker.
- **`x:Name` kun når code-behind faktisk bruker navnet** — hver gjenværende `x:Name`
  skal ha en View-faglig grunn.

### Custom controls (Views — får ALDRI ViewModels)

`PageHeader`, `DaySummaryCard`, `MealSectionCard`, `BottomNavBar`:
`BindableProperty` for data (`Title`, `Day`, `Section`), command + event side om side
(`CloseCommand`/`CloseClicked`, `AddCommand`/`AddClicked`) — samme mønster som
MAUIs egen `Button`. `BottomNavBar` er foreløpig kun event-basert; sidene
videresender navbar-events til VM-kommandoer i code-behind (4B-2 pkt. 10).

### Mekanisme vs. fasade

Statiske verktøy (`ThemeService`, `AppToast`) er mekanismene; instansklassene bak
Core-interfaces (`AppearanceService`, `ToastService`) er fasadene VM-ene bruker.
Ikke slå dem sammen: mekanismene har flere kunder (kortene leser farger via
`ThemeService.GetColor`), og statiske klasser kan ikke implementere interfaces.

## Gotchas

- **Grid-feil er alltid stille:** `Grid.Row` utenfor `RowDefinitions` klemmes inn
  i siste rad (overlapp, ingen feilmelding), og en `ScrollView` i en `Auto`-rad
  måles til innholdets fulle høyde og scroller aldri. Ved «scroller ikke» /
  «ligger feil»: sjekk at høyeste `Grid.Row` matcher definisjonene og at
  ScrollView bor i `*`-raden.
- **EF: flytt av entitet = namespace-endring i migrasjonsfilene.** Snapshot og
  Designer-filene i `Data/Migrations` refererer entitetene med fullt typenavn som
  STRENG — kompilerer fint etter en flytt, men neste `migrations add` differ mot
  feil navn og genererer søppel. Søk/erstatt gamle namespaces i alle tre filene og
  verifiser med `dotnet ef migrations has-pending-model-changes` (skal si "No changes").
- **EF: navigasjons-oppdagede entiteter med SATT Guid-nøkkel blir Modified, ikke
  Added.** Guid-id-er settes av initialisererne, så barnerader som erstattes
  (`RemoveRange` + nye instanser) MÅ Add-es eksplisitt via DbSet-et — ellers
  genereres UPDATE mot rader som ikke finnes → `DbUpdateConcurrencyException`
  («expected to affect 1 row(s), but actually affected 0»). Rammet både
  `LibraryStore.SaveMealAsync` og `GoalStore.SaveAsync` — se kommentarene der.
- **`DatePicker.Focus()` åpner ikke kalenderdialogen pålitelig på Android.**
  Mønsteret som virker: usynlig `DatePicker` (`Opacity="0"`, IKKE `IsVisible=False`)
  strukket OVER den synlige visningen — native trykk åpner alltid. NB .NET 10:
  `Date`/`NewDate` er nullable (`DateTime?`) — vakt utpakking, og programmatisk
  `Date`-setting fyrer `DateSelected` (guard mot eget synk-kall).
- **`[ObservableProperty]`-kroker navngis mekanisk:** felt `_selectedMealType` →
  property `SelectedMealType` → krok `OnSelectedMealTypeChanged`. Feil navn gir
  «No defining declaration found for implementing declaration of partial method».
- **`NotifyPropertyChangedFor` vs `NotifyCanExecuteChangedFor` — begge kompilerer,
  bare én virker:** knappers enable/disable lytter på `ICommand.CanExecuteChanged`,
  ikke `PropertyChanged`. Property-navn i førstnevnte, kommando-navn i sistnevnte —
  feil valg gir en stille «knappen henger»-bug.
- **Konstruktør-rekkefølge i VM-er:** initialiser lister FØR observables som trigger
  kroker som leser dem (f.eks. `MealTypeOptions` før `SelectedMealType`), og kall
  sync-metoden eksplisitt etterpå (settes verdien til default fyrer ikke kroken).
- **`CollectionView` nullstiller `SelectedItem` ved gjenlasting** og dytter null inn
  via TwoWay-bindingen. Derfor `SelectedListItem` (listens rå valg, null-guard i krok)
  adskilt fra `ActiveItem` (det panelet viser — overlever gjenlasting og kan settes
  fra Tilpass-flyten med varer som ikke finnes i listen).
- **Tema-/språkbytte gjenskaper AppShell** (`AppearanceService.RestartShell`) —
  navigasjonsstacken nullstilles ved bytte. Kjent og akseptert.
- **`BarZoneHeight` (130)** er duplisert i `StatsViewModel` og StatsPage-XAML-malen —
  må endres begge steder (4B-2 pkt. 10).
- **Android Debug-config:** Fast Deployment + symboler i Debug; r8/aab/innbakte
  assemblies KUN i Release-gruppen i `Calorie.csproj`. Ikke flytt dem tilbake globalt.
  Ved rar installasjonsfeil på emulator: `adb uninstall com.lynsoftware.lyncalorie`.
- **Ingen GlobalUsings** — eksplisitte usings per fil (Magees beslutning);
  using-listen skal vise avhengighetene. Slett usings som blir foreldreløse.
- **`PreferenceKeys` skal aldri til Core** — MAUI `Preferences`-API-et finnes kun i appen.

## Tester

`Calorie.Core.Tests` (xUnit + Moq + FluentAssertions + `FakeTimeProvider`, uten
MAUI-workload) — speiler feature-strukturen: `Features/<Feature>/XxxViewModelTests.cs`.
Storene mockes via interfacene; navnekonvensjon `Metode_NårTilstand_SkalOppførsel`
(engelsk: `Method_WhenCondition_ShouldExpectedBehavior`).

```bash
dotnet test Calorie.Core.Tests/Calorie.Core.Tests.csproj
```

NB: `FakeTimeProvider.SetUtcNow` kan ikke stilles bakover — lag en ny instans
per test-case når klokkeslettet varierer (Theory).

## Bygg og kjør

```bash
# Fra Lyn-roten (C:\Users\fredr\Lyn\Lyn):
dotnet build Calorie/Calorie.csproj -f net10.0-android -c Debug     # rask verifisering
dotnet build Calorie/Calorie.csproj -f net10.0-windows10.0.19041.0  # Windows
```

Kjøring/debugging på Android-emulator gjøres fra Rider/VS (Debug-konfigurasjon).
NETSDK1147 om manglende workloads etter VS-oppdatering: `dotnet workload restore Calorie/Calorie.csproj`.
