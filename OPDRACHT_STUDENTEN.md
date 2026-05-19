# Opdracht: WPF Applicatie met MVVM en REST API

## Leerdoelen

Na het voltooien van deze opdracht kun je:
- Een WPF-applicatie opzetten met het MVVM-patroon
- Data Binding toepassen tussen een View en een ViewModel
- `INotifyPropertyChanged` implementeren zodat de UI automatisch wordt bijgewerkt
- Een REST API aanroepen vanuit een WPF-applicatie
- JSON deserializeren naar C#-objecten met Newtonsoft.Json

---

## Achtergrond: Wat is MVVM?

Het **MVVM-patroon** (Model–View–ViewModel) verdeelt een applicatie in drie lagen:

| Laag | Verantwoordelijkheid | Bestand (in dit project) |
|---|---|---|
| **Model** | De datastructuur (wat sla je op?) | `models/Person.cs` |
| **ViewModel** | Logica + data voor de UI | `Viewmodels/MainViewmodel.cs` |
| **View** | Wat de gebruiker ziet | `views/MainWindow.xaml` |

De View "praat" nooit rechtstreeks met het Model. De ViewModel is de brug ertussen.

```
View  <-->  ViewModel  <-->  Model / REST API
 (XAML)     (C# klasse)       (Person.cs / HTTP)
```

---

## Stap 1 — Project aanmaken

1. Open **Visual Studio 2022** of hoger.
2. Kies **Nieuw project** → zoek op **WPF Application**.
3. Stel in:
   - **Projectnaam:** `WpfAppBindings`
   - **Framework:** `.NET 8.0`
4. Klik op **Maken**.

> Het project bevat standaard al een `MainWindow.xaml` en `MainWindow.xaml.cs`.

---

## Stap 2 — Mappenstructuur aanmaken

Maak de volgende mappen aan in je project (rechtsklik op project → Toevoegen → Nieuwe map):

```
WpfAppBindings/
├── models/          ← dataklassen
├── Viewmodels/      ← viewmodel-klassen
└── views/           ← XAML-vensters (verplaats MainWindow hier)
```

Verplaats `MainWindow.xaml` (en `MainWindow.xaml.cs`) naar de map `views/`.

> **Let op:** na het verplaatsen moet je in `MainWindow.xaml` de namespace aanpassen:
> ```xml
> x:Class="WpfAppBindings.MainWindow"
> ```
> Dit blijft hetzelfde omdat de namespace van het project niet verandert.

---

## Stap 3 — NuGet-pakket installeren

We gebruiken **Newtonsoft.Json** om JSON van de REST API te deserializeren.

1. Rechtsklik op het project → **NuGet-pakketten beheren**.
2. Zoek op `Newtonsoft.Json`.
3. Installeer versie **13.0.4** (of nieuwer).

Je `.csproj`-bestand ziet er dan zo uit:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net8.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <UseWPF>true</UseWPF>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
  </ItemGroup>

</Project>
```

---

## Stap 4 — Het Model aanmaken (`Person.cs`)

Maak in de map `models/` een nieuw bestand aan: `Person.cs`.

Een **model** is een eenvoudige dataklasse die beschrijft hoe een object eruitziet. In dit geval een `Person` met een id, naam en leeftijd.

```csharp
namespace WpfAppBindings.models
{
    public class Person
    {
        int _id;
        public int id
        {
            get { return _id; }
            set { _id = value; }
        }

        string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        int _age;
        public int age
        {
            get { return _age; }
            set { _age = value; }
        }

        // Lege constructor nodig voor JSON-deserialisatie
        public Person() { }

        public Person(int id, string name, int age)
        {
            _id = id;
            _name = name;
            _age = age;
        }
    }
}
```

**Waarom een lege constructor?**
Newtonsoft.Json maakt objecten aan via de lege constructor en vult daarna de properties in via de setters.

---

## Stap 5 — Het ViewModel aanmaken (`MainViewmodel.cs`)

Maak in de map `Viewmodels/` een nieuw bestand aan: `MainViewmodel.cs`.

### 5a — De basisstructuur met INotifyPropertyChanged

`INotifyPropertyChanged` is een interface die de UI laat weten dat een property is gewijzigd, zodat de UI zichzelf automatisch kan vernieuwen.

```csharp
using System.ComponentModel;

namespace WpfAppBindings.Viewmodel
{
    public class MainViewmodel : INotifyPropertyChanged
    {
        // Verplicht door INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
```

### 5b — Properties toevoegen

Elke property die de UI moet bijhouden, roept `OnPropertyChanged` aan in de setter:

```csharp
private int? _personsloaded;

public int? PersonsLoaded
{
    get { return _personsloaded; }
    set
    {
        _personsloaded = value;
        OnPropertyChanged(nameof(PersonsLoaded)); // UI wordt bijgewerkt
    }
}
```

> **Vuistregel:** gebruik altijd een private backing field (bijv. `_personsloaded`) en roep in de setter `OnPropertyChanged` aan.

### 5c — ObservableCollection voor lijsten

Een gewone `List<T>` werkt niet goed met WPF-binding omdat de UI niet weet wanneer items worden toegevoegd of verwijderd. Gebruik altijd `ObservableCollection<T>`:

```csharp
using System.Collections.ObjectModel;
using WpfAppBindings.models;

private ObservableCollection<Person>? _persons;

public ObservableCollection<Person>? Persons
{
    get { return _persons; }
    set
    {
        _persons = value;
        OnPropertyChanged(nameof(Persons));
    }
}
```

### 5d — SelectedPerson property

Om bij te houden welke persoon de gebruiker selecteert in de ComboBox:

```csharp
private Person? _selectedperson;

public Person? SelectedPerson
{
    get { return _selectedperson; }
    set
    {
        _selectedperson = value;
        OnPropertyChanged(nameof(SelectedPerson));
    }
}
```

### 5e — Constructor

```csharp
public MainViewmodel()
{
    _personsloaded = 0;
    _persons = new ObservableCollection<Person>();
    getPersons(); // Haal personen op bij het opstarten
}
```

### 5f — REST API aanroepen: getPersons()

```csharp
using Newtonsoft.Json;
using System;
using System.Net.Http;

public void getPersons()
{
    using (var client = new HttpClient())
    {
        try
        {
            // GET-verzoek naar de REST API
            var response = client.GetAsync("http://localhost:8888/api/person").Result;

            if (response.IsSuccessStatusCode)
            {
                var json = response.Content.ReadAsStringAsync().Result;

                // JSON omzetten naar een ObservableCollection<Person>
                Persons = JsonConvert.DeserializeObject<ObservableCollection<Person>>(json);
            }
        }
        catch (Exception ex)
        {
            // Geen verbinding? Gebruik testdata zodat de app toch werkt.
            Console.WriteLine($"Fout bij ophalen personen: {ex.Message}");
            Persons = new ObservableCollection<Person>
            {
                new Person(1, "John Doe", 30),
                new Person(2, "Jane Smith", 25),
                new Person(3, "Bob Johnson", 40)
            };
        }

        PersonsLoaded = Persons?.Count ?? 0;
    }
}
```

> **Tip:** de `try/catch` met testdata is handig tijdens ontwikkeling — de app werkt ook zonder actieve API.

### 5g — REST API aanroepen: addPerson()

```csharp
using System.Text;

public void addPerson(Person person)
{
    using (var client = new HttpClient())
    {
        try
        {
            // Alleen name en age meesturen; de server genereert het id
            var json = JsonConvert.SerializeObject(new { name = person.name, age = person.age });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = client.PostAsync("http://localhost:8888/api/person", content).Result;

            if (response.IsSuccessStatusCode)
            {
                Persons?.Add(person);
                PersonsLoaded = Persons?.Count ?? 0;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Fout bij toevoegen persoon: {ex.Message}");
        }
    }
}
```

### Volledig bestand `MainViewmodel.cs`

```csharp
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Text;
using WpfAppBindings.models;

#nullable enable

namespace WpfAppBindings.Viewmodel
{
    public class MainViewmodel : INotifyPropertyChanged
    {
        private int? _personsloaded;
        private ObservableCollection<Person>? _persons;
        private Person? _selectedperson;

        public MainViewmodel()
        {
            _personsloaded = 0;
            _persons = new ObservableCollection<Person>();
            getPersons();
        }

        public int? PersonsLoaded
        {
            get { return _personsloaded; }
            set { _personsloaded = value; OnPropertyChanged(nameof(PersonsLoaded)); }
        }

        public ObservableCollection<Person>? Persons
        {
            get { return _persons; }
            set { _persons = value; OnPropertyChanged(nameof(Persons)); }
        }

        public Person? SelectedPerson
        {
            get { return _selectedperson; }
            set { _selectedperson = value; OnPropertyChanged(nameof(SelectedPerson)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void getPersons()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = client.GetAsync("http://localhost:8888/api/person").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result;
                        Persons = JsonConvert.DeserializeObject<ObservableCollection<Person>>(json);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fout: {ex.Message}");
                    Persons = new ObservableCollection<Person>
                    {
                        new Person(1, "John Doe", 30),
                        new Person(2, "Jane Smith", 25),
                        new Person(3, "Bob Johnson", 40)
                    };
                }
                PersonsLoaded = Persons?.Count ?? 0;
            }
        }

        public void addPerson(Person person)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var json = JsonConvert.SerializeObject(new { name = person.name, age = person.age });
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = client.PostAsync("http://localhost:8888/api/person", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        Persons?.Add(person);
                        PersonsLoaded = Persons?.Count ?? 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Fout: {ex.Message}");
                }
            }
        }
    }
}
```

---

## Stap 6 — De View bouwen (`MainWindow.xaml`)

### 6a — DataContext instellen in de code-behind

Open `MainWindow.xaml.cs` en koppel het ViewModel aan het venster:

```csharp
using WpfAppBindings.Viewmodel;
using System.Windows;

namespace WpfAppBindings
{
    public partial class MainWindow : Window
    {
        private MainViewmodel _mainViewmodel = new MainViewmodel();

        public MainWindow()
        {
            InitializeComponent();
            // Door DataContext in te stellen kunnen alle XAML-elementen
            // de properties van het ViewModel binden.
            this.DataContext = _mainViewmodel;
        }

        private void Getpersons_Click(object sender, RoutedEventArgs e)
        {
            _mainViewmodel.getPersons();
        }

        private void Addpersons_Click(object sender, RoutedEventArgs e)
        {
            _mainViewmodel.addPerson(new models.Person { name = "New Person Test", age = 30 });
        }
    }
}
```

### 6b — XAML-layout met bindings

```xml
<Window x:Class="WpfAppBindings.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:WpfAppBindings"
        mc:Ignorable="d"
        Title="MainWindow" Height="450" Width="800">
    <Grid>

        <!-- Bovenste sectie: knoppen en teller -->
        <StackPanel>
            <Button Height="100" Margin="10"
                    Content="Get Persons"
                    Click="Getpersons_Click" />

            <!-- Binding naar PersonsLoaded in het ViewModel -->
            <Label Content="{Binding PersonsLoaded, FallbackValue=0}"
                   HorizontalAlignment="Center"
                   FontSize="36" />

            <Button Height="100" Margin="10"
                    Content="Add Person"
                    Click="Addpersons_Click" />
        </StackPanel>

        <!-- Onderste sectie: ComboBox en detailweergave -->
        <StackPanel Orientation="Vertical"
                    HorizontalAlignment="Center"
                    VerticalAlignment="Bottom"
                    Margin="10">

            <!-- ItemsSource bindt de lijst, SelectedItem bindt de selectie -->
            <ComboBox x:Name="cbxPersons"
                      Width="200" Margin="10"
                      DisplayMemberPath="name"
                      SelectedValuePath="id"
                      ItemsSource="{Binding Persons}"
                      SelectedItem="{Binding SelectedPerson}" />

            <!-- Toont details van de geselecteerde persoon -->
            <Label Content="{Binding SelectedPerson.name}"
                   HorizontalAlignment="Center"
                   FontSize="24" />
            <Label Content="{Binding SelectedPerson.age}"
                   HorizontalAlignment="Center"
                   FontSize="24" />
        </StackPanel>

    </Grid>
</Window>
```

### Uitleg van de bindingsyntax

| XAML-attribuut | Betekenis |
|---|---|
| `{Binding PersonsLoaded}` | Koppel aan de property `PersonsLoaded` van de DataContext |
| `FallbackValue=0` | Toon `0` als de binding nog geen waarde heeft |
| `ItemsSource="{Binding Persons}"` | Vul de ComboBox met de lijst `Persons` |
| `DisplayMemberPath="name"` | Toon de `name`-property van elk Person-object |
| `SelectedItem="{Binding SelectedPerson}"` | Sla het geselecteerde item op in `SelectedPerson` |
| `{Binding SelectedPerson.name}` | Gebruik een geneste property (naam van de geselecteerde persoon) |

---

## Stap 7 — Testen zonder REST API

Als je nog geen REST API hebt draaien, vult de `catch`-blok in `getPersons()` automatisch testdata in:

```
John Doe, 30
Jane Smith, 25
Bob Johnson, 40
```

Start de applicatie met **F5** en controleer:
- [x] De teller toont het aantal personen
- [x] De ComboBox toont de namen
- [x] Bij selectie verschijnen naam en leeftijd eronder

---

## Stap 8 — REST API aansluiten (optioneel)

Zorg dat je een REST API draait op `http://localhost:8888/api/person` die:

**GET /api/person** — geeft een JSON-array terug:
```json
[
  { "id": 1, "name": "Alice", "age": 28 },
  { "id": 2, "name": "Bob", "age": 34 }
]
```

**POST /api/person** — ontvangt een JSON-body:
```json
{ "name": "Charlie", "age": 22 }
```

Je kunt een eenvoudige API bouwen met Node.js/Express, ASP.NET Core, of een mock-tool zoals **json-server**.

### Voorbeeld met json-server (Node.js)

```bash
npm install -g json-server
```

Maak een bestand `db.json`:
```json
{
  "person": [
    { "id": 1, "name": "Alice", "age": 28 },
    { "id": 2, "name": "Bob", "age": 34 }
  ]
}
```

Start de server:
```bash
json-server --watch db.json --port 8888 --routes routes.json
```

Maak `routes.json`:
```json
{
  "/api/person": "/person"
}
```

---

## Overzicht van de dataflow

```
[Gebruiker klikt "Get Persons"]
        |
        v
[MainWindow.xaml.cs]
  Getpersons_Click()
        |
        v
[MainViewmodel.cs]
  getPersons()
    → HTTP GET naar API
    → JSON deserialiseren
    → Persons = new ObservableCollection<Person>(...)
    → PersonsLoaded = Persons.Count
        |
        v  (via INotifyPropertyChanged + PropertyChanged event)
        |
        v
[MainWindow.xaml]
  - Label toont bijgewerkte PersonsLoaded
  - ComboBox vult zich met de lijst Persons
```

---

## Veelgemaakte fouten

| Fout | Oorzaak | Oplossing |
|---|---|---|
| UI wordt niet bijgewerkt | `OnPropertyChanged` wordt niet aangeroepen | Roep het aan in elke setter |
| ComboBox blijft leeg | `ItemsSource` of `DataContext` niet ingesteld | Check `this.DataContext = _mainViewmodel` in de constructor |
| JSON kan niet worden geparsed | Properties in het model matchen niet de JSON-velden | Zorg dat de namen exact overeenkomen (hoofdlettergevoelig) |
| App crasht bij opstart | API niet bereikbaar én geen `try/catch` | Voeg testdata toe in de `catch`-blok |
| `ObservableCollection` werkt niet | Verkeerde namespace | Gebruik `System.Collections.ObjectModel` |

---

## Uitbreidingsopdrachten

Als je klaar bent met de basisopdracht, kun je de volgende uitbreidingen proberen:

1. **Verwijder een persoon** — voeg een knop toe die de geselecteerde persoon verwijdert via een DELETE-request.
2. **Voeg een invoerformulier toe** — laat de gebruiker zelf een naam en leeftijd invullen in `TextBox`-elementen vóór het toevoegen.
3. **Laadstatus tonen** — toon een `ProgressBar` of tekst "Laden..." tijdens het ophalen van data.
4. **Foutmelding in de UI** — toon een rode label als de API niet bereikbaar is, in plaats van stil te falen.
5. **Zoekfunctie** — voeg een `TextBox` toe waarmee je de lijst kunt filteren op naam.

---

## Beoordelingscriteria

| Criterium | Punten |
|---|---|
| Model (`Person.cs`) correct geïmplementeerd | 10 |
| ViewModel met `INotifyPropertyChanged` | 20 |
| Data binding in XAML (Label, ComboBox) | 20 |
| REST API GET aanroep werkt (of testdata) | 20 |
| REST API POST aanroep werkt | 15 |
| Foutafhandeling aanwezig | 10 |
| Code is leesbaar en overzichtelijk | 5 |
| **Totaal** | **100** |
